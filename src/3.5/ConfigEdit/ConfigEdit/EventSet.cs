using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConfigEdit
{
    public partial class EventSet : Form
    {
        private string _mEventType = "";
        public EventSet()
        {
            InitializeComponent();
        }
        public string sContent
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
        public ComboBox mCombox
        {
            get
            {
                return this.comboBox1;
            }
            set
            {
                this.comboBox1 = value;
            }
        }
        public string sComboxContent
        {
            get
            {
                return _mEventType;
            }
            set
            {
                _mEventType = value;
                if (_mEventType == "打开画面")
                {
                    comboBox1.SelectedIndex = 0;
                }
                else if (_mEventType == "写变量值")
                {
                    comboBox1.SelectedIndex = 1;
                }
                else
                {
                    comboBox1.SelectedIndex = -1;
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
               
                case 1:
                    _mEventType = "写变量值";
                    textBox1.Text = "";
                    break;
                case 0:
                    _mEventType = "打开画面";
                    textBox1.Text = "";
                    break;
                default:
                    _mEventType = "";
                    textBox1.Text = "";
                    break;
            }
        }
    }
}