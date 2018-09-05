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

    }
}