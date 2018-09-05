using System;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Collections;
using System.Reflection;


namespace VisualGraph
{
    [Serializable]
    public class ObjList
    {
        private const int MAX = int.MaxValue - 1;
        private ArrayList objList;
        private ArrayList tempList;
        public ObjList()
        {
            objList = new ArrayList();
            tempList = new ArrayList();
        }

        public void AddObject(DrawObject o)
        {
            objList.Add(o);
        }

        public int Count()
        {
            return objList.Count;
        }
        public void Draw(Graphics g, VisualGraph drawArea)
        {
            //MoveSelectionToFront();
            int n = objList.Count;
            DrawObject o;
            for (int i = 0; i <= n - 1; i++)
            {
                o = (DrawObject)objList[i];
                o.Draw(g, drawArea);
                if (o.Selected == true)
                {
                    o.DrawTracker(g, drawArea);
                }
            }
        }
        public DrawObject this[int index]
        {
            get
            {
                if (index < 0 || index >= objList.Count)
                    return null;

                return ((DrawObject)objList[index]);
            }
        }
        public int SelectionCount
        {
            get
            {
                int n = 0;

                foreach (DrawObject o in objList)
                {
                    if (o.Selected)

                        n++;
                }

                return n;
            }
        }
        public DrawObject GetSelectedObject(int index)
        {
            int n = -1;
            foreach (DrawObject o in objList)
            {

                if (o.Selected)
                {
                    n++;

                    if (n == index)
                        return o;
                }
            }

            return null;
        }
        public DrawObject GetSelectedObject(Point point)
        {
            foreach (DrawObject o in objList)
            {
                if (o.PointInObject(point))
                    return o;
            }
            return null;


        }
        public void SelectInRectangle(Rectangle rectangle)
        {
            UnselectAll();

            foreach (DrawObject o in objList)
            {
                if (o.IntersectsWith(rectangle))
                    o.Selected = true;
            }
        }

