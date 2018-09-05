using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGraph;
using System.IO;
using System.Runtime.Serialization;     // io
using System.Runtime.Serialization.Formatters.Binary; // io
using System.Xml;
using Basic;
using System.Drawing.Imaging;
namespace ConfigEdit
{
    public partial class Form1 : Form
    {
        private XmlDocument doc = null;
        private string myProjectPath = "";
        private string ceProjectPath;
        private bool  bModifiedFalg = false;
        //实例化一个设备管理列表
        List<ChannelManage> ChannelList = new List<ChannelManage>();
        public Form1()
        {
            InitializeComponent();
            ceProjectPath = "";
            this.tabMain.Region = new Region(new RectangleF(this.tabPage1.Left, this.tabPage1.Top, this.tabPage1.Width, this.tabPage1.Height));
            doc = new XmlDocument();
            //NewPage("Page1", Color.White, 800, 480);
            this.treeView1.ExpandAll();
            this.treeView1.ItemDrag += new ItemDragEventHandler(treeView1_ItemDrag);
            this.treeView1.MouseDown += new MouseEventHandler(treeView1_MouseDown);
            this.propertyGridEx1.PropertyValueChanged += new PropertyValueChangedEventHandler(propertyGridEx1_PropertyValueChanged);
            this.propertyGridEx2.PropertyValueChanged += new PropertyValueChangedEventHandler(propertyGridEx2_PropertyValueChanged);
            this.treeExplore.MouseClick +=new MouseEventHandler(treeExplore_MouseClick);
            this.treeExplore.MouseDoubleClick += new MouseEventHandler(treeExplore_MouseDoubleClick);
            CloseProject();
          
        }

        void treeExplore_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeExplore.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    if (CurrentNode.Text == "虚拟通道" || CurrentNode.Text=="串口通道")
                    {
                        DeviceSetForm form = new DeviceSetForm();
                        foreach (ChannelManage ch in ChannelList)
                        {
                            if (ch.ChannelName==CurrentNode.Text)
                            {
                                form.Com = ch.ComPort;
                                form.Baud = ch.BaudRate;
                                //省略数据位等参数
                                form.nRefresh =ch.RefreshTime ;
                                if (form.ShowDialog() == DialogResult.OK)
                                {
                                    ch.ComPort= form.Com;
                                    ch.BaudRate = form.Baud;
                                    ch.Databits = 8;
                                    ch.Stopbit = 1;
                                    ch.Parity = 0;
                                    ch.RefreshTime = form.nRefresh;
                                    break;
                                }
                            }
                        }
                       
                    }
                 
