using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGraph;
namespace ConfigEdit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.tabMain.Region = new Region(new RectangleF(this.tabPage1.Left, this.tabPage1.Top, this.tabPage1.Width, this.tabPage1.Height));
            NewPage("Page1", Color.White, 800, 480);
            this.treeView1.ExpandAll();
            this.treeView1.ItemDrag += new ItemDragEventHandler(treeView1_ItemDrag);
            this.treeView1.MouseDown += new MouseEventHandler(treeView1_MouseDown);
            this.propertyGridEx1.PropertyValueChanged += new PropertyValueChangedEventHandler(propertyGridEx1_PropertyValueChanged);
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
            //clear tabpage
            int count = tabMain.TabPages.Count;
            for (int j = count - 1; j >= 1; j--)
            {
                tabMain.TabPages.RemoveAt(j);
            }
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
    }
}