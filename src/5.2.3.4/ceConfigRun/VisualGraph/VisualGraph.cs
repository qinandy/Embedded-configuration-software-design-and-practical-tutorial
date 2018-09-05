using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
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
            //e.Graphics.PageUnit = GraphicsUnit.Pixel;
           // e.Graphics.PageScale = M_Scalef;

            SolidBrush brush = new SolidBrush(_backColor);
            e.Graphics.FillRectangle(brush, this.ClientRectangle);
            if (ShowGrid)
            {
             //   ControlPaint.DrawGrid(e.Graphics, this.ClientRectangle, gridSize, this.BackColor);
            }
            if (objList != null)
            {
                ObjList.Draw(e.Graphics, this);
            }
           
            brush.Dispose();
        }

        private void VisualGraph_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tools.OnMouseDown(this, e);
            }
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
        public void SaveToXml(string file)
        {
            XmlDocument doc = new XmlDocument();
            // 加入声明
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);
            // 加入根元素，即Table
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

            // 保存文件
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
                    DrawObject drawObject = null;
                    switch (drawObjectType)
                    {
                        case "DrawEllipse":
                            drawObject = (DrawObject)Activator.CreateInstance(typeof(DrawEllipse));
                            break;
                        case "DrawLine":
                            drawObject = (DrawObject)Activator.CreateInstance(typeof(DrawLine));
                            break;
                        case "DrawPic":
                            drawObject = (DrawObject)Activator.CreateInstance(typeof(DrawPic));
                            break;
                        case "DrawRectangle":
                            drawObject = (DrawObject)Activator.CreateInstance(typeof(DrawRectangle));
                            break;
                        case "DrawText":
                            drawObject = (DrawObject)Activator.CreateInstance(typeof(DrawText));
                            break;
                    }        
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