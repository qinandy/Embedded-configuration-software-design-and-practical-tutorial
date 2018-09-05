using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
namespace VisualGraph
{
    public partial class VisualGraph : UserControl
    {
        private Global.DrawType activedObjType;
        private DrawBaseTool tools;

        private ObjList objList;
        private bool drawNetRectangle = false;
        private Rectangle netRectangle;
        private bool showGrid;
        private Size gridSize = new Size(10, 10);
        private float m_Scalef = 1.0f;

        public VisualGraph()
        {
            InitializeComponent();
            ObjList = new ObjList();
            tools = new SelectTool();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            ActivedObjType = Global.DrawType.POINTER;
        }
        public Global.DrawType ActivedObjType
        {
            get
            {
                return activedObjType;
            }
            set
            {
                activedObjType = value;
            }
        }
        public bool DrawNetRectangle
        {
            get
            {
                return drawNetRectangle;
            }
            set
            {
                drawNetRectangle = value;
            }
        }
        public Rectangle NetRectangle
        {
            get
            {
                return netRectangle;
            }
            set
            {
                netRectangle = value;
            }
        }
        public ObjList ObjList
        {
            get
            {

                return objList;
            }
            set
            {
                objList = value;
            }
        }
       

        public bool ShowGrid
        {
            get
            {
                return showGrid;
            }
            set
            {
                showGrid = value;
            }
        }

        public Size GridSize
        {
            get
            {
                return gridSize;
            }
            set
            {
                gridSize = value;
            }
        }

