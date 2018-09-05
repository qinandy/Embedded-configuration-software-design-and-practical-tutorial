using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    [Serializable]
    public class ChannelManage
    {
        private string _rtucom = "COM1";
        private string rename;
        private int refreshtime = 300;
        private UInt32 _Baud = 9600;
        private uint _databit = 8;
        private uint _stopbit = 1;
        private uint _checkbit = 0;
        public int nChannelType = 0;//0ÐéÄâµÄ£¬1£¬´®¿Ú
        private List<DeviceManage>  _DevList;
        public bool bConnected = false; 
        public  ChannelManage()
        {
            AllDevList = new List<DeviceManage>();

        }
        public List<DeviceManage> AllDevList
        {
            get
            {
                return _DevList;
            }
            set
            {
                _DevList = value;
            }
        }
         public string ComPort
        {
            get
            {
                return _rtucom;
            }
            set
            {
                _rtucom = value;
            }
        }
        public UInt32 BaudRate
        {
            get
            {
                return _Baud;
            }
            set
            {
                _Baud = value;
            }
        }
        public uint Databits
        {
            get
            {
                return _databit;
            }
            set
            {
                _databit = value;
            }
        }
        public uint Stopbit
        {
            get
            {
                return _stopbit;
            }
            set
            {
                _stopbit = value;
            }
        }
        public uint Parity
        {
            get
            {
                return _checkbit;
            }
            set
            {
                _checkbit = value;
            }
        }
        public int RefreshTime
        {
            get
            {
                return refreshtime;
            }
            set
            {
                refreshtime = value;
            }
        }
        public string ChannelName
        {
            get
            {
                return rename;
            }
            set
            {
                rename = value;
            }
        }
    }
}