                    if (CurrentNode.Parent!=null)
                    {
                        if (CurrentNode.Parent.Text.Contains("虚拟通道") || CurrentNode.Parent.Text.Contains("串口通道"))//设备选择
                        {
                            DevVarForm form = new DevVarForm();
                            foreach (ChannelManage ch in ChannelList)
                            {
                                foreach (DeviceManage dev in ch.AllDevList)
                                {
                                    if (dev.DeviceName==CurrentNode.Text)
                                    {
                                        form.DevList = dev;
                                        break;
                                    }
                                }
                            }
                            if (  form.ShowDialog()==DialogResult.OK)
                            {
                                foreach (ChannelManage ch in ChannelList)
                                {
                                    foreach (DeviceManage dev in ch.AllDevList)
                                    {
                                        if (dev.DeviceName == CurrentNode.Text)
                                        {
                                           dev.DeviceName = form.DevList.DeviceName;
                                           dev.AllVarList = form.DevList.AllVarList;
                                           dev.RtuAddr = form.DevList.RtuAddr;
                                            break;
                                        }
                                    }
                                }                               
                            }
                            else
                            {

                            }
                            form.Close();
                        }
                        if (CurrentNode.Text.Contains("page"))//说明你选中了画面名称了
                        {
                            int i=tabMain.TabPages.Count;
                            for(int j=0;j<i;j++)
                            {
                                if (tabMain.TabPages[j].Text.Equals(CurrentNode.Text))
                                {
                                    tabMain.SelectedTab=tabMain.TabPages[j];
                                    string strCurPageName = "当前画面名：" + CurrentNode.Text;
                                    label1.Text = strCurPageName;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        void treeExplore_MouseClick(object sender, MouseEventArgs e)
        {
            if (myProjectPath == "")
                return;
            if (e.Button == MouseButtons.Right)//判断你点的是不是右键
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeExplore.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    switch (CurrentNode.Text)//根据不同节点显示不同的右键菜单，当然你可以让它显示一样的菜单
                    {
                        case "画面组态":
                            CurrentNode.ContextMenuStrip = contextMenuStripNew;
                            break;                 
                        case "设备组态":
                            CurrentNode.ContextMenuStrip = contextMenuStripDev;
                            break;
                        case "虚拟通道":
                        case "串口通道":
                            CurrentNode.ContextMenuStrip = contextDevMenu;
                            break;
                    }
                    treeExplore.SelectedNode = CurrentNode;//选中这个节点
                    //如果不是主节点，而是子节点，这里是画面的子节点
                    if (CurrentNode.Parent != null)
                    {
                        //全部驱动
                      if (CurrentNode.Parent.Text == "虚拟通道"  || CurrentNode.Parent.Text == "串口通道")
                        {
                           CurrentNode.ContextMenuStrip =contextDelDev;
                        }                  
                    }
                }
            }    
        }
        public void closeTree()
        {
            foreach (TreeNode tn in treeExplore.Nodes)
            {
                tn.Remove();
            }
        }
        public void initTree()
        {
            treeExplore.Nodes.Clear();
            TreeNode newnode1 = new TreeNode("画面组态");
            treeExplore.Nodes.Add(newnode1);
            TreeNode newnode2 = new TreeNode("设备组态");
            treeExplore.Nodes.Add(newnode2);
            treeExplore.ExpandAll();
        }
        public void saveTreeSolution(string path)
        {
            doc.LoadXml("<cePrj></cePrj>");
            XmlNode root = doc.SelectSingleNode("cePrj");
            //遍历树控件，存储树叶
            foreach (TreeNode tn in treeExplore.Nodes)
            {
                XmlElement Test = doc.CreateElement(tn.Text);
                root.AppendChild(Test);
                if (tn.Nodes != null)
                {
                    ChildNods(tn, Test);
                }
            }
            doc.Save(path);
        }
        private void ChildNods(TreeNode ParentNode, XmlElement Test)
        {
            foreach (TreeNode tn in ParentNode.Nodes)
            {
                try
                {
                    XmlElement Ts = doc.CreateElement(tn.Text);
                    Test.AppendChild(Ts);
                    if (tn.Nodes != null)
                    {
                        ChildNods(tn, Ts);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public void openTreeSolution(string path)
        {
            treeExplore.Nodes.Clear();
            XmlDocument document = new XmlDataDocument();
            document.Load(path);
            foreach (XmlNode node in document.ChildNodes[0].ChildNodes)
            {
                TreeNode tn = treeExplore.Nodes.Add(node.Name);
                if (node.ChildNodes != null)
                {
                    ChildNodes(node, tn);
                }
            }
            treeExplore.ExpandAll();
        }

        private void ChildNodes(XmlNode ParentNode, TreeNode tvNode)
        {
            foreach (XmlNode no in ParentNode.ChildNodes)
            {
                TreeNode tn = tvNode.Nodes.Add(no.Name);
                //for form
                if (tn.Parent.Text.Equals("画面组态"))
                {               
                        OpenPage(no.Name);
                }
                //for control
                if (tn.Parent.Text.Equals("设备组态"))
                {
                    opendriver(no.Name);
                }
                if (no.ChildNodes != null)
                {
                    ChildNodes(no, tn);
                }
            }
        }
        private void opendriver(string name)
        {
            string s = myProjectPath + name+ ".dev";
            try
            {
                using (FileStream fs = new FileStream(s, FileMode.Open))
                {
                    if (fs != null)
                    {
                        BinaryFormatter BinaryRead = new BinaryFormatter();
                        ChannelManage ch  = (ChannelManage)BinaryRead.Deserialize(fs);
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
        //事件属性参数值变化响应函数
        void propertyGridEx2_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
             TabPage page = tabMain.SelectedTab;//获取当前画面
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            if (drawArea.ObjList.SelectionCount > 0)
            {
                //获得当前对象
                DrawObject obj = drawArea.ObjList.GetSelectedObject(0);
                switch (e.ChangedItem.Label.ToString())
                {
                    case "鼠标单击":
                        obj.Click = e.ChangedItem.Value.ToString();
                        break;
                    case "鼠标双击":
                        obj.DoubleClick = e.ChangedItem.Value.ToString();
                        break;
                }
            }
        }
        //动作属性参数值变化事件响应函数
        void propertyGridEx1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            TabPage page = tabMain.SelectedTab;//获取当前画面
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            if (drawArea.ObjList.SelectionCount > 0)
            {
                //获得当前对象
                DrawObject obj = drawArea.ObjList.GetSelectedObject(0);
                switch (e.ChangedItem.Label.ToString())//根据改变的动作属性值修改图元对应的动作属性名称
                {
                    case "可见":
                        obj.visibleName = e.ChangedItem.Value.ToString();
                        break;
                    case "X位置":
                        obj.xName = e.ChangedItem.Value.ToString();
                        break;
                    case "Y位置":
                        obj.yName = e.ChangedItem.Value.ToString();
                        break;
                    case "宽度":
                        obj.widthName = e.ChangedItem.Value.ToString();
                        break;
                    case "高度":
                        obj.heightName = e.ChangedItem.Value.ToString();
                        break;
                    case "文本":
                        obj.textName = e.ChangedItem.Value.ToString();
                        break;
                    case "文本格式":
                        obj.Format = e.ChangedItem.Value.ToString();
                        break;
                }
            }
        }
        void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//判断你点的是不键
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeView1.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    treeView1.SelectedNode = CurrentNode;
                }
            }
        }
        void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            //判断是否是鼠标右键按动
            if (e.Button == MouseButtons.Right) return;

            string str = treeView1.SelectedNode.Text;
            if (str == "") return;
            GetActivedObjectType(str);
            //对组件中的字符串开始拖放操作
            treeView1.DoDragDrop(str, DragDropEffects.Copy | DragDropEffects.Move);
        }
        protected void GetActivedObjectType(String s)
        {
            if (tabMain.TabPages.Count==0)
            {
                return;
            }
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            if (s == null) drawArea.ActivedObjType = Global.DrawType.POINTER;
            if (s == "Line") drawArea.ActivedObjType = Global.DrawType.DrawLine;
            if (s == "Retangle") drawArea.ActivedObjType = Global.DrawType.DrawRectangle;
            if (s == "Ellipse") drawArea.ActivedObjType = Global.DrawType.DrawEllipse;
            if (s == "Text") drawArea.ActivedObjType = Global.DrawType.DrawText;
            if (s == "Picture") drawArea.ActivedObjType = Global.DrawType.DrawPic;

        }
        //新建一个page
        private void NewPage(string pagename, Color back, int width, int height)
        {
            VisualGraph.VisualGraph vs = new VisualGraph.VisualGraph();
            vs.Name = pagename;
            vs.Width = width;
            vs.Height = height;
            vs.BackGroundColor= back;
            vs.MouseClick += new MouseEventHandler(vs_MouseClick);
            //vs.ResizeCanvase();
            TabPage page = new TabPage();
            page.Name = pagename;
            page.Text = pagename;
            page.Tag = pagename;
            page.Controls.Add(vs);
            tabMain.TabPages.Add(page);
            tabMain.SelectTab(pagename);
        }

        void vs_MouseClick(object sender, MouseEventArgs e)
        {
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)sender;
            if (e.Button == MouseButtons.Left)
            {
                //用户选择的是图形对象
                if (drawArea.ObjList.SelectionCount > 0)
                {
                    //添加基本属性
                    DrawObject obj = drawArea.ObjList.GetSelectedObject(0);
                    this.propertyGrid1.SelectedObject = obj;
                    //添加动作属性,根据选中的图元获取其图元类型，并按图元类型进行相应动作属性的添加。
                    if (obj.ObjectType == Global.DrawType.DrawLine || obj.ObjectType == Global.DrawType.DrawRectangle || obj.ObjectType == Global.DrawType.DrawEllipse || obj.ObjectType == Global.DrawType.DrawPic)
                   {
                       propertyGridEx1.ShowCustomProperties = true;
                       propertyGridEx1.Item.Clear();
                       propertyGridEx1.Item.Add("X位置", obj.xName, false, "位置", "水平位置", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("Y位置", obj.yName, false, "位置", "垂直位置", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("宽度", obj.widthName, false, "尺寸", "水平尺寸", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("高度", obj.heightName, false, "尺寸", "垂直尺寸", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("可见", obj.visibleName, false, "视觉", "是否可见", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Refresh();
                        //添加事件属性
                       propertyGridEx2.ShowCustomProperties = true;
                       propertyGridEx2.Item.Clear();
                       propertyGridEx2.Item.Add("鼠标单击", obj.Click, false, "操作", "鼠标单击", true);
                       propertyGridEx2.Item[propertyGridEx2.Item.Count - 1].OnClick += this.CustomEventItem_OnClick;
                       propertyGridEx2.Item.Add("鼠标双击", obj.DoubleClick, false, "操作", "鼠标双击", true);
                       propertyGridEx2.Item[propertyGridEx2.Item.Count - 1].OnClick += this.CustomEventItem_OnClick;
                       propertyGridEx2.Refresh();
                   }
                   else if (obj.ObjectType==Global.DrawType.DrawText)
                   {
                       propertyGridEx1.ShowCustomProperties = true;
                       propertyGridEx1.Item.Clear();
                       propertyGridEx1.Item.Add("X位置", obj.xName, false, "位置", "水平位置", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("Y位置", obj.yName, false, "位置", "垂直位置", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("宽度", obj.widthName, false, "尺寸", "水平尺寸", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("高度", obj.heightName, false, "尺寸", "垂直尺寸", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("可见", obj.visibleName, false, "视觉", "是否可见", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       //propertyGridEx1.Item.Add("填充色", sfillName, false, "填充色", "填充色", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("文本", obj.textName, false, "文本", "文本", true);
                       propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].OnClick += this.CustomActionItem_OnClick;
                       propertyGridEx1.Item.Add("文本格式", obj.Format, false, "文本", "文本格式", true);
                       propertyGridEx1.Refresh();
                       //添加事件属性
                       propertyGridEx2.ShowCustomProperties = true;
                       propertyGridEx2.Item.Clear();
                       propertyGridEx2.Item.Add("鼠标单击", obj.Click, false, "操作", "鼠标单击", true);
                       propertyGridEx2.Item[propertyGridEx2.Item.Count - 1].OnClick += this.CustomEventItem_OnClick;
                       propertyGridEx2.Item.Add("鼠标双击", obj.DoubleClick, false, "操作", "鼠标双击", true);
                       propertyGridEx2.Item[propertyGridEx2.Item.Count - 1].OnClick += this.CustomEventItem_OnClick;
                       propertyGridEx2.Refresh();
                   }
                }
                else//图元库控件画面
                {
                    this.propertyGrid1.SelectedObject = null;//drawArea;
                }
            }
        }
        //动作属性窗口鼠标单击响应
        private object CustomActionItem_OnClick(object sender, EventArgs e)
        {
            PropertyGridEx.CustomProperty prop = (PropertyGridEx.CustomProperty)((PropertyGridEx.CustomProperty.CustomPropertyDescriptor)sender).CustomProperty;
            VarForm form = new VarForm();
            form.ChannelTmp = ChannelList;
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                   prop.Value = form.VarExpress;
             }
            form.Dispose();
            return prop.Value;
        }
        //事件属性窗口鼠标单击响应
        private object CustomEventItem_OnClick(object sender, EventArgs e)
        {
            PropertyGridEx.CustomProperty prop = (PropertyGridEx.CustomProperty)((PropertyGridEx.CustomProperty.CustomPropertyDescriptor)sender).CustomProperty;
            EventSet form = new EventSet();
            string stmp = prop.Value.ToString();
            if (stmp == "")
            {
                form.sComboxContent = "";
                form.sContent = "";
            }
            else
            {
                string[] EventContent;
                if (stmp.Length > 0)
                {
                    EventContent = stmp.Split('+');
                    if (EventContent.Length==2)//打开画面
                    {
                        form.sComboxContent = EventContent[0];
                        form.sContent = EventContent[1];
                    }
                    else if (EventContent.Length == 3)//写变量值
                    {
                        form.sComboxContent = EventContent[0];
                        form.sContent = EventContent[1] + "+" + EventContent[2];
                    }             
                }
            }
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                if (form.sComboxContent == "")
                {
                    prop.Value = "";
                }
                else
                {
                    if (form.sContent == "")
                    {
                        MessageBox.Show("没有事件内容!");
                        form.Dispose();
                        return prop.Value;
                    }
                     prop.Value = form.sComboxContent + "+" + form.sContent ;
                }
            }
            form.Dispose();
            return prop.Value;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        //alignleft
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.AlignLeft();
            drawArea.Invalidate();
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.AlignVCenter();
            drawArea.Invalidate();
        }
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.AlignRight();
            drawArea.Invalidate();
        }
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.AlignTop();
            drawArea.Invalidate();
        }
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.AlignHCenter();
            drawArea.Invalidate();
        }
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.AlignBottom();
            drawArea.Invalidate();
        }
        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.MoveSelectionToFront();
            drawArea.Invalidate();
        }
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            drawArea.ObjList.MoveSelectionToBack();
            drawArea.Invalidate();
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
                            TabPage page = new TabPage();
                            page.Name = pagename;
                            page.Text = pagename;
                            page.Tag = pagename;
                            page.Controls.Add(vs);
                            tabMain.TabPages.Add(page);
                            tabMain.SelectTab(pagename);
                            BinaryFormatter BinaryRead = new BinaryFormatter();
                            vs.ObjList = (ObjList)BinaryRead.Deserialize(fs);
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
       private void  SavePagesAndDevices()
        {
            //save pages form
            try
            {
                int count = tabMain.TabPages.Count;
                for (int i = 0; i < count; i++)
                {

                    VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)tabMain.TabPages[i].Controls[0];
                    string s = myProjectPath + tabMain.TabPages[i].Text + ".page";
                    using (FileStream fs = new FileStream(s, FileMode.Create))
                    {
                        if (fs != null)
                        {
                            BinaryFormatter BinaryWrite = new BinaryFormatter();
                            BinaryWrite.Serialize(fs, drawArea.ObjList);
                            BinaryWrite.Serialize(fs, drawArea.BackGroundColor);
                            fs.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Exception:" + ex.ToString(), " Save Page error.");
            }
            try
            {
                foreach (ChannelManage ch in ChannelList)
                {
                    string s = myProjectPath + ch.ChannelName + ".dev";
                    using (FileStream fs = new FileStream(s, FileMode.Create))
                    {
                        if (fs != null)
                        {
                            BinaryFormatter BinaryWrite = new BinaryFormatter();
                            BinaryWrite.Serialize(fs, ch);
                            fs.Close();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Exception:" + err.ToString(), " Save device error.");
            }
       }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (ceProjectPath != "")
            {
                saveTreeSolution(ceProjectPath);
                SavePagesAndDevices();
            }
            else
            {
                SaveFileDialog dlg = new SaveFileDialog();
                //设置文件类型
                dlg.Filter = " Prj files(*.cePrj)|*.cePrj|All files(*.*)|*.*";
                //设置默认文件类型显示顺序
                dlg.FilterIndex = 1;
                //保存对话框是否记忆上次打开的目录
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string filename = dlg.FileName;
                    ceProjectPath = filename;
                    saveTreeSolution(ceProjectPath);
                    SavePagesAndDevices();
                }
            }
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (ceProjectPath != "")
            {
                if (DialogResult.No == MessageBox.Show("工程已经存在，确实要新打开这个工程吗？",
               "重要提示", MessageBoxButtons.YesNo))
                {
                    return;
                }
                //CloseProject();
                closeTree();
                initTree();
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

            }
        }
        //new project
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (ceProjectPath != "")
            {
                if (DialogResult.No == MessageBox.Show("工程已经存在，确实要新建吗？",
               "重要提示", MessageBoxButtons.YesNo))
                {
                    return;
                }
                //save the project
                toolStripButton3.PerformClick();
                CloseProject();
                closeTree();
                initTree();
                myProjectPath = "";
            }
            NewPrjForm prj = new NewPrjForm();
            if (prj.ShowDialog() == DialogResult.OK)
            {
                string ProjectPath = prj.prjPath;
                string myProjectName = prj.prjName;
                string s = ProjectPath.Substring(ProjectPath.Length - 1, 1);
                if (s == "\\")
                {
                    myProjectPath = ProjectPath + myProjectName + "\\";
                    ceProjectPath = myProjectPath + myProjectName + ".cePrj";
                }
                else
                {
                    myProjectPath = ProjectPath + "\\" + myProjectName + "\\"; ;
                    ceProjectPath = myProjectPath + myProjectName + ".cePrj";
                }
                DirectoryInfo dir = new DirectoryInfo(myProjectPath);
                dir.Create();
                CloseProject();
                initTree();
                saveTreeSolution(ceProjectPath);
            }
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
            if (tabMain.TabPages.Count>0)
            {
                int j=tabMain.TabPages.Count;
                for (int i=0;i<j;i++)
                {
                    tabMain.TabPages.RemoveAt(i);
                }
            }
        }
        private string FindName(string text)
        {
           int i = tabMain.TabPages.Count;
           for (int j = 0; j < i; j++)
           {
               string s = tabMain.TabPages[j].Text;
               if (s.Equals(text))
               {
                   return s;
               }
           }
            foreach (ChannelManage ch in ChannelList)         
            {
                  foreach (DeviceManage dev in ch.AllDevList )
                  {
                      if (dev.DeviceName.Equals(text))
                      {
                          return text;
                      }
                  }         
            }
            return null;
        } 
        //创建form的名称，根据类型决定
        public string CreateFormName(string name)
        {
            if (name.Contains("画面组态"))
            {
                int count = 1;
                string text = "page" + count.ToString();

                while (FindName(text) != null)
                {
                    count++;
                    text = "page" + count.ToString();
                }
                return text;
            }
            else if (name.Contains("虚拟通道"))
            {
                int count = 1;
                string text = "虚拟设备" + count.ToString();
                while (FindName(text) != null)
                {
                    count++;
                    text = "虚拟设备" + count.ToString();
                }
                return text;
            }
            else if (name.Contains("串口通道"))
            {
                int count = 1;
                string text = "ModbusRTU设备" + count.ToString();
                while (FindName(text) != null)
                {
                    count++;
                    text = "ModbusRTU设备" + count.ToString();
                }
                return text;
            }
            return "error";
        }
        //新建一个画面或者设备
        private void CreatePage_Click(object sender, EventArgs e)
        {
            switch (treeExplore.SelectedNode.Text)
            {
                case "画面组态":
                    string name = CreateFormName("画面组态");
                    TreeNode tn = new TreeNode(name);
                    treeExplore.SelectedNode.Nodes.Add(tn);
                    treeExplore.ExpandAll();
                    NewPage(name,Color.White,800,480);
                    string strCurPageName = "当前画面名：" + name;
                    label1.Text = strCurPageName;
                    break;
            }
        }
        //虚拟通道函数
        private void VirtualChannel_Click(object sender, EventArgs e)
        {
            foreach (TreeNode nod in treeExplore.SelectedNode.Nodes)
            {
                if (nod.Text == "虚拟通道")
                {
                    MessageBox.Show("该通道已经建立!");
                    return;
                }
            }
            string name = "虚拟通道";
            TreeNode tn = new TreeNode(name);
            treeExplore.SelectedNode.Nodes.Add(tn);
            treeExplore.ExpandAll();
            ChannelManage ch = new ChannelManage();
            ChannelList.Add(ch);
            ch.ChannelName = name;
        }

        private void ComChannel_Click(object sender, EventArgs e)
        {
            foreach (TreeNode nod in treeExplore.SelectedNode.Nodes)
            {
                if (nod.Text == "串口通道")
                {
                    MessageBox.Show("该通道已经建立!");
                    return;
                }
            }
            string name = "串口通道";
            TreeNode tn = new TreeNode(name);
            treeExplore.SelectedNode.Nodes.Add(tn);
            treeExplore.ExpandAll();
            ChannelManage ch = new ChannelManage();
            ChannelList.Add(ch);
            ch.ChannelName = name;
        }
        //新建设备
        private void NewDevice_Click(object sender, EventArgs e)
        {
            string name = treeExplore.SelectedNode.Text;
            if (name.Contains("虚拟通道"))
            {
                string vname = CreateFormName(name);
                TreeNode tn = new TreeNode(vname);
                treeExplore.SelectedNode.Nodes.Add(tn);
                treeExplore.ExpandAll();
                foreach (ChannelManage ch in ChannelList)
                {
                    if (ch.ChannelName==name)
                    {
                        DeviceManage dev = new DeviceManage();
                        ch.AllDevList.Add(dev);
                        dev.DeviceName = vname;
                    } 
                }  
            }
            else if (name.Contains("串口通道"))
            {
                string vname = CreateFormName(name);
                TreeNode tn = new TreeNode(vname);
                treeExplore.SelectedNode.Nodes.Add(tn);
                treeExplore.ExpandAll();
                foreach (ChannelManage ch in ChannelList)
                {
                    if (ch.ChannelName == name)
                    {
                        DeviceManage dev = new DeviceManage();
                        ch.AllDevList.Add(dev);
                        dev.DeviceName = vname;
                    }
                }  
            }
        }
        //删除驱动及下面的设备
        private void Delit_Click(object sender, EventArgs e)
        {
            string name = treeExplore.SelectedNode.Text;
            if (name.Contains("虚拟通道") || name.Contains("串口通道") )
            {
                if (DialogResult.No == MessageBox.Show("删除该驱动将导致其下属全部设备驱动被删除，确实要删除吗？",
               "重要提示", MessageBoxButtons.YesNo))
                {
                    return;
                }
                foreach (TreeNode node in treeExplore.SelectedNode.Nodes)
                {
                    if (node != null)
                    {
                        foreach (ChannelManage form in ChannelList)//遍历每个通道
                        {
                            if (node.Text == form.ChannelName)//通道名相同
                            {
                                foreach (DeviceManage dev in form.AllDevList)//遍历每个设备
                                {
                                    dev.AllVarList.Clear();
                                    string s = myProjectPath + dev.DeviceName + ".dev";
                                    FileInfo file = new FileInfo(@s);
                                    if (file.Exists)
                                    {
                                        file.Delete(); //删除                       
                                    }
                                } 
                                form.AllDevList.Clear();//delete all device
                                ChannelList.Remove(form);//remove current channel
                            }
                        }
                    }
                }
            }
            //delete tree
            treeExplore.SelectedNode.Remove();
        }
        //删除某一设备
        private void DeleteDevice_Click(object sender, EventArgs e)
        {
            string name = treeExplore.SelectedNode.Text;
            if (DialogResult.No == MessageBox.Show("确实要删除吗？",
               "重要提示", MessageBoxButtons.YesNo))
                {
                    return;
                }

                foreach (ChannelManage form in ChannelList)//遍历每个通道
                {
                    foreach (DeviceManage dev in form.AllDevList)//遍历每个设备
                    {
                        if (dev.DeviceName == name)
                        {
                            dev.AllVarList.Clear();
                            string s = myProjectPath + dev.DeviceName + ".dev";
                            FileInfo file = new FileInfo(@s);
                            if (file.Exists)
                            {
                                file.Delete(); //删除                       
                            }
                            form.AllDevList.Remove(dev);
                            //delete tree
                            treeExplore.SelectedNode.Remove();
                            break;
                        }
                    }
                }
            }

        private void button2_Click(object sender, EventArgs e)
        {
            string name = CreateFormName("画面组态");
            TreeNode tn = new TreeNode(name);
            foreach (TreeNode node in treeExplore.Nodes)
            {
                if (node.Text.Equals("画面组态"))
               {
                   treeExplore.SelectedNode = node;
                   break;
               }
            }
            treeExplore.SelectedNode.Nodes.Add(tn);
            treeExplore.ExpandAll();
            NewPage(name, Color.White, 800, 480);
            string strCurPageName = "当前画面名：" + name;
            label1.Text = strCurPageName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TabPage page = tabMain.SelectedTab;//获取当前画面
            VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)page.Controls[0];
            BackColorForm form =new BackColorForm();
            form.backColor=drawArea.BackGroundColor;
            if (form.ShowDialog()==DialogResult.OK)
            {
                drawArea.BackGroundColor = form.backColor;
                drawArea.Invalidate();
            }            
        }
        //一键生成web画面
        private void toolStripButton13_Click(object sender, EventArgs e)
        {

            //main.htm make
            string main = myProjectPath + "main.html";
            StreamWriter man = new StreamWriter(main, false);
            man.WriteLine("<HTML>");
            man.WriteLine("<HEAD>");
            man.WriteLine("<TITLE>");
            man.WriteLine("Main Web");
            man.WriteLine("</TITLE>");
            man.WriteLine("</HEAD>");
            man.WriteLine("<BODY>");
            man.WriteLine("<P align=center>Windows CE WEB Monitor System</P>");
            man.WriteLine("<P align=center>Web List:</P>");

            //save form!         
            foreach (TabPage Node in tabMain.TabPages)
            {
                //main.htm make
                man.WriteLine("<P align=center><A href=\"./" + Node.Text + ".html" + "\">" + Node.Text + "</A></P>");
                //end main
                string jpgname = myProjectPath + Node.Text + ".jpeg";
                string name = myProjectPath + Node.Text + ".xml";
                string htmlname = myProjectPath + Node.Text + ".html";
                string vbsname = myProjectPath + Node.Text + ".vbs";
                VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)Node.Controls[0];
                int n = drawArea.ObjList.Count();
                //html make
                StreamWriter sw = new StreamWriter(htmlname, false);
                sw.WriteLine("<HTML>");
                sw.WriteLine("<HEAD>");
                sw.WriteLine("<TITLE>");
                sw.WriteLine(Node.Text);
                sw.WriteLine("</TITLE>");
                sw.WriteLine("</HEAD>");
                sw.WriteLine("<BODY  onload=update() >");
                //end html make
                //vbs make
                StreamWriter vbs = new StreamWriter(vbsname, false);
                vbs.WriteLine("Set poster = CreateObject(\"Microsoft.XMLHTTP\")");
                vbs.WriteLine("Set XMLDocument=CreateObject(\"Microsoft.XMLDOM\")");
                vbs.WriteLine("sub update()");
                vbs.WriteLine("poster.Open \"GET\", \"" + Node.Text + ".xml\"" + ", False");
                vbs.WriteLine("poster.send");
                vbs.WriteLine("XMLDocument.async=false");
                vbs.WriteLine("XMLDocument.loadXML(poster.responseText)");
                //end vbs
                System.Drawing.Bitmap bmp = new Bitmap(drawArea.Width, drawArea.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(drawArea.BackGroundColor);
                System.Drawing.Imaging.ImageFormat format = ImageFormat.Jpeg;
                XmlTextWriter writer = new XmlTextWriter(name, null);
                writer.Formatting = Formatting.Indented;
                //write start
                writer.WriteStartDocument();
                writer.WriteComment("Form objects xml,Windows CE make for 1.0 version!");
                //write boot!!
                writer.WriteStartElement("objectlist");
                for (int i = 0; i < n; i++)
                {
                    DrawObject o = (DrawObject)drawArea.ObjList[i];

                    switch (o.ObjectType)
                    {
                        case Global.DrawType.DrawLine://line
                            DrawLine m2 = (DrawLine)o;
                            m2.Draw(g, drawArea);
                            writer.WriteStartElement("object");
                            writer.WriteAttributeString("id", m2.ID.ToString());
                            writer.WriteAttributeString("type", "line");
                            writer.WriteAttributeString("x", m2.StartPoint.X.ToString());
                            writer.WriteAttributeString("y", m2.StartPoint.Y.ToString());
                            writer.WriteEndElement();
                            break;
                        case Global.DrawType.DrawRectangle://矩形
                            DrawRectangle m41 = (DrawRectangle)o;
                            m41.Draw(g,drawArea);
                            writer.WriteStartElement("object");
                            writer.WriteAttributeString("id", m41.ID.ToString());
                            writer.WriteAttributeString("type", "rectangle");
                            writer.WriteAttributeString("x", m41.X.ToString());
                            writer.WriteAttributeString("y", m41.Y.ToString());
                            writer.WriteEndElement();
                            break;
                        case Global.DrawType.DrawEllipse://和椭圆
                            DrawEllipse m4 = (DrawEllipse)o;
                            m4.Draw(g,drawArea);
                            writer.WriteStartElement("object");
                            writer.WriteAttributeString("id", m4.ID.ToString());
                            writer.WriteAttributeString("type", "circle");
                            writer.WriteAttributeString("x", m4.X.ToString());
                            writer.WriteAttributeString("y", m4.Y.ToString());
                            writer.WriteEndElement();
                            break;
                        case Global.DrawType.DrawText://文本
                            DrawText m = (DrawText)o;
                            m.Draw(g,drawArea);
                            writer.WriteStartElement("object");
                            writer.WriteAttributeString("id", m.ID.ToString());
                            writer.WriteAttributeString("type", "text");
                            writer.WriteAttributeString("x", m.X.ToString());
                            writer.WriteAttributeString("y", m.Y.ToString());
                            writer.WriteEndElement();
                            //html make
                            if (m.textName != "")
                            {
                                //html make
                                int x = m.X + 10;//修正偏移量
                                int y = m.Y + 10;
                                sw.WriteLine("<SPAN id=Span" + m.ID.ToString() + " style=" + "\"" + "Z-INDEX:" + m.ID.ToString() + "; " + "LEFT:" + x.ToString() + "; WIDTH: " + m.Width + "; POSITION: absolute; TOP: " + y.ToString() + "; HEIGHT: " + m.Height +  "; TEXT-ALIGN: " + m.StringFormat.Alignment.ToString() + "\"></SPAN>");                            
                                //vbs make
                                vbs.WriteLine("set node=XMLDocument.documentElement.selectSingleNode(\"//object[@id=\" & chr(34) & \"" + m.ID.ToString()+ "\" & chr(34) & \"]/value\")");
                                vbs.WriteLine("v= node.firstChild.nodeValue");
                                vbs.WriteLine("Span" + m.ID.ToString() + ".innerHTML=\"<FONT color=blue face=黑体 size=6><STRONG>\" & v & \"</STRONG></FONT>\"");
                                //
                            }
                            break;                     
                        case Global.DrawType.DrawPic://图片
                            DrawPic m7 = (DrawPic)o;
                            m7.Draw(g,drawArea);
                            writer.WriteStartElement("object");
                            writer.WriteAttributeString("id", m7.ID.ToString());
                            writer.WriteAttributeString("type", "picture");
                            writer.WriteAttributeString("x", m7.X.ToString());
                            writer.WriteAttributeString("y", m7.Y.ToString());
                            writer.WriteEndElement();
                            break;                    
                    }
                }
                //html make src="fcu_update.vbs"
                sw.WriteLine("<IMG src=\"./" + Node.Text + ".jpeg\">");
                sw.WriteLine("<script language=vbScript src=\"" + Node.Text + ".vbs\">");
                sw.WriteLine("</script>");
                sw.WriteLine("</BODY>");
                sw.WriteLine("</HTML>");
                sw.Close();
                //
                //vbs make
                vbs.WriteLine(" window.settimeout \"update()\",1000 ");
                vbs.WriteLine("end sub");
                vbs.Close();
                //
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
                bmp.Save(jpgname, format);
            }
            //main.htm make
            man.WriteLine("</BODY>");
            man.WriteLine("</HTML>");
            man.Close();
            //
        }
    }
}