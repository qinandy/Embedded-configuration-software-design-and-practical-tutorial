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
namespace ConfigRun
{
    public partial class Form1 : Form
    {
        private string ceProjectPath = "";
        private string myProjectPath="";
        //实例化一个设备管理列表
        List<ChannelManage> ChannelList = new List<ChannelManage>();
        public Form1()
        {
            InitializeComponent();
            this.tabMain.Region = new Region(new RectangleF(this.tabPage1.Left, this.tabPage1.Top, this.tabPage1.Width, this.tabPage1.Height));
            CloseProject();
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