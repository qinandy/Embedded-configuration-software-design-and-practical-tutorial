using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConfigEdit
{
    public partial class DeviceSetForm : Form
    {
        public DeviceSetForm()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        private string _com = "";
        public string Com
        {
            get
            {
                return this.comboBox1.Text;
            }
            set
            {
                _com=value;
                if (_com.Equals("COM1"))
                {
                    comboBox1.SelectedIndex = 0;
                }
                else if (_com.Equals("COM2"))
                {
                    comboBox1.SelectedIndex = 1;
                }
                else if (_com.Equals("COM3"))
                {
                    comboBox1.SelectedIndex = 2;
                }
                else if (_com.Equals("COM4"))
                {
                    comboBox1.SelectedIndex = 3;
                }
            }
        }
        private UInt32 _baud = 9600;
        public UInt32 Baud
        {
            get
            {
                return Convert.ToUInt32(this.comboBox2.Text);
            }
            set
            {
                _baud=value;
                if (_baud==9600)
                {
                    comboBox2.SelectedIndex = 0;
                }
                else if (_baud==19200)
                {
                    comboBox2.SelectedIndex = 1;
                }
                else if (_baud==115200)
                {
                    comboBox2.SelectedIndex = 2;
                }
            }
        }
        public int nRefresh
        {
            get
            {
                return Convert.ToInt32(this.textBox1.Text);
            }
            set
            {
                this.textBox1.Text = value.ToString();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}