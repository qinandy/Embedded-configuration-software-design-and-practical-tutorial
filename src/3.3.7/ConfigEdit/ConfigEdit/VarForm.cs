using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConfigEdit
{
    public partial class VarForm : Form
    {
        public VarForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        public string VarExpress
        {
            get
            {
                return this.textBox1.Text;
            }
            set
            {
                this.textBox1.Text = value;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void VarForm_Load(object sender, EventArgs e)
        {
             ListViewItem item = new ListViewItem();
             item = listView1.Items.Add("var1");
   
             item.SubItems.Add("虚拟设备");
             item.SubItems.Add("模拟的虚拟设备变量");

              item = new ListViewItem();
             item = listView1.Items.Add("var2");

             item.SubItems.Add("虚拟设备");
             item.SubItems.Add("模拟的虚拟设备变量");
             listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                string name = listView1.SelectedItems[0].SubItems[0].Text;
                VarExpress = name;
            }
        }
    }
}