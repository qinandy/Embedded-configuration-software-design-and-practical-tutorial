using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
namespace VisualGraph
{
    public partial class VisualGraph : UserControl
    {
        private Global.DrawType activedObjType;
        private DrawBaseTool tools;

        private ObjList objList;
        private bool drawNetRectangle = false;
        private Rectangle netRectangle;
        private bool showGrid;
        private Size gridSize = new Size(10, 10);
        private float m_Scalef = 1.0f;

        public VisualGraph()
        {
            InitializeComponent();
            ObjList = new ObjList();
            tools = new SelectTool();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            ActivedObjType = Global.DrawType.POINTER;
        }
        public Global.DrawType ActivedObjType
        {
            get
            {
                return activedObjType;
            }
            set
            {
                activedObjType = value;
            }
        }
        public bool DrawNetRectangle
        {
            get
            {
                return drawNetRectangle;
            }
            set
            {
                drawNetRectangle = value;
            }
        }
        public Rectangle NetRectangle
        {
            get
            {
                return netRectangle;
            }
            set
            {
                netRectangle = value;
            }
        }
        public ObjList ObjList
        {
            get
            {

                return objList;
            }
            set
            {
                objList = value;
            }
        }


        public bool ShowGrid
        {
            get
            {
                return showGrid;
            }
            set
            {
                showGrid = value;
            }
        }

        public Size GridSize
        {
            get
            {
                return gridSize;
            }
            set
            {
                gridSize = value;
            }
        }

        public float M_Scalef
        {
            get
            {
                return m_Scalef;
            }
            set
            {
                m_Scalef = value;
            }
        }
        public void DrawSelectAll(Graphics g)
        {
            if (!DrawNetRectangle)
                return;
            ControlPaint.DrawFocusRectangle(g, NetRectangle, Color.Black, Color.Transparent);
        }
        private Color _backColor = Color.White;
        public Color BackGroundColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                this.Refresh();
            }
        }
        private void VisualGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.PageUnit = GraphicsUnit.Pixel;
            e.Graphics.PageScale = M_Scalef;

            SolidBrush brush = new SolidBrush(_backColor);
            e.Graphics.FillRectangle(brush, this.ClientRectangle);
            if (ShowGrid)
            {
                ControlPaint.DrawGrid(e.Graphics, this.ClientRectangle, gridSize, this.BackColor);
            }
            if (objList != null)
            {
                ObjList.Draw(e.Graphics, this);
            }
            DrawSelectAll(e.Graphics);
            brush.Dispose();
        }

        private void VisualGraph_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tools.OnMouseDown(this, e);
            }

        }

        private void VisualGraph_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.None)
            {

                tools.OnMouseMove(this, e);
            }

            else
                this.Cursor = Cursors.Default;
        }

        private void VisualGraph_MouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
                tools.OnMouseUp(this, e);
        }
        public String CreateObjName()
        {
            int No = 1;
            for (int n = 0; n <= ObjList.Count() - 1; n++)
            {

                if (ObjList[n].ObjName == (ActivedObjType.ToString() + No.ToString()))
                {
                    No = No + 1;
                }
            }
            return ActivedObjType.ToString() + No.ToString();
        }

        private void VisualGraph_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void VisualGraph_DragDrop(object sender, DragEventArgs e)
        {
            Point point = new Point(e.X, e.Y);
            point = this.PointToClient(point);
            switch (activedObjType)
            {
                case Global.DrawType.DrawText:
                    DrawText text = new DrawText(point, this);
                    objList.UnselectAll();
                    objList.AddObject(text);
                    text.Selected = true;
                    break;
                case Global.DrawType.DrawRectangle:
                    DrawRectangle rect = new DrawRectangle(point, this);
                    objList.UnselectAll();
                    objList.AddObject(rect);
                    rect.Selected = true;
                    break;
                case Global.DrawType.DrawEllipse:
                    DrawEllipse elip = new DrawEllipse(point, this);
                    objList.UnselectAll();
                    objList.AddObject(elip);
                    elip.Selected = true;
                    break;
                case Global.DrawType.DrawLine:
                    DrawLine line = new DrawLine(point, this);
                    objList.UnselectAll();
                    objList.AddObject(line);
                    line.Selected = true;
                    break;
                case Global.DrawType.DrawPic:
                    DrawPic pic = new DrawPic(point, this);
                    objList.UnselectAll();
                    objList.AddObject(pic);
                    pic.Selected = true;
                    break;
            }
            activedObjType = Global.DrawType.POINTER;
            this.Refresh();
        }

    }
}