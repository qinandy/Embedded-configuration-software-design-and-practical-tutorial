using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    public class DeviceManage
    {
        private string _rtuaddr = "1";
      
        private string devivename;
        private string devinfo;
    
        private List<Variable> varlist;
        public DeviceManage()
        {
            AllVarList = new List<Variable>();
            DeviceName = "";
        }
     
        public string RtuAddr
        {
            get
            {
                return _rtuaddr;
            }
            set
            {
                _rtuaddr = value;
            }
        }
        public string Devinfo
        {
            get
            {
                return devinfo;
            }
            set
            {
                devinfo = value;
            }
        }
        public List<Variable> AllVarList
        {
            get
            {
                return varlist;
            }
            set
            {
                varlist = value;
            }
        }
        public string DeviceName
        {
            get
            {
                return devivename;
            }
            set
            {
                devivename = value;
            }
        }
    }
}
