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
            MoveSelectionToFront();
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
