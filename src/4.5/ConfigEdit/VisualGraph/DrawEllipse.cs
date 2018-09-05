using System;
using System.Windows.Forms;
using System.Drawing;
using Basic;
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
        //¶¯»­×¢²áº¯Êý
        public override void SetAction(Object sender)
        {
            Variable var = (Variable)sender;
             if (xName.Equals(var.Name))
            {
                Rectangle ret = new Rectangle(Convert.ToInt32(var.Value), ShapeRect.Y, ShapeRect.Width, ShapeRect.Height);
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
                Rectangle ret = new Rectangle(ShapeRect.X, ShapeRect.Y, ShapeRect.Width, Convert.ToInt32(var.Value));
                ShapeRect = ret;
            }
            else if (visibleName.Equals(var.Name))
            {

            }
        }
        public override void Draw(Graphics g, VisualGraph drawArea)
        {
            Pen pen = new Pen(Color, PenWidth);
            g.DrawEllipse(pen, DrawRectangle.GetNormalizedRectangle(ShapeRect));
            pen.Dispose();
        }

    }
}