        public float M_Scalef
        {
            get
            {
                return m_Scalef;
            }
            set
            {
                m_Scalef = value;
            }
        }
        public void DrawSelectAll(Graphics g)
        {
            if (!DrawNetRectangle)
                return;
            ControlPaint.DrawFocusRectangle(g, NetRectangle, Color.Black, Color.Transparent);
        }
        private Color _backColor = Color.White;
        public Color BackGroundColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                this.Refresh();
            }
        }
        private void VisualGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.PageUnit = GraphicsUnit.Pixel;
            e.Graphics.PageScale = M_Scalef;

            SolidBrush brush = new SolidBrush(_backColor);
            e.Graphics.FillRectangle(brush, this.ClientRectangle);
            if (ShowGrid)
            {
                ControlPaint.DrawGrid(e.Graphics, this.ClientRectangle, gridSize, this.BackColor);
            }
            if (objList != null)
            {
                ObjList.Draw(e.Graphics, this);
            }
            DrawSelectAll(e.Graphics);
            brush.Dispose();
        }

        private void VisualGraph_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tools.OnMouseDown(this, e);
            }

        }

        private void VisualGraph_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.None)
            {

                tools.OnMouseMove(this, e);
            }

            else
                this.Cursor = Cursors.Default;
        }

        private void VisualGraph_MouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
                tools.OnMouseUp(this, e);
        }
        public String CreateObjName()
        {
            int No = 1;
            for (int n = 0; n <= ObjList.Count() - 1; n++)
            {

                if (ObjList[n].ObjName == (ActivedObjType.ToString() + No.ToString()))
                {
                    No = No + 1;
                }
            }
            return ActivedObjType.ToString() + No.ToString();
        }

        private void VisualGraph_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void VisualGraph_DragDrop(object sender, DragEventArgs e)
        {
            Point point = new Point(e.X, e.Y);
            point = this.PointToClient(point);
            switch (activedObjType)
            {
                case Global.DrawType.DrawText:
                    DrawText text = new DrawText(point, this);
                    objList.UnselectAll();
                    objList.AddObject(text);
                    text.Selected = true;
                    break;
                case Global.DrawType.DrawRectangle:
                    DrawRectangle rect = new DrawRectangle(point, this);
                    objList.UnselectAll();
                    objList.AddObject(rect);
                    rect.Selected = true;
                    break;
                case Global.DrawType.DrawEllipse:
                    DrawEllipse elip = new DrawEllipse(point, this);
                    objList.UnselectAll();
                    objList.AddObject(elip);
                    elip.Selected = true;
                    break;
                case Global.DrawType.DrawLine:
                    DrawLine line = new DrawLine(point, this);
                    objList.UnselectAll();
                    objList.AddObject(line);
                    line.Selected = true;
                    break;
                case Global.DrawType.DrawPic:
                    DrawPic pic = new DrawPic(point, this);
                    objList.UnselectAll();
                    objList.AddObject(pic);
                    pic.Selected = true;
                    break;
            }
            activedObjType = Global.DrawType.POINTER;
            this.Refresh();
        }

        private void VisualGraph_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                this.objList.SelectAll();
                this.Refresh();
            }
            if (e.Control && e.KeyCode == Keys.C)
            {

                this.ObjList.Copy();
                this.Refresh();
            }
            if (e.Control && e.KeyCode == Keys.V)
            {
                this.ObjList.Clone(this);
                this.Refresh();
            }
            if (e.KeyCode == Keys.Delete)
            {
                this.ObjList.DeleteSelection();
                this.Refresh();
            }
            switch (e.KeyCode)
            {
                //case Keys.Down:
                case Keys.S:
                    MoveDown();
                    break;
                //case Keys.Up:
                case Keys.W:
                    MoveUp();
                    break;
                //case Keys.Left:
                case Keys.A:
                    MoveLeft();
                    break;
                //case Keys.Right :
                case Keys.D:
                    MoveRight();
                    break;

            }
            this.Refresh();
        }
        public void MoveDown()
        {
            int dx = 0;
            int dy = 1;
            int n = this.ObjList.SelectionCount;
            for (int i = n - 1; i >= 0; i--)
            {
                this.ObjList.GetSelectedObject(i).Move(dx, dy);
            }
            this.Refresh();
        }


        public void MoveUp()
        {
            int dx = 0;
            int dy = -1;
            int n = this.ObjList.SelectionCount;
            for (int i = n - 1; i >= 0; i--)
            {
                this.ObjList.GetSelectedObject(i).Move(dx, dy);
            }
            this.Refresh();
        }

        public void MoveLeft()
        {
            int dx = -1;
            int dy = 0;
            int n = this.ObjList.SelectionCount;
            for (int i = n - 1; i >= 0; i--)
            {
                this.ObjList.GetSelectedObject(i).Move(dx, dy);
            }
            this.Refresh();
        }
        public void MoveRight()
        {
            int dx = 1;
            int dy = 0;
            int n = this.ObjList.SelectionCount;
            for (int i = n - 1; i >= 0; i--)
            {
                this.ObjList.GetSelectedObject(i).Move(dx, dy);
            }
            this.Refresh();
        }
        public void SaveToXml(string file)
        {
            XmlDocument doc = new XmlDocument();  
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);  
            XmlElement root = doc.CreateElement("DrawDocument");
            doc.AppendChild(root);
            XmlElement itemdoc = doc.CreateElement("PageObject");
            root.AppendChild(itemdoc);
            DocToXml(doc, itemdoc);
            int n = ObjList.Count();
            for (int i = 0; i < n; i++)
            {
                XmlElement item = doc.CreateElement("DrawObject");
                root.AppendChild(item);
                ObjList[i].WriteToXml(doc, item);
            }
            XmlTextWriter tr = new XmlTextWriter(file, null);
            tr.Formatting = Formatting.Indented;
            doc.WriteContentTo(tr);
            tr.Close();
        }
       
        public void LoadFromXml(string file)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlTextReader xtr = new XmlTextReader(file);
            xdoc.Load(xtr);
            int n = xdoc.DocumentElement.ChildNodes.Count;
            for (int i = 0; i < n; i++)
            {
                XmlElement xe = (XmlElement)xdoc.DocumentElement.ChildNodes[i];
                string drawObjectType = xe.GetAttribute("Type");
                if (drawObjectType.Equals("VisualGraph"))
                {
                    DocFromXml(xe);
                }
                else
                {
                    DrawObject drawObject = (DrawObject)Activator.CreateInstance("VisualGraph", "VisualGraph." + drawObjectType).Unwrap();
                    drawObject.ReadFromXml(xe);
                    ObjList.AddObject(drawObject);
                }
            }
        }
        public void DocToXml(XmlDocument xmlDoc, XmlElement xmlElement)
        {
            xmlElement.SetAttribute("Type", this.GetType().Name);
            xmlElement.SetAttribute("PageWidth", this.Width.ToString());
            xmlElement.SetAttribute("PageHeight", this.Height.ToString());
            xmlElement.SetAttribute("PageColor", BackGroundColor.ToArgb().ToString());
        }
        public void DocFromXml(XmlElement xmlElement)
        {
            string val;
            val = xmlElement.GetAttribute("PageWidth");
            this.Width = int.Parse(val);
            val = xmlElement.GetAttribute("PageHeight");
            this.Height = int.Parse(val);
            val = xmlElement.GetAttribute("PageColor");
            this.BackGroundColor = Color.FromArgb(Convert.ToInt32(val));
        }
    }
}