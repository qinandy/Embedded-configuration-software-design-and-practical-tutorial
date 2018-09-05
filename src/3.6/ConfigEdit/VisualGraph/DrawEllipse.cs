using System;
using System.Windows.Forms;
using System.Drawing;

namespace VisualGraph
{
    [Serializable]
    public class DrawEllipse : DrawRectangle
    {
        public DrawEllipse(Point point, VisualGraph drawArea)
        {

            ObjName = drawArea.CreateObjName();

            ShapeRect = new Rectangle(point.X, point.Y, Width, Height);
            ObjectType = Global.DrawType.DrawEllipse;
            GenerateID(Global.DrawType.DrawEllipse);
            //Initialize();
        }

        public override void Draw(Graphics g, VisualGraph drawArea)
        {
            Pen pen = new Pen(Color, PenWidth);
            g.DrawEllipse(pen, DrawRectangle.GetNormalizedRectangle(ShapeRect));
            pen.Dispose();
        }

    }
}
