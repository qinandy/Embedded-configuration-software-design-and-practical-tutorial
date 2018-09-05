using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;

namespace VisualGraph
{
    [Serializable]
    public class DrawRectangle : DrawObject
    {


        public DrawRectangle()
        {

        }
        public DrawRectangle(Point point, VisualGraph drawArea)
        {

            ObjName = drawArea.CreateObjName();
            ShapeRect = new Rectangle(point.X, point.Y, Width, Height);
            ObjectType = Global.DrawType.DrawRectangle;
            GenerateID(Global.DrawType.DrawRectangle);
            //Initialize();
        }

        public override void Draw(Graphics g, VisualGraph drawArea)
        {
            Pen pen = new Pen(Color, PenWidth);

            g.DrawRectangle(pen, DrawRectangle.GetNormalizedRectangle(ShapeRect));

            pen.Dispose();
        }

        protected void SetRectangle(int x, int y, int width, int height)
        {
            ShapeRect = new Rectangle(x, y, width, height);
        }

        public override int HandleCount
        {
            get
            {
                return 8;
            }
        }

        public override Point GetHandle(int handleNumber)
        {
            int x, y, xCenter, yCenter;

            xCenter = ShapeRect.X + ShapeRect.Width / 2;
            yCenter = ShapeRect.Y + ShapeRect.Height / 2;
            x = ShapeRect.X;
            y = ShapeRect.Y;

            switch (handleNumber)
            {
                case 1:
                    x = ShapeRect.X;
                    y = ShapeRect.Y;
                    break;
                case 2:
                    x = xCenter;
                    y = ShapeRect.Y;
                    break;
                case 3:
                    x = ShapeRect.Right;
                    y = ShapeRect.Y;
                    break;
                case 4:
                    x = ShapeRect.Right;
                    y = yCenter;
                    break;
                case 5:
                    x = ShapeRect.Right;
                    y = ShapeRect.Bottom;
                    break;
                case 6:
                    x = xCenter;
                    y = ShapeRect.Bottom;
                    break;
                case 7:
                    x = ShapeRect.X;
                    y = ShapeRect.Bottom;
                    break;
                case 8:
                    x = ShapeRect.X;
                    y = yCenter;
                    break;
            }

            return new Point(x, y);

        }

        //¶¯»­×¢²áº¯Êý
        public override void SetAction(Object sender)
        {

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

        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return Cursors.Default;
            }
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
                    left = point.X;
                    top = point.Y;
                    break;
                case 2:
                    top = point.Y;
                    break;
                case 3:
                    right = point.X;
                    top = point.Y;
                    break;
                case 4:
                    right = point.X;
                    break;
                case 5:
                    right = point.X;
                    bottom = point.Y;
                    break;
                case 6:
                    bottom = point.Y;
                    break;
                case 7:
                    left = point.X;
                    bottom = point.Y;
                    break;
                case 8:
                    left = point.X;
                    break;
            }

            SetRectangle(left, top, right - left, bottom - top);
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

        public override void Normalize()
        {
            ShapeRect = DrawRectangle.GetNormalizedRectangle(ShapeRect);
        }

        public static Rectangle GetNormalizedRectangle(int x1, int y1, int x2, int y2)
        {
            if (x2 < x1)
            {
                int tmp = x2;
                x2 = x1;
                x1 = tmp;
            }

            if (y2 < y1)
            {
                int tmp = y2;
                y2 = y1;
                y1 = tmp;
            }

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        public static Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static Rectangle GetNormalizedRectangle(Rectangle r)
        {
            return GetNormalizedRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
        }

    }
}
