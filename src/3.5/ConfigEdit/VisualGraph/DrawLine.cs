using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace VisualGraph
{
    [Serializable]
    public class DrawLine : DrawObject
    {

        private Point startPoint;
        private Point endPoint;
        [Description("起点"), Category("起点")]
        public Point StartPoint
        {
            get
            {
                return startPoint;
            }
            set
            {
                startPoint = value;
            }
        }
        [Description("终点"), Category("终点")]
        public Point EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                endPoint = value;
            }
        }
        public DrawLine(Point point, VisualGraph drawArea)
        {
            //int Width;
            // int Height;
            //  PenWidth = 1;
            // Width = 50;
            // Height = 20;		
            ObjName = drawArea.CreateObjName();
            ObjectType = Global.DrawType.DrawLine;
            GenerateID(Global.DrawType.DrawLine);
            startPoint = point;
            endPoint = new Point(point.X + Width, point.Y + Height);
            ShapeRect = new Rectangle(startPoint.X, StartPoint.Y, Width, Height);

            //Initialize();
        }

        public override void Draw(Graphics g, VisualGraph drawArea)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Pen pen = new Pen(Color, PenWidth);
            g.DrawLine(pen, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);

            pen.Dispose();
        }

        public override int HandleCount
        {
            get
            {
                return 2;
            }
        }

        public override Point GetHandle(int handleNumber)
        {

            if (handleNumber == 1)
                return startPoint;
            else
                return endPoint;
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

            Point p1 = startPoint;
            Point p2 = endPoint;
            Point s;
            Rectangle r1, r2;
            int o, u;

            if (p1.X > p2.X) { s = p2; p2 = p1; p1 = s; }

            r1 = new Rectangle(p1.X, p1.Y, 0, 0);
            r2 = new Rectangle(p2.X, p2.Y, 0, 0);
            r1.Inflate(3, 3);
            r2.Inflate(3, 3);

            if (Rectangle.Union(r1, r2).Contains(point))
            {
                if ((Math.Abs(p1.Y - p2.Y) < 0.001) && (Math.Abs(point.Y - p1.Y) < 3))
                    return true;
                if ((Math.Abs(p1.X - p2.X) < 0.001) && (Math.Abs(point.X - p1.X) < 3))
                    return true;
                if (p1.Y < p2.Y) //SWNE
                {
                    o = r1.Left + (((r2.Left - r1.Left) * (point.Y - r1.Bottom)) / (r2.Bottom - r1.Bottom));
                    u = r1.Right + (((r2.Right - r1.Right) * (point.Y - r1.Top)) / (r2.Top - r1.Top));
                    return ((point.X > o) && (point.X < u));
                }
                else //NWSE
                {
                    o = r1.Left + (((r2.Left - r1.Left) * (point.Y - r1.Top)) / (r2.Top - r1.Top));
                    u = r1.Right + (((r2.Right - r1.Right) * (point.Y - r1.Bottom)) / (r2.Bottom - r1.Bottom));
                    return ((point.X > o) && (point.X < u));
                }
            }
            return false;
        }

        public override bool IntersectsWith(Rectangle rectangle)
        {

            return rectangle.Contains(startPoint) && rectangle.Contains(endPoint);
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                case 2:
                    return Cursors.SizeAll;
                default:
                    return Cursors.Default;
            }
        }

        public override void MoveHandleTo(Point point, int handleNumber)
        {
            if (handleNumber == 1)
            {
                startPoint = point;
            }
            else
            {
                endPoint = point;

            }
            //SetRectangle(point.X, point.Y, Math.Abs(endPoint.X - startPoint.X),Math.Abs(EndPoint.Y - startPoint.Y));
            SetRectangle(startPoint.X, startPoint.Y, endPoint.X - startPoint.X, EndPoint.Y - startPoint.Y);
        }
        protected void SetRectangle(int x, int y, int width, int height)
        {

            ShapeRect = new Rectangle(x, y, width, height);
        }
        public override void Move(int deltaX, int deltaY)
        {

            startPoint.X += deltaX;
            startPoint.Y += deltaY;

            endPoint.X += deltaX;
            endPoint.Y += deltaY;
            SetRectangle(startPoint.X, startPoint.Y, endPoint.X - startPoint.X, EndPoint.Y - startPoint.Y);
        }



    }
}
