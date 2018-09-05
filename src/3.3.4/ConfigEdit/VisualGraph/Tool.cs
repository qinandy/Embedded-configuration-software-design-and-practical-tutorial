using System;
using System.Windows.Forms;
using System.Drawing;

namespace VisualGraph
{
    public abstract class DrawBaseTool
    {
        public virtual void OnMouseDown(VisualGraph drawArea, MouseEventArgs e)
        {
        }
        public virtual void OnMouseMove(VisualGraph drawArea, MouseEventArgs e)
        {
        }
        public virtual void OnMouseUp(VisualGraph drawArea, MouseEventArgs e)
        {
        }
    }
}
