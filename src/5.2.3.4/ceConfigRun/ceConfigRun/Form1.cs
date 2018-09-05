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
namespace ceConfigRun
{
    public partial class Form1 : Form
    {
        private string ceProjectPath = "";
        private string myProjectPath = "";
        private List<FormEdit> editFormList = new List<FormEdit>();
        //实例化一个设备管理列表
        List<ChannelManage> ChannelList = new List<ChannelManage>();
        public Form1()
        {
            InitializeComponent();
           
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
        private void menuItem2_Click(object sender, EventArgs e)
        {
            if (ceProjectPath != "")
            {
                //CloseProject();
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
                ////register event
                //RegisterEvent();
                ////create virtual device
                //CreateVirtualDevice();
                ////create serial device 
                //thread_poll = new Thread(new ThreadStart(Poll_Thread));
                //thread_poll.Start();
                ////create web server
                //CreateWebServer();
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
                //建立监控画面
                FormEdit page = new FormEdit();
                page.Name = pagename;
                page.Text = pagename;
                page.Controls.Add(vs);
                editFormList.Add(page);
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
                //select first page
                FormEdit edit = (FormEdit)editFormList[0];
                edit.Show();
                edit.BringToFront();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Exception:" + ex.ToString(), " Open Page error.");
            }
        }
    }
}