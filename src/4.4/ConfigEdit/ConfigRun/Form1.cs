using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Basic;
using System.IO;
using System.Runtime.Serialization;     // io
using System.Runtime.Serialization.Formatters.Binary; // io
using System.Xml;
using VisualGraph;
using System.Threading;
using System.IO.Ports;
namespace ConfigRun
{
    public partial class Form1 : Form
    {
        private Thread thread_poll = null;
        private string ceProjectPath = "";
        private string myProjectPath="";
        private System.Windows.Forms.Timer m_VirTimer = null;
        private bool bExitSoftware=false;
        private modbusrtu MasterRTU=new modbusrtu();
        delegate void HandleInterfaceUpdateDelegate(Variable text);
        HandleInterfaceUpdateDelegate InterfaceUpdate;
        delegate void ShowDebugInfo(string info,int ntype);
        ShowDebugInfo showInfo;
        //实例化一个设备管理列表
        List<ChannelManage> ChannelList = new List<ChannelManage>();
        public Form1()
        {
            InitializeComponent();
            this.tabMain.Region = new Region(new RectangleF(this.tabPage1.Left, this.tabPage1.Top, this.tabPage1.Width, this.tabPage1.Height));
            CloseProject();
            InterfaceUpdate = new HandleInterfaceUpdateDelegate(UpdateRTUData);
            showInfo = new ShowDebugInfo(UpdateDebugInfo);
        }
        //界面调试信息显示
        void UpdateDebugInfo(string str, int n)
        {
            if (n==1)
            {
                this.Errorshow.Text = str;
            }
            else if (n==2)
            {
                this.SendData.Text = str;
            }
            else if (n==3)
            {
                this.RecvData.Text = str;
            }
            else if (n==4)
            {
                UpdatePage();
            }

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
        public void CreateVirtualDevice()
        {
            foreach (ChannelManage ch in ChannelList)
            {
                if (ch.ChannelName.Equals("虚拟通道"))
                {
                    m_VirTimer = new System.Windows.Forms.Timer();
                    m_VirTimer.Enabled = true;
                    m_VirTimer.Interval = ch.RefreshTime;
                    m_VirTimer.Tick += new EventHandler(m_VirTimer_Tick);
                    m_VirTimer.Start();
                }
            }
        }
        public void UpdatePage()
        {
            VisualGraph.VisualGraph vs = (VisualGraph.VisualGraph)tabMain.SelectedTab.Controls[0];
            vs.Invalidate();
        }
        //虚拟串口通道的虚拟驱动定时器
        void m_VirTimer_Tick(object sender, EventArgs e)
        {          
            foreach (ChannelManage ch in ChannelList)
            {
                if (ch.ChannelName.Equals("虚拟通道"))
                {
                    foreach(DeviceManage dev in ch.AllDevList)
                    {
                        foreach (Variable var in dev.AllVarList)
                        {
                            if (var.VirtualVarType==0)
                            {
                                var.Counter++;
                                var.Value = var.Counter;//虚拟一些数据变化
                                var.OnDataChange(var);//人为触发变量变化事件
                            }
                            else if (var.VirtualVarType==1)
                            {
                                  var.Counter++;
                                  var.Value = Math.Sin(var.Counter);//虚拟一些数据变化
                                  var.OnDataChange(var);//人为触发变量变化事件
                            }
                        }
                    }
                }
            }
            UpdatePage();
        }
        //打开     
        private void OpenSolution_Click(object sender, EventArgs e)
        {
            if (ceProjectPath != "")
            {
                if (DialogResult.No == MessageBox.Show("工程已经存在，确实要新打开这个工程吗？",
               "重要提示", MessageBoxButtons.YesNo))
                {
                    return;
                }
                CloseProject();           
            }
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "cePrj Files (*.cePrj)|*.cePrj";
            openFileDialog1.InitialDirectory = "";
            openFileDialog1.Title = "打开工程文件";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ceProjectPath = openFileDialog1.FileName;
                myProjectPath = new FileInfo(ceProjectPath).DirectoryName;
                myProjectPath = myProjectPath + "\\";
                //open solution tree
                openTreeSolution(ceProjectPath);
                //register event
                RegisterEvent();
                //create virtual device
                CreateVirtualDevice();
                //create serial device 
                thread_poll = new Thread(new ThreadStart(Poll_Thread));
                thread_poll.Start();
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
                    if (datatype.Equals("uint16"))
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
                    else if (datatype.Equals("int16"))
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
                    else if (datatype.Equals("float"))
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
                    else if (datatype.Equals("int32"))
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
                    else if (datatype.Equals("uint32"))
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
                    if (datatype.Equals("uint16"))
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
                    else if (datatype.Equals("int16"))
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
                    else if (datatype.Equals("float"))
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
                    else if (datatype.Equals("int32"))
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
                    else if (datatype.Equals("uint32"))
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
            Invoke(showInfo, new object[] { ModbusRtu.modbusStatus,1 });
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
        //poll thread to read data from device and refresh page
        private void Poll_Thread()
        {
            while (true)
            {
                if (bExitSoftware)
                    break;
                foreach (ChannelManage ch  in ChannelList)
                {
                    if (ch.ChannelName.Equals("串口通道"))
                    {
                        if (ch.bConnected==false)//open serial device
                        {
                            if (MasterRTU.Open(ch.ComPort, (int)ch.BaudRate, (int)ch.Databits, (Parity)ch.Parity, (StopBits)ch.Stopbit)) 
                           {
                                ch.bConnected=true;
                           }
                           else
                           {
                               ch.bConnected=false;
                           }
                           Invoke(showInfo, new object[] { MasterRTU.modbusStatus, 1 });
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
                                Invoke(showInfo, new object[] { MasterRTU.modbusStatus, 1 });
                                Invoke(showInfo, new object[] { MasterRTU.SendData, 2});
                                Invoke(showInfo, new object[] { MasterRTU.RecvData, 3});
                                for (int j = 0; j < n; j++)
                                {
                                    dev.AllVarList[i + j].Value = sValue[j];
                                    dev.AllVarList[i + j].Datatime = DateTime.Now;
                                    if ( dev.AllVarList[i + j].OldValue != dev.AllVarList[i + j].Value)//变化触发
                                    {
                                         dev.AllVarList[i + j].OldValue = dev.AllVarList[i + j].Value;
                                         Variable v = dev.AllVarList[i + j];
                                         Invoke(InterfaceUpdate, new object[] { v });//画面刷新触发事件
                                    }
                                }
                          }
                      }
                        //sleep time,for refresh
                      //Thread.Sleep(ch.RefreshTime);
                      Invoke(showInfo, new object[] { "update", 4 });
                    }
                }
            }
        }
        //添加图元动作属性的注册
        public void RegisterEvent()
        {
            //遍历每个画面
            int count = tabMain.TabPages.Count;
            for (int i = 0; i < count;i++ )
            {
                VisualGraph.VisualGraph drawarea = (VisualGraph.VisualGraph)tabMain.TabPages[i].Controls[0];
                int n = drawarea.ObjList.Count();
                //遍历每个画面的图元
                for (int j = 0; j < n;j++ )
                {
                        DrawObject o = (DrawObject)drawarea.ObjList[j];          
                       //查找关联了变量的属性，并注册动画函数到变量事件
                        if (o.xName!="" || o.yName!="" || o.widthName!="" || o.heightName!="" || o.textName!="" || o.visibleName!="")
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
            XmlDocument document = new XmlDataDocument();
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
                using (FileStream fs = new FileStream(s, FileMode.Open))
                {
                    if (fs != null)
                    {
                        BinaryFormatter BinaryRead = new BinaryFormatter();
                        ChannelManage ch = (ChannelManage)BinaryRead.Deserialize(fs);
                        ChannelList.Add(ch);
                        fs.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void OpenPage(string pagename)
        {
            string s = myProjectPath + pagename + ".page";
            try
            {
                using (FileStream fs = new FileStream(s, FileMode.Open))
                {
                    if (fs != null)
                    {
                        //open pages form
                        VisualGraph.VisualGraph vs = new VisualGraph.VisualGraph();
                        vs.Name = pagename;
                        vs.Width = 800;
                        vs.Height = 480;
                        vs.MouseClick += new MouseEventHandler(vs_MouseClick);
                        vs.MouseDoubleClick += new MouseEventHandler(vs_MouseDoubleClick);
                        TabPage page = new TabPage();
                        page.Name = pagename;
                        page.Text = pagename;
                        page.Tag = pagename;
                        page.Controls.Add(vs);
                        tabMain.TabPages.Add(page);
                        tabMain.SelectTab(pagename);
                        BinaryFormatter BinaryRead = new BinaryFormatter();
                        vs.ObjList = (ObjList)BinaryRead.Deserialize(fs);
                        //设置每个图元的lock和runmode属性
                        int count = vs.ObjList.Count();
                        for (int i = 0; i < count;i++ )
                        {
                            DrawObject o = (DrawObject)vs.ObjList[i];
                            o.RunMode = true;
                            o.Lock = true;
                        }
                        vs.BackGroundColor = (Color)BinaryRead.Deserialize(fs); ;
                        fs.Close();
                        vs.Invalidate();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Exception:" + ex.ToString(), " Save Page error.");
            }
        }
        public void WritetoDevice(string name ,object val)
        {

        }
        public void DealEventProperty(bool bClick, object sender, MouseEventArgs e)
        {
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)sender;
            if (e.Button == MouseButtons.Left)
            {
                //取得鼠标位置
                Point point = new Point(e.X, e.Y);
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
                                tabMain.SelectTab(strContent[1]);
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
                                tabMain.SelectTab(strContent[1]);
                            }
                            else if (strContent[0].Equals("写变量值"))
                            {
                                WritetoDevice(strContent[1], strContent[2]);
                            }
                        }
                    }
                  
                }
            }
        }
        void vs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DealEventProperty(true, sender,e);
        }

        void vs_MouseClick(object sender, MouseEventArgs e)
        {
            DealEventProperty(false, sender,e);
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
            if (tabMain.TabPages.Count > 0)
            {
                int j = tabMain.TabPages.Count;
                for (int i = 0; i < j; i++)
                {
                    tabMain.TabPages.RemoveAt(i);
                }
            }
        }

        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
     
    }
}