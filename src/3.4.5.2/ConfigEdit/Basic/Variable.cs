using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    public enum DataType
    {
      bit,
    int16, 
    uint16,
    int32,
    uint32,
    Float
    }
    public class Variable
    {
        private string mName="";
        private string mAddr="";
        private string mDescription="";
        private object mValue;
        private object mOldValue;
        private string mDevice;
        public DataType mValuetype;
        public float offseta = 0;
        public float offsetb = 0;
        public float factor = 1;
        public float Uper;
        public float Lower;
        public bool bWrite = false;
        public bool bAlert = false;
        public bool bEnableAlert = false;
        public object mWriteValue;
        public DateTime Datatime;
        public delegate void DataEventHandler(Object sender);
        public event DataEventHandler Datachanged; //ÉùÃ÷ÊÂ¼þ
        public Variable()
        {

        }
        public void OnDataChange(Object sender)
        {
            Datachanged(sender);
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
