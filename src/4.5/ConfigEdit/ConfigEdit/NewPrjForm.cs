using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConfigEdit
{
    public partial class NewPrjForm : Form
    {
        public NewPrjForm()
        {
            InitializeComponent();
        }
        public string prjName
        {
            get
            {
                return this.textBox1.Text;
            }
        }
        public string prjPath
        {
            get
            {
                return this.textBox2.Text;
            }
        }
        private void NewPrjForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = "YourPrjName";
            textBox2.Text = "c:\\";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }
    }
}