using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    [Serializable]
    public enum DataType
    {
      bit,
    int16, 
    uint16,
    int32,
    uint32,
    Float
    }
    [Serializable]
    public class Variable
    {
        private string mName="";
        private string mAddr="";
        private string mDescription="";
        private object mValue="";
        private object mOldValue="";
        private string mDevice;
        public DataType mValuetype;
        public float offset = 0;
        public float factor = 1;
        public float Counter = 0;
        public bool bWrite = false;
        public object mWriteValue;
        public DateTime Datatime;
        public int VirtualVarType=0;//0,递增，1，sin函数，当然可随意添加各种虚拟值
        public int readNum = 1;
        public delegate void DataEventHandler(Object sender);
        public event DataEventHandler Datachanged; //声明事件
        public Variable()
        {
            mValue = 0;
        }
        public void OnDataChange(Object sender)
        {
            if (Datachanged!=null)
            {  
                Datachanged(sender);
            }
          
        }
        public string Name
        {
            get
            {
                return this.mName;
            }
            set
            {
                this.mName = value;
            }
        }
        public string Addr
        {
            get
            {
                return this.mAddr;
            }
            set
            {
                this.mAddr = value;
            }
        }

        public string Description
        {
            get
            {
                return this.mDescription;
            }
            set
            {
                this.mDescription = value;
            }
        }
        public object Value
        {
            get
            {
                return this.mValue;
            }
            set
            {
                this.mValue = value;
            }
        }
        public object OldValue
        {
            get
            {
                return this.mOldValue;
            }
            set
            {
                this.mOldValue = value;
            }
        }
        public string Device
        {
            get
            {
                return this.mDevice;
            }
            set
            {
                this.mDevice = value;
            }
        }
    }
}
