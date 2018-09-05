using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Drawing.Drawing2D;
namespace VisualGraph
{

    public class SelectTool : DrawBaseTool
    {
        private enum SelectionMode
        {
            None,
            NetSelection,
            Move,
            Size
        }
        private SelectionMode selectMode = SelectionMode.None;
        private DrawObject resizedObject;
        private int resizedObjectHandle;
        private Point lastPoint = new Point(0, 0);
        private Point startPoint = new Point(0, 0);


        public SelectTool()
        {

        }

        public override void OnMouseDown(VisualGraph drawArea, MouseEventArgs e)
        {

            Point point = new Point(e.X, e.Y);
            selectMode = SelectionMode.None;

            int n = drawArea.ObjList.SelectionCount;
            for (int i = n - 1; i >= 0; i--)
            {
                DrawObject o = drawArea.ObjList.GetSelectedObject(i);

                int handleNumber = o.HitTest(point);
                if (handleNumber > 0)
                {
                    selectMode = SelectionMode.Size;
                    resizedObject = o;
                    resizedObjectHandle = handleNumber;
                    drawArea.ObjList.UnselectAll();
                    o.Selected = true;

                    break;
                }
            }

            if (selectMode == SelectionMode.None)
            {
                int m = drawArea.ObjList.Count();
                DrawObject o = null;
                for (int i = m - 1; i >= 0; i--)
                {
                    if (drawArea.ObjList[i].HitTest(point) == 0)
                    {
                        o = drawArea.ObjList[i];
                        break;
                    }
                }

                if (o != null)
                {
                    selectMode = SelectionMode.Move;
                    if ((Control.ModifierKeys & Keys.Control) == 0 && !o.Selected)
                        drawArea.ObjList.UnselectAll();
                    o.Selected = true;
                    //»­Ñ¡Ôñ±ß¿ò
                    drawArea.Cursor = Cursors.SizeAll;
                }
            }
            if (selectMode == SelectionMode.None)
            {
                if ((Control.ModifierKeys & Keys.Control) == 0)
                    drawArea.ObjList.UnselectAll();
                selectMode = SelectionMode.NetSelection;
                drawArea.DrawNetRectangle = true;
            }
            lastPoint.X = point.X;
            lastPoint.Y = point.Y;
            startPoint.X = point.X;
            startPoint.Y = point.Y;

            drawArea.Capture = true;
            drawArea.NetRectangle = GetNormalizedRectangle(startPoint, lastPoint);

            drawArea.Refresh();
        }

        public override void OnMouseMove(VisualGraph drawArea, MouseEventArgs e)
        {

            Point point = new Point(e.X, e.Y);
            if (e.Button == MouseButtons.None)
            {
                Cursor cursor = null;
                for (int i = drawArea.ObjList.Count() - 1; i >= 0; i--)
                {
                    int n = drawArea.ObjList[i].HitTest(point);
                    if (n > 0)
                    {
                        cursor = drawArea.ObjList[i].GetHandleCursor(n);
                        break;
                    }


                }
                if (cursor == null)
                    cursor = Cursors.Default;
                drawArea.Cursor = cursor;
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            int dx = point.X - lastPoint.X;
            int dy = point.Y - lastPoint.Y;

            lastPoint.X = point.X;
            lastPoint.Y = point.Y;
            if (selectMode == SelectionMode.Size)
                if (resizedObject != null)
                {
                    resizedObject.MoveHandleTo(point, resizedObjectHandle);
                    drawArea.Refresh();
                }


            if (selectMode == SelectionMode.Move)
            {
                int n = drawArea.ObjList.SelectionCount;

                for (int i = n - 1; i >= 0; i--)
                {
                    drawArea.ObjList.GetSelectedObject(i).Move(dx, dy);
                }
                DrawObject l = (DrawObject)drawArea.ObjList.GetSelectedObject(point);
                if (l != null)
                {
                    drawArea.Cursor = Cursors.SizeAll;
                }
                drawArea.Refresh();
            }
            if (selectMode == SelectionMode.NetSelection)
            {
                drawArea.NetRectangle = GetNormalizedRectangle(startPoint, lastPoint);
                drawArea.Refresh();
                return;
            }
        }

        public override void OnMouseUp(VisualGraph drawArea, MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);
            if (selectMode == SelectionMode.NetSelection)
            {
                drawArea.ObjList.SelectInRectangle(drawArea.NetRectangle);
                selectMode = SelectionMode.None;
                drawArea.DrawNetRectangle = false;
            }
            if (resizedObject != null)
            {
                // after resizing
                resizedObject.Normalize();
                resizedObject = null;
            }
            selectMode = SelectionMode.None;
            drawArea.Refresh();

        }

        protected Rectangle GetNormalizedRectangle(Point startPoint, Point lastPoint)
        {
            int temp;
            if (lastPoint.X < startPoint.X)
            {
                temp = startPoint.X;
                startPoint.X = lastPoint.X;
                lastPoint.X = temp;
            }
            if (lastPoint.Y < startPoint.Y)
            {
                temp = startPoint.Y;
                startPoint.Y = lastPoint.Y;
                lastPoint.Y = temp;
            }
            return new Rectangle(startPoint.X, startPoint.Y, lastPoint.X - startPoint.X, lastPoint.Y - startPoint.Y);

        }

    }
}
