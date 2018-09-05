using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;     // io
using System.Xml;
using System.Xml.Serialization;
using Basic;
using VisualGraph;
using System.Threading;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
namespace ceConfigRun
{
    public partial class Form1 : Form
    {
        private Thread thread_poll = null;
        private System.Windows.Forms.Timer m_VirTimer = null;
        private string ceProjectPath = "";
        public string myProjectPath = "";
        static private List<FormEdit> editFormList = new List<FormEdit>();
        private FormEdit SelectedPage;
        private bool bExitSoftware = false;
        private modbusrtu MasterRTU = new modbusrtu();
        delegate void HandleInterfaceUpdateDelegate(Variable text);
        HandleInterfaceUpdateDelegate InterfaceUpdate;
        delegate void UpdateEveryPage();
        UpdateEveryPage UpdateCurPage;
        private int nCurrentForm = -1;
        private TcpListener myListener = null;
        private int port = 80;  // Web服务器端口,通常为80
        Encoding GBencoder;
        delegate string GetXMLData(string id);
        GetXMLData xmlUpdate;
        //实例化一个设备管理列表
        List<ChannelManage> ChannelList = new List<ChannelManage>();
        public Form1()
        {
            InitializeComponent();
            InterfaceUpdate = new HandleInterfaceUpdateDelegate(UpdateRTUData);
            UpdateCurPage = new UpdateEveryPage(UpdateCurrentPage);
            xmlUpdate = new GetXMLData(GetFormWebXml);
        }
        void UpdateCurrentPage()
        {
            UpdatePage();
        }
        //界面刷新函数
        void UpdateRTUData(Variable v)
        {
            try
            {
                v.OnDataChange(v);
            }
            catch
            {
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
        //关闭当前工程
        public void CloseProject()
        {
            if (ChannelList.Count > 0)
            {
                foreach (ChannelManage ch in ChannelList)
                {
                    foreach (DeviceManage dev in ch.AllDevList)
                    {
                        dev.AllVarList.Clear();
                    }
                    ch.AllDevList.Clear();
                }
                ChannelList.Clear();
            }
            int n=editFormList.Count;
            for (int i=0;i<n;i++)
            {
                editFormList.RemoveAt(i);
            }
            
        }
        private void menuItem2_Click(object sender, EventArgs e)
        {
            if (ceProjectPath != "")
            {
                CloseProject();
            }
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "cePrj Files (*.cePrj)|*.cePrj";
            openFileDialog1.InitialDirectory = "";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ceProjectPath = openFileDialog1.FileName;
                myProjectPath = new FileInfo(ceProjectPath).DirectoryName;
                myProjectPath = myProjectPath + "\\";
                //open solution tree
                openTreeSolution(ceProjectPath);
                //select first page
                SelectedPage = (FormEdit)editFormList[0];
                FormState state = new FormState();
                state.Maximize((FormEdit)editFormList[0]);
                SelectedPage.Show();
                SelectedPage.BringToFront();
                nCurrentForm = 0;
                ////register event
                RegisterEvent();
                ////create virtual device
                CreateVirtualDevice();
                ////create serial device 
                thread_poll = new Thread(new ThreadStart(Poll_Thread));
                thread_poll.Start();   
                ////create web server            
                CreateWebServer();
            }
        }
       public void CreateWebServer()
        {
            //获取本地计算机IP,计算机名称
            IPHostEntry IPHost = Dns.Resolve(Dns.GetHostName());
            GBencoder = Encoding.GetEncoding(0);//"gb2312"---0
            if (IPHost.AddressList.Length > 0)
            {
                IPAddress myIP = IPHost.AddressList[0];
                try
                {
                    //开始监听端口
                    myListener = new TcpListener(myIP, port);
                    myListener.Start();
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Start web server error.");
                    return;
                }
            }
            else
            {
                return;
            }
            //同时启动一个监听进程
            Thread th = new Thread(new ThreadStart(StartListen));
            th.Start();
        }
        public void SendHeader(string sHttpVersion, string sMIMEHeader, int iTotBytes, string sStatusCode, ref Socket mySocket)
        {
            String sBuffer = "";
            if (sMIMEHeader.Length == 0)
            {
                sMIMEHeader = "text/html"; // 默认 text/html
            }
            sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
            sBuffer = sBuffer + "Server: MyWebServer\r\n";
            sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
            SendToBrowser(bSendData, ref mySocket);
        }
        public void SendToBrowser(String sData, ref Socket mySocket)
        {
            SendToBrowser(GBencoder.GetBytes(sData), ref mySocket);
        }
        public void SendToBrowser(Byte[] bSendData, ref Socket mySocket)
        {
            try
            {
                if (mySocket.Connected)
                {
                    mySocket.Send(bSendData, bSendData.Length, 0);
                }
            }
            catch (Exception e)
            {
            }
        }
        public void StartListen()
        {
            int iStartPos = 0;
            String sRequest;
            String sDirName;
            String sRequestedFile;
            String sErrorMessage;
            String sLocalDir;
            String sPhysicalFilePath;
            //虚拟目录
            String sMyWebServerRoot =myProjectPath;
            String sResponse = "";
            while (true)
            {
                //接受新连接
                Socket mySocket = myListener.AcceptSocket();
                if (mySocket.Connected)
                {
                    Byte[] bReceive = new Byte[1024];
                    int i = mySocket.Receive(bReceive, bReceive.Length, 0);
                    //转换成字符串类型
                    string sBuffer = Encoding.ASCII.GetString(bReceive, 0, bReceive.Length);
                    //处理post请求类型
                    if (sBuffer.Substring(0, 4) == "POST")
                    {
                    }
                    //处理"get"请求类型
                    if (sBuffer.Substring(0, 3) == "GET")
                    {
                        // 查找 "HTTP" 的位置
                        iStartPos = sBuffer.IndexOf("HTTP", 1);
                        string sHttpVersion = sBuffer.Substring(iStartPos, 8);
                        // 得到请求类型和文件目录文件名
                        sRequest = sBuffer.Substring(0, iStartPos - 1);
                        sRequest.Replace("\\", "/");
                        //如果结尾不是文件名也不是以"/"结尾则加"/"
                        if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/")))
                        {
                            sRequest = sRequest + "/";
                        }
                        //得带请求文件名
                        iStartPos = sRequest.LastIndexOf("/") + 1;
                        sRequestedFile = sRequest.Substring(iStartPos);
                        //得到请求文件目录
                        sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);
                        //获取虚拟目录物理路径
                        sLocalDir = sMyWebServerRoot;
                        if (sLocalDir.Length == 0)
                        {
                            sErrorMessage = "<H2>错误! 请求的目录不存在...</H2><Br>";
                            SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                            SendToBrowser(sErrorMessage, ref mySocket);
                            mySocket.Close();
                            continue;
                        }
                        if (sRequestedFile.Length == 0)
                        {
                            // 取得请求文件名
                            sRequestedFile = "main.html";
                        }
                        // 取得请求文件类型（设定为text/html）
                        String sMimeType = "text/html";
                        sPhysicalFilePath = sLocalDir + sRequestedFile;
                        if (sRequestedFile.IndexOf(".xml") >= 0)//刷新请求的话，单独处理，此处是刷新的关键所在
                        {
                            string strFormId = sRequestedFile.Substring(0, sRequestedFile.Length - 4);
                            string xml = Invoke(xmlUpdate, new object[] { strFormId }).ToString();//GetFormWebXml(strFormId);
                            string xmlheader = "<?xml version=\"1.0\"?>" + "\r\n" + "<!--Form objects xml,Windows ce make for 1.0 version!-->" + "\r\n";
                            xml = xmlheader + xml;
                            SendHeader(sHttpVersion, sMimeType, xml.Length, " 200 OK", ref mySocket);
                            SendToBrowser(xml, ref mySocket);
                        }
                        else//正常http请求处理
                        {
                            if (File.Exists(sPhysicalFilePath) == false)
                            {
                                sErrorMessage = "<H2>404 错误! 文件不存在...</H2>";
                                SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                                SendToBrowser(sErrorMessage, ref mySocket);
                            }
                            else
                            {
                                int iTotBytes = 0;
                                sResponse = "";
                                FileStream fs = new FileStream(sPhysicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                                BinaryReader reader = new BinaryReader(fs);
                                byte[] bytes = new byte[fs.Length];
                                int read;
                                while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                                {
                                    sResponse = sResponse + Encoding.ASCII.GetString(bytes, 0, read);
                                    iTotBytes = iTotBytes + read;
                                }
                                reader.Close();
                                fs.Close();
                                SendHeader(sHttpVersion, sMimeType, iTotBytes, " 200 OK", ref mySocket);
                                SendToBrowser(bytes, ref mySocket);
                            }
                        }
                    }
                    mySocket.Close();
                } //if(mySocket.Connected)
            } //while(true)
        } //public void StartListen()
     
        //根据请求的formid,这里为page1~n，即对应画面里的名称，获得该page里相关文本的xml格式数据，用以返回给web显示。
        //本程序中，其他图元的相关代码被注释掉了，需要的话，可以根据需要继续完善其他图元的相关信息。
        //返回给客户的XML数据格式为：，其中id为图元中属性id值，建议采用数字作为id
        //<objectlist>
        //<object id="1">
        //<value>12.3</value>
        //</object>
        //<object id="2">
        //<value>45.6</value>
        //</object>
        //</objectlist>
        public string GetFormWebXml(string formid)
        {
            string xml = "<?xml version=\" & chr(34) & \"1.0\" & chr(34) & \"?><empty/></xml>";
            foreach (FormEdit Node in editFormList)
            {
                if (Node.Text.Equals(formid))
                {
                    VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)Node.Controls[0];
                    using (StringWriter sw = new StringWriter())
                    {
                        XmlTextWriter writer = new XmlTextWriter(sw);
                        writer.WriteStartElement("objectlist");
                        int n = drawArea.ObjList.Count();
                        for (int i = 0; i < n; i++)
                        {
                            DrawObject o = drawArea.ObjList[i];
                            switch (o.ObjectType)
                            {
                                case Global.DrawType.DrawText://文本
                                    DrawText m = (DrawText)o;
                                    //xml make
                                    if (m.textName != "")
                                    {
                                        writer.WriteStartElement("object");
                                        writer.WriteAttributeString("id", m.ID.ToString());
                                        writer.WriteStartElement("value");
                                        writer.WriteString(m.CurText);
                                        writer.WriteEndElement();
                                        writer.WriteEndElement();
                                    }
                                    break;
                                /*
                               case 7://图片
                                    DrawPic m7 = (DrawPic)o                                 
                                    writer.WriteStartElement("object");
                                    writer.WriteAttributeString("id", m7.Id);                               
                                    writer.WriteEndElement();
                                    break;*/
                            }
                        }
                        writer.WriteEndElement();
                        writer.Close();
                        xml = sw.ToString();
                        sw.Close();
                    }
                }
            }
            return xml;
        }
        //poll thread to read data from device and refresh page
        private void Poll_Thread()
        {
            while (true)
            {
                if (bExitSoftware)
                    break;
                foreach (ChannelManage ch in ChannelList)
                {
                    if (ch.ChannelName.Equals("串口通道"))
                    {
                        if (ch.bConnected == false)//open serial device
                        {
                            if (MasterRTU.Open(ch.ComPort, (int)ch.BaudRate, (int)ch.Databits, (Parity)ch.Parity, (StopBits)ch.Stopbit))
                            {
                                ch.bConnected = true;
                            }
                            else
                            {
                                ch.bConnected = false;
                            }
                        }
                        foreach (DeviceManage dev in ch.AllDevList)
                        {
                            int conuter = dev.AllVarList.Count;
                            int i = 0, n = 0;
                            for (i = 0; i < conuter; i = i + n)
                            {
                                n = dev.AllVarList[i].readNum;
                                string[] sValue = new string[n];
                                ReadModbusData(MasterRTU, Convert.ToByte(dev.RtuAddr), Convert.ToUInt16(dev.AllVarList[i].Addr), dev.AllVarList[i].mValuetype, (ushort)n, ref sValue);                           
                                for (int j = 0; j < n; j++)
                                {
                                    dev.AllVarList[i + j].Value = sValue[j];
                                    dev.AllVarList[i + j].Datatime = DateTime.Now;
                                    if (dev.AllVarList[i + j].OldValue != dev.AllVarList[i + j].Value)//变化触发
                                    {
                                        dev.AllVarList[i + j].OldValue = dev.AllVarList[i + j].Value;
                                        Variable v = dev.AllVarList[i + j];
                                        Invoke(InterfaceUpdate, new object[] { v });//画面刷新触发事件
                                    }
                                }
                            }
                        }
                        //sleep time,for refresh
                        Thread.Sleep(ch.RefreshTime);
                        Invoke(UpdateCurPage, null);
                    }
                    else 
                    {
                        Thread.Sleep(ch.RefreshTime);
                          Invoke(UpdateCurPage, null);
                    }
                }
            }
        }
        public void CreateVirtualDevice()
        {
            foreach (ChannelManage ch in ChannelList)
            {
                if (ch.ChannelName.Equals("虚拟通道"))
                {
                    m_VirTimer = new System.Windows.Forms.Timer();                
                    m_VirTimer.Interval = ch.RefreshTime;
                    m_VirTimer.Tick += new EventHandler(m_VirTimer_Tick);
                    m_VirTimer.Enabled = true;
                }
            }
        }
        void ReadModbusData(modbusrtu ModbusRtu, byte SlaveAddr, ushort ModbusAddr, DataType datatype, ushort nNumber, ref string[] sValue)
        {
            ushort ntype = (ushort)((ModbusAddr / 10000));
            ushort naddr = (ushort)((ModbusAddr % 10000));
            naddr = (ushort)(naddr - 1);
            switch (ntype)
            {
                case 0://DO
                    ushort[] coils = new ushort[nNumber];
                    bool bcoils = ModbusRtu.SendFc1(SlaveAddr, naddr, nNumber, ref coils);
                    if (bcoils)
                    {
                        for (int i = 0; i < nNumber; i++)
                        {
                            sValue[i] = coils[i].ToString();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < nNumber; i++)
                        {
                            sValue[i] = "0";
                        }
                    }
                    break;
                case 1://DI
                    ushort[] dis = new ushort[nNumber];
                    bool bdis = ModbusRtu.SendFc2(SlaveAddr, naddr, nNumber, ref dis);
                    if (bdis)
                    {
                        for (int i = 0; i < nNumber; i++)
                        {
                            sValue[i] = dis[i].ToString();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < nNumber; i++)
                        {
                            sValue[i] = "0";
                        }
                    }
                    break;
                case 4://AO
                    if (datatype.ToString().Equals("uint16"))
                    {
                        ushort[] registerhold = new ushort[nNumber];
                        bool bhold = ModbusRtu.SendFc3(SlaveAddr, naddr, nNumber, ref registerhold);
                        if (bhold)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = registerhold[i].ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("int16"))
                    {
                        ushort[] registerhold = new ushort[nNumber];
                        bool bhold = ModbusRtu.SendFc3(SlaveAddr, naddr, nNumber, ref registerhold);
                        if (bhold)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = ((short)registerhold[i]).ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("float"))
                    {
                        ushort[] registerhold = new ushort[2 * nNumber];
                        bool bhold = ModbusRtu.SendFc3(SlaveAddr, naddr, (ushort)(2 * nNumber), ref registerhold);
                        if (bhold)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                int intValue = (int)registerhold[i * 2 + 1];
                                intValue <<= 16;
                                intValue += (int)registerhold[i * 2 + 0];
                                sValue[i] = BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0).ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("int32"))
                    {
                        ushort[] registerhold = new ushort[2 * nNumber];
                        bool bhold = ModbusRtu.SendFc3(SlaveAddr, naddr, (ushort)(2 * nNumber), ref registerhold);
                        if (bhold)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                int intValue = (int)registerhold[2 * i + 1];
                                intValue <<= 16;
                                intValue += (int)registerhold[2 * i + 0];
                                sValue[i] = intValue.ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("uint32"))
                    {
                        ushort[] registerhold = new ushort[2 * nNumber];
                        bool bhold = ModbusRtu.SendFc3(SlaveAddr, naddr, (ushort)(2 * nNumber), ref registerhold);
                        if (bhold)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                UInt32 intValue = (UInt32)registerhold[2 * i + 1];
                                intValue <<= 16;
                                intValue += (UInt32)registerhold[2 * i + 0];
                                sValue[i] = intValue.ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    break;

                case 3://AI
                    if (datatype.ToString().Equals("uint16"))
                    {
                        ushort[] registerinput = new ushort[nNumber];
                        bool binput = ModbusRtu.SendFc4(SlaveAddr, naddr, nNumber, ref registerinput);
                        if (binput)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = registerinput[i].ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("int16"))
                    {
                        ushort[] registerinput = new ushort[nNumber];
                        bool binput = ModbusRtu.SendFc4(SlaveAddr, naddr, nNumber, ref registerinput);
                        if (binput)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = ((short)registerinput[i]).ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("float"))
                    {
                        ushort[] registerinput = new ushort[2 * nNumber];
                        bool binput = ModbusRtu.SendFc4(SlaveAddr, naddr, (ushort)(2 * nNumber), ref registerinput);
                        if (binput)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                int intValue = (int)registerinput[2 * i + 1];
                                intValue <<= 16;
                                intValue += (int)registerinput[2 * i + 0];
                                sValue[i] = BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0).ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("int32"))
                    {
                        ushort[] registerinput = new ushort[2 * nNumber];
                        bool binput = ModbusRtu.SendFc4(SlaveAddr, naddr, (ushort)(2 * nNumber), ref registerinput);
                        if (binput)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                int intValue = (int)registerinput[2 * i + 1];
                                intValue <<= 16;
                                intValue += (int)registerinput[2 * i + 0];
                                sValue[i] = intValue.ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    else if (datatype.ToString().Equals("uint32"))
                    {
                        ushort[] registerinput = new ushort[2 * nNumber];
                        bool binput = ModbusRtu.SendFc4(SlaveAddr, naddr, (ushort)(2 * nNumber), ref registerinput);
                        if (binput)
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                UInt32 intValue = (UInt32)registerinput[2 * i + 1];
                                intValue <<= 16;
                                intValue += (UInt32)registerinput[2 * i + 0];
                                sValue[i] = intValue.ToString();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < nNumber; i++)
                            {
                                sValue[i] = "0";
                            }
                        }
                    }
                    break;
            }         
        }
        void m_VirTimer_Tick(object sender, EventArgs e)
        {
            foreach (ChannelManage ch in ChannelList)
            {
                if (ch.ChannelName.Equals("虚拟通道"))
                {
                    foreach (DeviceManage dev in ch.AllDevList)
                    {
                        foreach (Variable var in dev.AllVarList)
                        {
                            if (var.VirtualVarType == 0)
                            {
                                var.Counter++;
                                var.Value = var.Counter;//虚拟一些数据变化
                                var.OnDataChange(var);//人为触发变量变化事件
                            }
                            else if (var.VirtualVarType == 1)
                            {
                                var.Counter++;
                                var.Value = Math.Sin(var.Counter);//虚拟一些数据变化
                                var.OnDataChange(var);//人为触发变量变化事件
                            }
                        }
                    }
                }
            }
            Invoke(UpdateCurPage, null);
        }
        public void UpdatePage()
        {
                VisualGraph.VisualGraph vs = (VisualGraph.VisualGraph)SelectedPage.Controls[0];
                vs.Invalidate();
        }
        //添加图元动作属性的注册
        public void RegisterEvent()
        {
            //遍历每个画面
            int count = editFormList.Count;
            for (int i = 0; i < count; i++)
            {
                VisualGraph.VisualGraph drawarea = (VisualGraph.VisualGraph)editFormList[i].Controls[0];
                int n = drawarea.ObjList.Count();
                //遍历每个画面的图元
                for (int j = 0; j < n; j++)
                {
                    DrawObject o = (DrawObject)drawarea.ObjList[j];
                    //查找关联了变量的属性，并注册动画函数到变量事件
                    if (o.xName != "" || o.yName != "" || o.widthName != "" || o.heightName != "" || o.textName != "" || o.visibleName != "")
                    {
                        foreach (ChannelManage ch in ChannelList)
                        {
                            foreach (DeviceManage dev in ch.AllDevList)
                            {
                                foreach (Variable var in dev.AllVarList)
                                {
                                    if (var.Name.Equals(o.xName) || var.Name.Equals(o.yName) || var.Name.Equals(o.widthName) || var.Name.Equals(o.heightName)
                                        || var.Name.Equals(o.textName) || var.Name.Equals(o.visibleName))
                                    {
                                        var.Datachanged += new Variable.DataEventHandler(o.SetAction);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void openTreeSolution(string path)
        {
            XmlDocument document = new XmlDocument();
            document.Load(path);
            foreach (XmlNode node in document.ChildNodes[0].ChildNodes)
            {
                if (node.ChildNodes != null)
                {
                    ChildNodes(node);
                }
            }
        }

        private void ChildNodes(XmlNode ParentNode)
        {
            foreach (XmlNode no in ParentNode.ChildNodes)
            {
                //for form
                if (ParentNode.Name.Equals("画面组态"))
                {
                    OpenPage(no.Name);
                }
                //for control
                if (ParentNode.Name.Equals("设备组态"))
                {
                    opendriver(no.Name);
                }
                if (no.ChildNodes != null)
                {
                    ChildNodes(no);
                }
            }
        }
        private void opendriver(string name)
        {
            string s = myProjectPath + name + ".dev";
            try
            {
                //using (FileStream fs = new FileStream(s, FileMode.Open))
                //{
                //    if (fs != null)
                //    {
                //        BinaryFormatter BinaryRead = new BinaryFormatter();
                //        ChannelManage ch = (ChannelManage)BinaryRead.Deserialize(fs);
                //        ChannelList.Add(ch);
                //        fs.Close();
                //    }
                //}
                FileStream fs = new FileStream(s, FileMode.Open);
                XmlSerializer mySerializer = new XmlSerializer(typeof(List<ChannelManage>));
                ChannelList = (List<ChannelManage>)mySerializer.Deserialize(fs);
                fs.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void WritetoDevice(string name, object val)
        {
            foreach (ChannelManage ch in ChannelList)
            {
                foreach (DeviceManage dev in ch.AllDevList)
                {
                    foreach (Variable var in dev.AllVarList)
                    {
                        if (var.Name.Equals(name))
                        {
                            WriteModbusData(MasterRTU, Convert.ToByte(dev.RtuAddr), Convert.ToUInt16(var.Addr), var.mValuetype.ToString(), val.ToString());
                            return;
                        }
                    }
                }
            }
        }
        public void DealEventProperty(bool bClick, object sender, EventArgs e)
        {
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)sender;

            //取得鼠标位置
            Point point = new Point(MousePosition.X, MousePosition.Y);
            DrawObject gp = drawArea.ObjList.GetSelectedObject(point);
            if (gp != null)//选中了
            {
                if (bClick)
                {
                    if (gp.Click != "")//deal the click event
                    {
                        string[] strContent = gp.Click.Split('+');
                        if (strContent[0].Equals("打开画面"))
                        {                         
                            try
                            {
                                int n = editFormList.Count;
                                for (int i = 0; i < n; i++)
                                {
                                    if (((FormEdit)editFormList[i]).Text.Equals(strContent[1]))
                                    {
                                        SelectedPage.SendToBack();
                                        FormState state = new FormState();
                                        state.Maximize((FormEdit)editFormList[i]);
                                        ((FormEdit)editFormList[i]).Show();
                                        ((FormEdit)editFormList[i]).BringToFront();                                    
                                        SelectedPage = editFormList[i];                            
                                        nCurrentForm = i;
                                        break;
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error");
                            }                         
                        }
                        else if (strContent[0].Equals("写变量值"))
                        {
                            WritetoDevice(strContent[1], strContent[2]);
                        }
                    }
                }
                else
                {
                    if (gp.DoubleClick != "")//deal the dclick event
                    {
                        string[] strContent = gp.Click.Split('+');
                        if (strContent[0].Equals("打开画面"))
                        {                           
                            try
                            {
                                int n = editFormList.Count;
                                for (int i = 0; i < n; i++)
                                {
                                    if (((FormEdit)editFormList[i]).Text.Equals(strContent[1]))
                                    {
                                        FormState state = new FormState();
                                        state.Maximize((FormEdit)editFormList[i]);
                                        ((FormEdit)editFormList[i]).Show();
                                        ((FormEdit)editFormList[i]).BringToFront();
                                        SelectedPage = editFormList[i];
                                        break;
                                    }
                                }

                            }
                            catch (System.Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error");
                            }                       
                        }
                        else if (strContent[0].Equals("写变量值"))
                        {
                            WritetoDevice(strContent[1], strContent[2]);
                        }
                    }
                }
            }
        }
        bool WriteModbusData(modbusrtu ModbusRtu, byte SlaveAddr, ushort ModbusAddr, string datatype, string sValue)
        {
            ushort ntype = (ushort)(ModbusAddr / 10000);
            ushort naddr = (ushort)(ModbusAddr % 10000);
            bool bok = false;
            naddr = (ushort)(naddr - 1);
            switch (ntype)
            {
                case 0://DO
                    short[] coils = new short[1];
                    if (sValue.Equals("1"))
                    {
                        coils[0] = 1;
                    }
                    else
                    {
                        coils[0] = 0;
                    }
                    bok = ModbusRtu.SendFc5(SlaveAddr, naddr, coils[0]);
                    break;
                case 4://AO
                    if (datatype.Equals("uint16"))
                    {
                        ushort[] registerhold = new ushort[1];
                        registerhold[0] = ushort.Parse(sValue);
                        bok = ModbusRtu.SendFc16(SlaveAddr, naddr, 1, registerhold);
                    }
                    else if (datatype.Equals("int16"))
                    {
                        ushort[] registerhold = new ushort[1];
                        registerhold[0] = (ushort)short.Parse(sValue);
                        bok = ModbusRtu.SendFc16(SlaveAddr, naddr, 1, registerhold);
                    }
                    else if (datatype.Equals("float"))
                    {
                        ushort[] registerhold = new ushort[2];
                        int intValue = int.Parse(sValue);
                        registerhold[1] = (ushort)(intValue >> 16);
                        registerhold[0] = (ushort)intValue;
                        bok = ModbusRtu.SendFc16(SlaveAddr, naddr, 2, registerhold);
                    }
                    else if (datatype.Equals("uint32"))
                    {
                        ushort[] registerhold = new ushort[2];
                        int intValue = int.Parse(sValue);
                        registerhold[1] = (ushort)(intValue >> 16);
                        registerhold[0] = (ushort)intValue;
                        bok = ModbusRtu.SendFc16(SlaveAddr, naddr, 2, registerhold);
                    }
                    else if (datatype.Equals("int32"))
                    {
                        ushort[] registerhold = new ushort[2];
                        int intValue = int.Parse(sValue);
                        registerhold[1] = (ushort)(intValue >> 16);
                        registerhold[0] = (ushort)intValue;
                        bok = ModbusRtu.SendFc16(SlaveAddr, naddr, 2, registerhold);
                    }
                    break;
            }
            return bok;
        }
        private void OpenPage(string pagename)
        {
            string s = myProjectPath + pagename + ".page";
            try
            {

                //open pages form
                VisualGraph.VisualGraph vs = new VisualGraph.VisualGraph();
                vs.Name = pagename;
                vs.Width = 800;
                vs.Height = 480;
                vs.Click += new EventHandler(vs_Click);
                vs.DoubleClick += new EventHandler(vs_DoubleClick);
                vs.MouseDown += new MouseEventHandler(vs_MouseDown);
               vs.MouseUp += new MouseEventHandler(vs_MouseUp);
                //建立监控画面
                FormEdit page = new FormEdit();
                page.Name = pagename;
                page.Text = pagename;
                page.Controls.Add(vs);
                editFormList.Add(page);
                FormState formState = new FormState();
                formState.Maximize(page);
                vs.LoadFromXml(s);
                //设置每个图元的lock和runmode属性
                int count = vs.ObjList.Count();
                for (int i = 0; i < count; i++)
                {
                    DrawObject o = (DrawObject)vs.ObjList[i];
                    o.RunMode = true;
                    o.Lock = true;
                }
                vs.Invalidate();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Exception:" + ex.ToString(), " Open Page error.");
            }
        }

        void vs_MouseUp(object sender, MouseEventArgs e)
        {
           // throw new Exception("The method or operation is not implemented.");
        }

        void vs_MouseDown(object sender, MouseEventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void vs_DoubleClick(object sender, EventArgs e)
        {
            DealEventProperty(false, sender, e);
        }

        void vs_Click(object sender, EventArgs e)
        {
            DealEventProperty(true, sender, e);
        }
        public class WinApi
        {
            [DllImport("coredll.dll", EntryPoint = "GetSystemMetrics")]
            public static extern int GetSystemMetrics(int which);

            [DllImport("coredll.dll")]
            public static extern void
                SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
                             int X, int Y, int width, int height, uint flags);
            private const int SM_CXSCREEN = 0;
            private const int SM_CYSCREEN = 1;
            private static IntPtr HWND_TOP = IntPtr.Zero;
            private const int SWP_SHOWWINDOW = 64; 
            public static int ScreenX
            {
                get { return GetSystemMetrics(SM_CXSCREEN); }
            }
            public static int ScreenY
            {
                get { return GetSystemMetrics(SM_CYSCREEN); }
            }
            public static void SetWinFullScreen(IntPtr hwnd)
            {
                SetWindowPos(hwnd, HWND_TOP, 0, 0, ScreenX, ScreenY, SWP_SHOWWINDOW);
            }
        }

        public class FormState
        {
            private FormWindowState winState;
            private FormBorderStyle brdStyle;
            private bool topMost;
            private Rectangle bounds;
            private bool IsMaximized = false;
            public void Maximize(Form targetForm)
            {
                if (!IsMaximized)
                {
                    IsMaximized = true;
                    Save(targetForm);
                    targetForm.WindowState = FormWindowState.Maximized;
                    targetForm.FormBorderStyle = FormBorderStyle.None;
                    targetForm.TopMost = true;
                    WinApi.SetWinFullScreen(targetForm.Handle);
                }
            }
            public void Save(Form targetForm)
            {
                winState = targetForm.WindowState;
                brdStyle = targetForm.FormBorderStyle;
                topMost = targetForm.TopMost;
                bounds = targetForm.Bounds;
            }
            public void Restore(Form targetForm)
            {
                targetForm.WindowState = winState;
                targetForm.FormBorderStyle = brdStyle;
                targetForm.TopMost = topMost;
                targetForm.Bounds = bounds;
                IsMaximized = false;
            }
        }

    }
}