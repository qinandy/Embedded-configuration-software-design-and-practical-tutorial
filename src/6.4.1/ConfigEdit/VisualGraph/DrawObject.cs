using System;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Collections;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace VisualGraph
{

    [Serializable]
    public abstract class DrawObject
    {

        public DrawObject()
        {
            Initialize();
        }

        private bool selected;
        private Color color;
        private int penWidth;

        private Rectangle Rect;

        private string objName;

        private Global.DrawType objectType;
        private uint id = 0;
        //以下为对象事件属性
        //click
        private string _Click = "";//1,打开画面，2，设置变量值
        [Browsable(false)]
        public string Click
        {
            get
            {
                return _Click;
            }
            set
            {
                _Click = value;
            }
        }
        //double
        private string _DoubleClick = "";//1,打开画面，2，设置变量值
        [Browsable(false)]
        public string DoubleClick
        {
            get
            {
                return _DoubleClick;
            }
            set
            {
                _DoubleClick = value;
            }
        }
        //private string _format = "{0:#.00}";
        private string _format = "{0:F2}Unit";
        [Description("文本框用的数据格式方法"), Category("格式")]
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
            }
        }
        private string _textName = "";
        [Browsable(false)]
        public string textName
        {
            get
            {
                return _textName;
            }
            set
            {
                _textName = value;
            }
        }
        private string _xName = "";
        [Browsable(false)]
        public string xName
        {
            get
            {
                return _xName;
            }
            set
            {
                _xName = value;
            }
        }
        private string _yName = "";
        [Browsable(false)]
        public string yName
        {
            get
            {
                return _yName;
            }
            set
            {
                _yName = value;
            }
        }
        private string _widthName = "";
        [Browsable(false)]
        public string widthName
        {
            get
            {
                return _widthName;
            }
            set
            {
                _widthName = value;
            }
        }
        private string _heightName = "";
        [Browsable(false)]
        public string heightName
        {
            get
            {
                return _heightName;
            }
            set
            {
                _heightName = value;
            }
        }
        private string _visibleName = "";
        [Browsable(false)]
        public string visibleName
        {
            get
            {
                return _visibleName;
            }
            set
            {
                _visibleName = value;
            }
        }
        private string _FillColorName = "";
        [Browsable(false)]
        public string FillColorName
        {
            get
            {
                return _FillColorName;
            }
            set
            {
                _FillColorName = value;
            }
        }
        //
        [Browsable(false)]
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }
        [Description("对象的颜色"), Category("设置")]
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
        [Description("对象的线宽"), Category("设置")]
        public int PenWidth
        {
            get
            {
                return penWidth;
            }
            set
            {
                penWidth = value;
            }
        }
        private int _X = 0;
        [Browsable(false)]
        public int X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }
        private int _Y = 0;
        [Browsable(false)]
        public int Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }
        private int _Width = 50;
        [Browsable(false)]
        public int Width
        {
            get
            {
                return _Width;
            }
            set
            {
                _Width = value;
            }
        }
        private int _Height = 50;
        [Browsable(false)]
        public int Height
        {
            get
            {
                return _Height;
            }
            set
            {
                _Height = value;
            }
        }
        [Description("对象的位置与尺寸"), Category("对象")]
        public Rectangle ShapeRect
        {
            get
            {
                return Rect;
            }
            set
            {
                Rect = value;
                X = Rect.X;
                Y = Rect.Y;
                Width = Rect.Width;
                Height = Rect.Height;
            }
        }
        [Description("对象的名称"), Category("对象")]
        public string ObjName
        {
            get
            {
                return objName;
            }
            set
            {
                objName = value;
            }
        }
        [Browsable(false)]
        public Global.DrawType ObjectType
        {
            get
            {
                return objectType;
            }
            set
            {
                objectType = value;
            }
        }
        [Browsable(false)]
        public uint ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        //lock
        protected bool _lock = false;
        [Description("锁定"), Category("锁定")]
        public bool Lock
        {
            get { return _lock; }
            set { _lock = value; }
        }
        //runmode
        protected bool _run = false;
       [Browsable(false)]
        public bool RunMode
        {
            get { return _run; }
            set { _run = value; }
        }
        #region Virtual Functions
        public virtual void Draw(Graphics g, VisualGraph drawArea)
        {

        }
        [Browsable(false)]
        public virtual int HandleCount
        {
            get
            {
                return 0;
            }
        }

        public virtual Point GetHandle(int handleNumber)
        {
            return new Point(0, 0);
        }

        public virtual Rectangle GetHandleRectangle(int handleNumber)
        {
            Point point = GetHandle(handleNumber);

            return new Rectangle(point.X - 3, point.Y - 3, 7, 7);
        }
        public virtual void DrawTracker(Graphics g, VisualGraph drawArea)
        {
            if (_run)
            {
                return;
            }
            if (!Selected)
                return;

            SolidBrush brush = new SolidBrush(Color.Black);
            if (drawArea.Focused)
            {
                brush.Color = Color.Black;
            }
            else
            {
                brush.Color = Color.Gray;
            }

            for (int i = 1; i <= HandleCount; i++)
            {
                g.FillRectangle(brush, GetHandleRectangle(i));
            }

            brush.Dispose();
        }
        public virtual int HitTest(Point point)
        {
            return -1;
        }

        public virtual bool PointInObject(Point point)
        {
            return false;
        }

        public virtual Cursor GetHandleCursor(int handleNumber)
        {
            return Cursors.Default;
        }

        public virtual bool IntersectsWith(Rectangle rectangle)
        {
            return false;
        }
        public virtual void SetAction(Object sender)
        {

        }
        public virtual void Move(int deltaX, int deltaY)
        {
          
        }

        public virtual void MoveHandleTo(Point point, int handleNumber)
        {
          
        }

        public virtual void Normalize()
        {
        }

        protected void Initialize()
        {
            color = Color.Black;
            penWidth = 1;
        }

        public void GenerateID(Global.DrawType type)
        {
            Random ra = new Random(unchecked((int)DateTime.Now.Ticks));
            int i = ra.Next(1, 100000);
            id = (uint)i;
        }
        #endregion
        public virtual void WriteToXml(XmlDocument xmlDoc, XmlElement xmlElement)
        {
            xmlElement.SetAttribute("Type", this.GetType().Name);
            xmlElement.SetAttribute("DrawType", objectType.ToString());
            xmlElement.SetAttribute("ObjName", ObjName);
            xmlElement.SetAttribute("Run", this._run.ToString());
            xmlElement.SetAttribute("Lock", _lock.ToString());
            xmlElement.SetAttribute("ID", this.id.ToString());
            xmlElement.SetAttribute("Click", Click);
            xmlElement.SetAttribute("DoubleClick", DoubleClick);
            xmlElement.SetAttribute("Format", Format);
            xmlElement.SetAttribute("textName", textName);
            xmlElement.SetAttribute("xName", xName);
            xmlElement.SetAttribute("yName", yName);
            xmlElement.SetAttribute("widthName", widthName);
            xmlElement.SetAttribute("heightName", heightName);
            xmlElement.SetAttribute("visibleName", visibleName);
            xmlElement.SetAttribute("FillColorName", FillColorName);
            xmlElement.SetAttribute("Color", Color.ToArgb().ToString());
            xmlElement.SetAttribute("PenWidth", PenWidth.ToString());
            xmlElement.SetAttribute("X", X.ToString());
            xmlElement.SetAttribute("Y", Y.ToString());
            xmlElement.SetAttribute("Width", Width.ToString());
            xmlElement.SetAttribute("Height", Height.ToString());

        }

        public virtual void ReadFromXml(XmlElement xmlElement)
        {
            string val;
            xmlElement.GetAttribute("Type");
            val=xmlElement.GetAttribute("DrawType");
            switch (val)
            {
                case "DrawEllipse":
                    objectType = Global.DrawType.DrawEllipse;
                    break;
                case "DrawLine":
                    objectType = Global.DrawType.DrawLine;
                    break;
                case "DrawPic":
                    objectType = Global.DrawType.DrawPic;
                    break;
                case "DrawRectangle":
                    objectType = Global.DrawType.DrawRectangle;
                    break;
                case "DrawText":
                    objectType = Global.DrawType.DrawText;
                    break;
            }
            ObjName=xmlElement.GetAttribute("ObjName");
            val=xmlElement.GetAttribute("Run");
            RunMode = Convert.ToBoolean(val);
            val=xmlElement.GetAttribute("Lock");
            Lock = Convert.ToBoolean(val);
            val=xmlElement.GetAttribute("ID");
            ID = Convert.ToUInt32(val);
            Click=xmlElement.GetAttribute("Click");
            DoubleClick=xmlElement.GetAttribute("DoubleClick");
            Format=xmlElement.GetAttribute("Format");
            textName=xmlElement.GetAttribute("textName");
            xName=xmlElement.GetAttribute("xName");
            yName=xmlElement.GetAttribute("yName");
            widthName=xmlElement.GetAttribute("widthName");
            heightName=xmlElement.GetAttribute("heightName");
            visibleName=xmlElement.GetAttribute("visibleName");
            FillColorName=xmlElement.GetAttribute("FillColorName");
            val=xmlElement.GetAttribute("Color");
            Color=Color.FromArgb(Convert.ToInt32(val));
            val=xmlElement.GetAttribute("PenWidth");
            PenWidth = Convert.ToInt32(val);
            val=xmlElement.GetAttribute("X");
            X = Convert.ToInt32(val);
            val=xmlElement.GetAttribute("Y");
            Y = Convert.ToInt32(val);
            val=xmlElement.GetAttribute("Width");
            Width = Convert.ToInt32(val);
            val=xmlElement.GetAttribute("Height");
            Height = Convert.ToInt32(val);
            ShapeRect = new Rectangle(X, Y, Width, Height);
        }
    }
}
