using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Xml;
using System.Xml.Serialization;
using Basic;
namespace VisualGraph
{
    [Serializable]
    public class DrawText : DrawObject
    {
        private string curText = "text";
        public DrawText()
        {
        }
        public DrawText(Point point, VisualGraph drawArea)
        {

            ObjName = drawArea.CreateObjName();
            ShapeRect = new Rectangle(point.X, point.Y, Width, Height);
            ObjectType = Global.DrawType.DrawText;
            GenerateID(Global.DrawType.DrawText);
            //Initialize();
        }
        //¶¯»­×¢²áº¯Êý
        public override void SetAction(Object sender)
        {
            Variable var = (Variable)sender;
            if (textName.Equals(var.Name))
            {
                CurText = string.Format(Format, var.Value);
            }
            else if (xName.Equals(var.Name))
            {
                Rectangle ret=new Rectangle(Convert.ToInt32(var.Value),ShapeRect.Y,ShapeRect.Width,ShapeRect.Height);
                ShapeRect = ret;
            }
            else if (yName.Equals(var.Name))
            {
                Rectangle ret = new Rectangle(ShapeRect.X, Convert.ToInt32(var.Value), ShapeRect.Width, ShapeRect.Height);
                ShapeRect = ret;
            }
            else if (widthName.Equals(var.Name))
            {
                Rectangle ret = new Rectangle(ShapeRect.X, ShapeRect.Y, Convert.ToInt32(var.Value), ShapeRect.Height);
                ShapeRect = ret;
            }
            else if (heightName.Equals(var.Name))
            {
                Rectangle ret = new Rectangle(ShapeRect.X, ShapeRect.Y,ShapeRect.Width , Convert.ToInt32(var.Value));
                ShapeRect = ret;
            }
            else if (visibleName.Equals(var.Name))
            {
                
            }
            
        }
        
        public string CurText
        {
            get
            {
                return curText;
            }
            set
            {
                curText = value;
            }
        }
        private bool _Show = false;
       
        public bool ShowEdge
        {
            get
            {
                return _Show;
            }
            set
            {
                _Show = value;
            }
        }
        [NonSerialized]
        private StringFormat _stringFormat = null;
       
        public StringFormat StringFormat
        {
            get
            {
                return _stringFormat;
            }
            set
            {
                _stringFormat = value;
            }
        }
        public override void Draw(Graphics g, VisualGraph drawArea)
        {
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            Pen fontPen = new Pen(Color, PenWidth);
            if (_stringFormat == null)
            {
                _stringFormat = new StringFormat();
                _stringFormat.Alignment = StringAlignment.Center;
                _stringFormat.LineAlignment = StringAlignment.Center; ;
            }

            Font drawFont = new Font("Arial", 12, System.Drawing.FontStyle.Bold);

            g.DrawString(CurText, drawFont, drawBrush, ShapeRect, _stringFormat);
            if (_Show)
            {
                g.DrawRectangle(fontPen, DrawRectangle.GetNormalizedRectangle(ShapeRect));
            }
            drawFont.Dispose();
            drawBrush.Dispose();
            fontPen.Dispose();

        }
        public override int HandleCount
        {
            get
            {
                return 4;
            }
        }
        public override Point GetHandle(int handleNumber)
        {
            int x, y;

            x = ShapeRect.X;
            y = ShapeRect.Y;

            switch (handleNumber)
            {
                case 1:
                    x = ShapeRect.X;
                    y = ShapeRect.Y;
                    break;
                case 2:
                    x = ShapeRect.Right;
                    y = ShapeRect.Y;
                    break;
                case 3:
                    x = ShapeRect.Right;
                    y = ShapeRect.Bottom;
                    break;
                case 4:
                    x = ShapeRect.X;
                    y = ShapeRect.Bottom;
                    break;
            }
            return new Point(x, y);

        }

        public override void MoveHandleTo(Point point, int handleNumber)
        {
         
            int left = ShapeRect.Left;
            int top = ShapeRect.Top;
            int right = ShapeRect.Right;
            int bottom = ShapeRect.Bottom;
            switch (handleNumber)
            {
                case 1:

                    break;
                case 2:
                    if ((point.X - left) > 10)
                        right = point.X;
                    top = point.Y;
                    break;
                case 3:
                    if ((point.X - left) > 10)
                        right = point.X;
                    bottom = point.Y;
                    break;
                case 4:

                    break;
            }

            SetRectangle(left, top, right - left, bottom - top);
        }
        protected void SetRectangle(int x, int y, int width, int height)
        {

            ShapeRect = new Rectangle(x, y, width, height);
        }

        public override int HitTest(Point point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (PointInObject(point))
                return 0;

            return -1;
        }

       
        public override bool PointInObject(Point point)
        {
            return ShapeRect.Contains(point);
        }
        public override bool IntersectsWith(Rectangle rectangle)
        {
            return rectangle.Contains(ShapeRect);
        }

        public override void Move(int deltaX, int deltaY)
        {
            if (Lock)
            {
                return;
            }
            ShapeRect = new Rectangle(ShapeRect.X + deltaX, ShapeRect.Y + deltaY, ShapeRect.Width, ShapeRect.Height);
        }
        public override void WriteToXml(XmlDocument xmlDoc, XmlElement xmlElement)
        {
            base.WriteToXml(xmlDoc, xmlElement);
            xmlElement.SetAttribute("ShowEdge", ShowEdge.ToString());
            xmlElement.SetAttribute("CurText", CurText);
        }

        public override void ReadFromXml(XmlElement xmlElement)
        {
            base.ReadFromXml(xmlElement);

            string val;
            val = xmlElement.GetAttribute("ShowEdge");
            ShowEdge = Convert.ToBoolean(val);
            CurText = xmlElement.GetAttribute("CurText");
        }
    }
}
