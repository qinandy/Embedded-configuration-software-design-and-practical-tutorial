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
namespace ConfigEdit
{
    public partial class Form1 : Form
    {
        private XmlDocument doc = null;
        private string myProjectPath = "";
        private string ceProjectPath;
        private bool  bModifiedFalg = false;
        //实例化一个设备管理列表
        List<DeviceManage> DevList = new List<DeviceManage>();
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
                    if (CurrentNode.Parent!=null)
                    {
                        if (CurrentNode.Text.Contains("画面"))//说明你选中了画面名称了
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
                        //case "设备组态":
                        //    CurrentNode.ContextMenuStrip = MenuDriver;
                        //    break;                
                    }
                    treeExplore.SelectedNode = CurrentNode;//选中这个节点
                    //如果不是主节点，而是子节点，这里是画面的子节点
                    if (CurrentNode.Parent != null)
                    {
                        //画面编辑
                       /*   if (CurrentNode.Parent.Text == "画面组态")
                        {
                            CurrentNode.ContextMenuStrip = contextMenuLook;
                        }
                        //全部驱动
                      if (CurrentNode.Parent.Text == "虚拟驱动" || CurrentNode.Parent.Text == "OPC驱动" || CurrentNode.Parent.Text == "ModbusRTU驱动")
                        {
                            CurrentNode.ContextMenuStrip = contextMenuLook;
                        }*/
                   
                    }
                }
            }
            if (MouseButtons.Left == e.Button)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeExplore.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    if (CurrentNode.Text.Contains("串口设备"))
                    {

                    }
                    else if (CurrentNode.Text.Contains("串口通道"))
                    {

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
                        //OpenOneForm(no.Name);
                }
                //for control
                if (tn.Parent.Text.Equals("设备组态"))
                {
                   //opendriver()
                }
                if (no.ChildNodes != null)
                {
                    ChildNodes(no, tn);
                }
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
                    this.propertyGrid1.SelectedObject = drawArea;
                }
            }
        }
        //动作属性窗口鼠标单击响应
        private object CustomActionItem_OnClick(object sender, EventArgs e)
        {
            PropertyGridEx.CustomProperty prop = (PropertyGridEx.CustomProperty)((PropertyGridEx.CustomProperty.CustomPropertyDescriptor)sender).CustomProperty;
            VarForm form = new VarForm();
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
        //存储工程函数
        private void SaveProject(string sPath)
        {
       /*     if (sPath == "") return;
            //save the project
            doc.LoadXml("<cePrj></cePrj>");
            XmlNode root = doc.SelectSingleNode("cePrj");
            //遍历树控件，存储树叶
            XmlElement Test = doc.CreateElement(name[1]);
            root.AppendChild(Test);
          
            doc.Save(sPath);
            //save all page
            //获得路径名
            string DirectoryPath = new FileInfo(sPath).DirectoryName;
            string send = DirectoryPath.Substring(DirectoryPath.Length - 1, 1);
            if (send == "\\")
            {
                //DirectoryPath = DirectoryPath;
            }
            else
            {
                DirectoryPath = DirectoryPath + "\\";
            }
            //save default com setting
            FileStream fs = new FileStream(DirectoryPath + "Com.settings", FileMode.Create);
            if (fs != null)
            {
                StreamWriter writer = new StreamWriter(fs);
                writer.WriteLine("COM1");
                writer.WriteLine("9600");
                writer.WriteLine("8");
                writer.WriteLine("None");
                writer.WriteLine("One");
                writer.Close();
                fs.Close();
            }
            //save page
            int count = tabMain.TabPages.Count;
            for (int k = 1; k < count; k++)
            {
                string file = DirectoryPath + tabMain.TabPages[k].Text;
                //((VisualGraph.VisualGraph)tabMain.TabPages[k].Controls[0]).Document.SaveToXml(file);
                XmlSerializer mySerializer = new XmlSerializer(typeof(DrawDoc));
                StreamWriter myWriter = new StreamWriter(DirectoryPath + tabMain.TabPages[k].Text);
                mySerializer.Serialize(myWriter, ((VisualGraph.VisualGraph)tabMain.TabPages[k].Controls[0]).Document);
                myWriter.Close();
            }
            //save
            try
            {
                Stream StreamWrite;
                SaveFileDialog DialogSave = new SaveFileDialog();
                DialogSave.DefaultExt = "cePrj";
                DialogSave.Title = "Save  Project";
                DialogSave.Filter = "cePrj files (*.cePrj)|*.cePrj|All files (*.*)|*.*";
                if (DialogSave.ShowDialog() == DialogResult.OK)
                {
                    if ((StreamWrite = DialogSave.OpenFile()) != null)
                    {
                        int count = tabMain.TabPages.Count;
                        for (int j = count - 1; j >= 1; j--)
                        {
                            tabMain.TabPages.RemoveAt(j);
                        }
                        BinaryFormatter BinaryWrite = new BinaryFormatter();
                        BinaryWrite.Serialize(StreamWrite, drawArea.ObjList);
                        StreamWrite.Close();
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Exception:" + err.ToString(), "保存错误");
            }
            bModifiedFalg = false;*/
        }
       private void  SavePagesAndDevices()
        {
            //save device list!
            foreach (DeviceManage dev in DevList)
            {
                string s = myProjectPath + dev.DeviceName + ".dev";
                try
                {
                    using (FileStream fs = new FileStream(s, FileMode.Create))
                    {
                        if (fs != null)
                        {
                            BinaryFormatter BinaryWrite = new BinaryFormatter();
                            BinaryWrite.Serialize(fs, dev);
                            fs.Close();
                        }
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show("Exception:" + err.ToString(), " Save device error.");
                }
            } 
           //save pages form
           try
           {
               int count = tabMain.TabPages.Count;
               for (int i=0;i<count;i++)
               {
                 
                   VisualGraph.VisualGraph drawArea = (VisualGraph.VisualGraph)tabMain.TabPages[i].Controls[0];
                   string s = myProjectPath + tabMain.TabPages[i].Text + ".page";
                   using (FileStream fs = new FileStream(s, FileMode.Create))
                   {
                       if (fs != null)
                       {
                           BinaryFormatter BinaryWrite = new BinaryFormatter();
                           BinaryWrite.Serialize(fs, drawArea.ObjList);
                           fs.Close();
                       }
                   }
               }
           }
           catch (System.Exception ex)
           {
               MessageBox.Show("Exception:" + ex.ToString(), " Save Page error.");
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
            if (DevList.Count > 0)
            {
                //close form
                foreach (DeviceManage Node in DevList)
                {
                    Node.AllVarList.Clear();
                }
                DevList.Clear();
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
            return null;
        } 
        //创建form的名称，根据类型决定
        public string CreateFormName(string name)
        {
            if (name.Contains("画面组态"))
            {
                int count = 1;
                string text = "画面" + count.ToString();

                while (FindName(text) != null)
                {
                    count++;
                    text = "画面" + count.ToString();
                }
                return text;
            }
         /*   else if (name.Contains("控制策略"))
            {
                int count = 1;
                string text = "控制方案" + count.ToString();
                while (FindDocument(text) != null)
                {
                    count++;
                    text = "控制方案" + count.ToString();
                }
                return text;
            }
            else if (name.Contains("虚拟驱动"))
            {
                int count = 1;
                string text = "虚拟设备" + count.ToString();
                while (FindDocument(text) != null)
                {
                    count++;
                    text = "虚拟设备" + count.ToString();
                }
                return text;
            }
            else if (name.Contains("OPC驱动"))
            {
                int count = 1;
                string text = "OPC设备" + count.ToString();
                while (FindDocument(text) != null)
                {
                    count++;
                    text = "OPC设备" + count.ToString();
                }
                return text;
            }
            else if (name.Contains("ModbusRTU驱动"))
            {
                int count = 1;
                string text = "ModbusRTU设备" + count.ToString();
                while (FindDocument(text) != null)
                {
                    count++;
                    text = "ModbusRTU设备" + count.ToString();
                }
                return text;
            }*/
            return "error";
        }
        //新建一个画面
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
                /*     case "设备组态":
                         string name2 = CreateFormName("设备组态");
                         TreeNode tnc = new TreeNode(name2);
                         treeExplore.SelectedNode.Nodes.Add(tnc);
                         treeExplore.ExpandAll();
                         newFBDForm(name2);
                         break;
                  */
            }
        }
    }
}