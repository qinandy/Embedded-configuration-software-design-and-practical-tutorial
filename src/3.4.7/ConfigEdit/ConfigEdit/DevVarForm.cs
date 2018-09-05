using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Basic;
namespace ConfigEdit
{
    public partial class DevVarForm : Form
    {
        private DeviceManage devlist;
        private int nVirtualType = -1;
        public DevVarForm()
        {
            InitializeComponent();
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
        }
     
        public string DevAddr
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
        public string VarName
        {
            get
            {
                return this.textBox2.Text;
            }
            set
            {
                this.textBox2.Text = value;
            }
        }
        public string VarAddr
        {
            get
            {
                return this.textBox3.Text;
            }
            set
            {
                this.textBox3.Text = value;
            }
        }
        public string ReadNum
        {
            get
            {
                return this.textBox6.Text;
            }
            set
            {
                this.textBox6.Text = value;
            }
        }
        public string VarDes
        {
            get
            {
                return this.textBox7.Text;
            }
            set
            {
                this.textBox7.Text = value;
            }
        }
        public string Factor
        {
            get
            {
                return this.textBox5.Text;
            }
            set
            {
                this.textBox5.Text = value;
            }
        }
        public string Offset
        {
            get
            {
                return this.textBox4.Text;
            }
            set
            {
                this.textBox4.Text = value;
            }
        }
        public int varType
        {
            get 
            {
                return this.comboBox1.SelectedIndex;
            }
            set
            {
                this.comboBox1.SelectedIndex = value;
            }
        }
        public DeviceManage DevList
        {
            get
            {
                return this.devlist;
            }
            set
            {
                this.devlist = value;
            }
        }
        public int varWrite
        {
            get
            {
                return this.comboBox2.SelectedIndex;
            }
            set
            {
                this.comboBox2.SelectedIndex = value;
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void DevVarForm_Load(object sender, EventArgs e)
        {
            if (devlist != null)
            {
                listViewVar.Items.Clear();
                DevAddr = devlist.RtuAddr;             
                foreach (Variable var in devlist.AllVarList)
                 {
                      ListViewItem item = new ListViewItem();
                      item = listViewVar.Items.Add(var.Name);             
                      item.SubItems.Add(var.Addr);
                      item.SubItems.Add(var.readNum.ToString());               
                      item.SubItems.Add(var.mValuetype.ToString());
                      if (var.bWrite)
                      {
                          item.SubItems.Add("True");
                      }
                      else
                      {
                          item.SubItems.Add("False");
                      }
                      item.SubItems.Add(var.Description);
                      item.SubItems.Add(var.factor.ToString());
                      item.SubItems.Add(var.offset.ToString());
                  }               
                listViewVar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listViewVar.SelectedItems.Count != 1)		// no selection
                return;
            //delete from listview
            ListViewItem selitem = listViewVar.SelectedItems[0];
            listViewVar.Items.Remove(selitem);
            //delete from varlist
            foreach(Variable v in devlist.AllVarList)
            {
                if (v.Name.Equals(selitem.Text))
                {
                    devlist.AllVarList.Remove(v);
                    break;
                }
            }
          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //add to listview
            ListViewItem item = new ListViewItem();
            item = listViewVar.Items.Add(VarName);
            item.SubItems.Add(VarAddr);
            item.SubItems.Add(ReadNum);
            item.SubItems.Add(comboBox1.Text);       
            item.SubItems.Add(comboBox2.Text);     
            item.SubItems.Add(VarDes);
            item.SubItems.Add(Factor);
            item.SubItems.Add(Offset);
            //add to dev
            Variable var=new Variable();
            var.Name=VarName;
            var.Addr=VarAddr;
            var.readNum=Convert.ToInt32(ReadNum);
            if (comboBox1.Text.Equals("bit"))
            {
                 var.mValuetype=DataType.bit;
            }
            else if (comboBox1.Text.Equals("int16"))
            {
                var.mValuetype=DataType.int16;
            }
             else if (comboBox1.Text.Equals("uint16"))
            {
                var.mValuetype=DataType.uint16;
            }
             else if (comboBox1.Text.Equals("int32"))
            {
                var.mValuetype=DataType.int32;
            }
              else if (comboBox1.Text.Equals("uint32"))
            {
                var.mValuetype=DataType.uint32;
            }
              else if (comboBox1.Text.Equals("Float"))
            {
                var.mValuetype=DataType.Float;
            }
            if (comboBox2.Text.Equals("True"))
            {
                var.bWrite=true;
            }
            else if (comboBox2.Text.Equals("False"))
            {
                var.bWrite=false;
            }
            var.Description=VarDes;
            var.factor=Convert.ToSingle(Factor);
            var.offset=Convert.ToSingle(Offset);
            var.Device = devlist.DeviceName;
            devlist.RtuAddr = DevAddr;
            //虚拟变量
            var.VirtualVarType = nVirtualType;
            devlist.AllVarList.Add(var);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            nVirtualType = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            nVirtualType = 1;
        }

        private void listViewVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewVar.SelectedItems.Count != 1)		// no selection
                return;

            ListViewItem selitem = listViewVar.SelectedItems[0];
            VarName = selitem.Text;
            VarAddr = selitem.SubItems[1].Text;
            ReadNum = selitem.SubItems[2].Text;
            //VarAddr = selitem.SubItems[3].Text;
            //VarAddr = selitem.SubItems[4].Text;
           //其他属性显示，不在描述了，读者自行完善
        }
    }
}