        public void UnselectAll()
        {
            foreach (DrawObject o in objList)
            {
                o.Selected = false;
            }
        }
        public void SelectAll()
        {

            foreach (DrawObject o in objList)
            {
                o.Selected = true;

            }
        }
        public void AlignLeft()
        {
            int MinLeft = MAX;
            int temp;
            int n = SelectionCount;
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                        DrawLine l = (DrawLine)o;
                        temp = Math.Min(l.StartPoint.X, l.EndPoint.X);
                 }
                 else
                {
                    temp = o.ShapeRect.Left;
                }
                if (temp < MinLeft)
                {
                      MinLeft = temp;//find the min value 
                }
            }
            //move and align
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    if (l.StartPoint.X < l.EndPoint.X)
                    {
                        l.Move(MinLeft - l.StartPoint.X, 0);
                    }
                    else
                    {
                        l.Move(MinLeft - l.EndPoint.X, 0);
                    }
                }
                else
                {
                    o.ShapeRect = new Rectangle(MinLeft, o.ShapeRect.Top, o.ShapeRect.Width, o.ShapeRect.Height);
                }
            }
        }
        public void AlignRight()
        {
            int MaxRight = 0;
            int temp;
            int n = SelectionCount;
            for (int i = 0; i < n; i++)
            {
                DrawObject o =GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    temp = Math.Max(l.StartPoint.X, l.EndPoint.X);
                }
                else
                {
                    temp = o.ShapeRect.Right;
                }
                if (temp > MaxRight)
                {
                    MaxRight = temp;
                }
            }
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    if (l.StartPoint.X > l.EndPoint.X)
                    {
                        l.Move(MaxRight - l.StartPoint.X, 0);
                    }
                    else
                    {
                        l.Move(MaxRight - l.EndPoint.X, 0);
                    }
                }
                else
                {
                    o.ShapeRect = new Rectangle(MaxRight - o.ShapeRect.Width, o.ShapeRect.Top, o.ShapeRect.Width, o.ShapeRect.Height);
                }
            }
        }
        public void AlignTop()
        {
            int MinTop = MAX;
            int temp;
            int n = SelectionCount;
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine )o;
                    temp = Math.Min(l.StartPoint.Y, l.EndPoint.Y);
                }
                else
                {
                    temp = o.ShapeRect.Top;
                }
                if (temp < MinTop)
                {
                    MinTop = temp;
                }
            }
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);

                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    if (l.StartPoint.Y < l.EndPoint.Y)
                    {
                        l.Move(0, MinTop - l.StartPoint.Y);
                    }
                    else
                    {
                        l.Move(0, MinTop - l.EndPoint.Y);
                    }
                }
                else
                {
                    o.ShapeRect = new Rectangle(o.ShapeRect.Left, MinTop, o.ShapeRect.Width, o.ShapeRect.Height);
                }
            }
        }


        public void AlignBottom()
        {
            int MaxBottom = 0;
            int temp;
            int n = SelectionCount;
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    temp = Math.Max(l.StartPoint.Y, l.EndPoint.Y);
                }
                else
                {
                    temp = o.ShapeRect.Bottom;
                }
                if (temp > MaxBottom)
                {
                    MaxBottom = temp;
                }
            }
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    if (l.StartPoint.Y > l.EndPoint.Y)
                    {
                        l.Move(0, MaxBottom - l.StartPoint.Y);
                    }
                    else
                    {
                        l.Move(0, MaxBottom - l.EndPoint.Y);
                    }
                }
                else
                {
                    o.ShapeRect = new Rectangle(o.ShapeRect.Left, MaxBottom - o.ShapeRect.Height, o.ShapeRect.Width, o.ShapeRect.Height);
                }
            }
        }

        public void AlignVCenter()
        {
            int MinLeft = MAX;
            int MaxRight = 0;
            int VCenter = 0;
            int tempL;
            int tempR;
            int n = SelectionCount;
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    tempL = Math.Min(l.StartPoint.X, l.EndPoint.X);
                    tempR = Math.Max(l.StartPoint.X, l.EndPoint.X);
                }
                else
                {
                    tempL = o.ShapeRect.X;
                    tempR = o.ShapeRect.Right;
                }
                if (tempL < MinLeft)
                {
                    MinLeft = tempL;
                }
                if (tempR > MaxRight)
                {
                    MaxRight = tempR;
                }
            }
            VCenter = (MinLeft + MaxRight) / 2;
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    l.Move(VCenter - (l.StartPoint.X + l.EndPoint.X) / 2, 0);
                }
                else
                {
                    int center = (o.ShapeRect.Left + o.ShapeRect.Right) / 2;
                    o.ShapeRect = new Rectangle(o.ShapeRect.Left + (VCenter - center), o.ShapeRect.Top, o.ShapeRect.Width, o.ShapeRect.Height);
                }
            }
        }

        public void AlignHCenter()
        {
            int MinTop = MAX;
            int MaxBottom = 0;
            int HCenter = 0;
            int tempT;
            int tempB;
            int n = SelectionCount;
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    tempT = Math.Min(l.StartPoint.Y, l.EndPoint.Y);
                    tempB = Math.Max(l.StartPoint.Y, l.EndPoint.Y);
                }
                else
                {
                    tempT = o.ShapeRect.Top;
                    tempB = o.ShapeRect.Bottom;
                }
                if (tempT < MinTop)
                {
                    MinTop = tempT;
                }
                if (tempB > MaxBottom)
                {
                    MaxBottom = tempB;
                }
            }
            HCenter = (MinTop + MaxBottom) / 2;
            for (int i = 0; i < n; i++)
            {
                DrawObject o = GetSelectedObject(i);
                if (o.ObjectType == Global.DrawType.DrawLine)
                {
                    DrawLine l = (DrawLine)o;
                    l.Move(0, HCenter - (l.StartPoint.Y + l.EndPoint.Y) / 2);
                }
                else
                {
                    int center = (o.ShapeRect.Top + o.ShapeRect.Bottom) / 2;
                    o.ShapeRect = new Rectangle(o.ShapeRect.Left, o.ShapeRect.Top + (HCenter - center), o.ShapeRect.Width, o.ShapeRect.Height);
                }
            }
        }
        public void MoveSelectionToFront()
        {
            if (GetSelectedObjectIndex() >= 0)
            {
                DrawObject temp = (DrawObject)objList[GetSelectedObjectIndex()];
                objList.RemoveAt(GetSelectedObjectIndex());
                objList.Add(temp);
            }
        }
        public void MoveSelectionToBack()
        {
            if (GetSelectedObjectIndex() >= 0)
            {
                DrawObject temp = (DrawObject)objList[GetSelectedObjectIndex()];
                objList.RemoveAt(GetSelectedObjectIndex());
                objList.Insert(0, temp);
            }
        }

        public int GetSelectedObjectIndex()
        {
            int n;
            n = objList.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                DrawObject o = (DrawObject)objList[i];

                if (o.Selected)
                    return i;
            }
            return -1;
        }


        public void Copy()
        {
            int n = SelectionCount;
            tempList.Clear();
            for (int i = 0; i < n; i++)
            {
                DrawObject o = (DrawObject)GetSelectedObject(i);
                tempList.Add(o);
            }
        }
        public void Clone(VisualGraph drawArea)
        {
            int n = tempList.Count;
            Point point;

            for (int i = 0; i < n; i++)
            {
                DrawObject o = (DrawObject)tempList[i];
                point = new Point(o.ShapeRect.X + 10, o.ShapeRect.Y + 10);
                switch (o.ObjectType)
                {
                    case Global.DrawType.DrawEllipse:
                        DrawEllipse dp = new DrawEllipse(point, drawArea);
                        dp.Selected = false;
                        dp.ShapeRect = new Rectangle(point.X, point.Y, o.ShapeRect.Width, o.ShapeRect.Height);
                        dp.PenWidth = o.PenWidth;
                        dp.Color = o.Color;
                        objList.Add(dp);
                        break;
                    case Global.DrawType.DrawLine:
                        DrawLine dl = new DrawLine(point, drawArea);
                        dl.Selected = false;
                        dl.StartPoint = point;
                        dl.EndPoint = new Point(point.X + o.ShapeRect.Width, point.Y + o.ShapeRect.Height);
                        dl.Width = o.ShapeRect.Width;
                        dl.Height = o.ShapeRect.Height;
                        dl.PenWidth = o.PenWidth;
                        dl.Color = o.Color;
                        objList.Add(dl);
                        break;
                    case Global.DrawType.DrawRectangle:
                        DrawRectangle dr = new DrawRectangle(point, drawArea);
                        dr.Selected = false;
                        dr.ShapeRect = new Rectangle(point.X, point.Y, o.ShapeRect.Width, o.ShapeRect.Height);
                        dr.PenWidth = o.PenWidth;
                        dr.Color = o.Color;
                        objList.Add(dr);
                        break;
                    case Global.DrawType.DrawText:
                        DrawText dt = new DrawText(point, drawArea);
                        dt.Selected = false;
                        dt.ShapeRect = new Rectangle(point.X, point.Y, o.ShapeRect.Width, o.ShapeRect.Height);
                        dt.PenWidth = o.PenWidth;
                        dt.Color = o.Color;
                        objList.Add(dt);
                        break;
                    case Global.DrawType.DrawPic:
                        DrawPic oldpic = (DrawPic)o;
                        DrawPic dpic = new DrawPic(point, drawArea);
                        Rectangle ret = new Rectangle(point.X, point.Y, oldpic.ShapeRect.Width, oldpic.ShapeRect.Height);
                        dpic.ShapeRect = ret;
                        dpic.TheImage = oldpic.TheImage;
                        dpic.Selected = false;
                        objList.Add(dpic);
                        break;
                }
            }
            drawArea.ActivedObjType = Global.DrawType.POINTER;
        }
        public bool DeleteSelection() //É¾³ý
        {
            bool result = false;
            int n = objList.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                if (((DrawObject)objList[i]).Selected)
                {
                    DrawObject o = (DrawObject)objList[i];
                    objList.RemoveAt(i);
                    result = true;

                }
            }
            return result;
        }


    }
}
