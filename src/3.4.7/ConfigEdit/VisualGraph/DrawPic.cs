using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
namespace VisualGraph
{
    [Serializable]
    public class DrawPic : DrawObject
    {
        public DrawPic(Point point, VisualGraph drawArea)
        {
            ObjName = drawArea.CreateObjName();

            ShapeRect = new Rectangle(point.X, point.Y, Width, Height);
            ObjectType = Global.DrawType.DrawPic;
            GenerateID(Global.DrawType.DrawPic);

        }
        private Bitmap _image = null;
        private string _path = "";
        // [Description("选择一个图片"), Category("图片")]
        //  [XmlIgnore]
        public Bitmap TheImage
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                _path = ImageToString(_image);
            }
        }
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        private Color _Color = Color.FromArgb(0, 0, 0);
        //  [XmlIgnore]
        //  [Description("图片对于该颜色透明"), Category("透明色")]
        public Color TransColor
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
            }
        }
        public static string ImageToString(Image img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //img.Save(stream, img.RawFormat);
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }

        public static Image StringToImage(string sz)
        {
            byte[] buffer = Convert.FromBase64String(sz);
            MemoryStream stream = new MemoryStream();
            stream.Write(buffer, 0, buffer.Length);
            //Bitmap bm = new Bitmap(stream);
            return Image.FromStream(stream);
        }
        public override void Draw(Graphics g, VisualGraph drawArea)
        {
            if (_image == null)
            {
                if (_path.Length != 0)
                {
                    _image = (Bitmap)StringToImage(_path);
                    _image.MakeTransparent(_Color);
                    g.DrawImage(_image, ShapeRect, new Rectangle(0, 0, _image.Width, _image.Height), GraphicsUnit.Pixel);
                }
            }
            else
            {
                _image.MakeTransparent(_Color);
                g.DrawImage(_image, ShapeRect, new Rectangle(0, 0, _image.Width, _image.Height), GraphicsUnit.Pixel);
            }


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

        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNESW;
                case 3:
                    return Cursors.SizeNWSE;
                case 4:
                    return Cursors.SizeNESW;
                default:
                    return Cursors.Default;
            }
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
            ShapeRect = new Rectangle(ShapeRect.X + deltaX, ShapeRect.Y + deltaY, ShapeRect.Width, ShapeRect.Height);
        }

    }
}
