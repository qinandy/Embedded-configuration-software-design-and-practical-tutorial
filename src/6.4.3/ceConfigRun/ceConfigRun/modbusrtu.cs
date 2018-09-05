using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
namespace ceConfigRun
{
    public class modbusrtu
    {
         private SerialPort sp = new SerialPort();
       
        public string modbusStatus;
        public string SendData;
        public string RecvData;
        public modbusrtu()
        {
        }    

        #region Open / Close Procedures
        public bool Open(string portName, int baudRate, int databits, Parity parity, StopBits stopBits)
        {
            //Ensure port isn't already opened:
            if (!sp.IsOpen)
            {
                //Assign desired settings to the serial port:
                sp.PortName = portName;
                sp.BaudRate = baudRate;
                sp.DataBits = databits;
                sp.Parity = parity;
                sp.StopBits = stopBits;
                //These timeouts are default and cannot be editted through the class at this point:
                sp.ReadTimeout = 1000;
                sp.WriteTimeout = 1000;

                try
                {
                    sp.Open();
                }
                catch (Exception err)
                {
                    modbusStatus = "Error opening " + portName + ": " + err.Message;
                    return false;
                }
                modbusStatus = portName + " opened successfully";
                return true;
            }
            else
            {
                modbusStatus = portName + " already opened";
                return false;
            }
        }
        public bool Close()
        {
            //Ensure port is opened before attempting to close:
            if (sp.IsOpen)
            {
                try
                {
                    sp.Close();
                }
                catch (Exception err)
                {
                    modbusStatus = "Error closing " + sp.PortName + ": " + err.Message;
                    return false;
                }
                modbusStatus = sp.PortName + " closed successfully";
                return true;
            }
            else
            {
                modbusStatus = sp.PortName + " is not open";
                return false;
            }
        }
        #endregion

        #region CRC Computation
        private void GetCRC(byte[] message, ref byte[] CRC)
        {
            //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
            //return the CRC values:

            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < (message.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }
        #endregion

        #region Build Message
        private void BuildMessage(byte address, byte type, ushort start, ushort registers, ref byte[] message)
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];

            message[0] = address;
            message[1] = type;
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;

            GetCRC(message, ref CRC);
            message[message.Length - 2] = CRC[0];
            message[message.Length - 1] = CRC[1];
        }
        #endregion

        #region Check Response
        private bool CheckResponse(byte[] response)
        {
            //Perform a basic CRC check:
            byte[] CRC = new byte[2];
            GetCRC(response, ref CRC);
            if (CRC[0] == response[response.Length - 2] && CRC[1] == response[response.Length - 1])
                return true;
            else
                return false;
        }
        #endregion

        #region Get Response
        private void GetResponse(ref byte[] response)
        {
            //There is a bug in .Net 2.0 DataReceived Event that prevents people from using this
            //event as an interrupt to handle data (it doesn't fire all of the time).  Therefore
            //we have to use the ReadByte command for a fixed length as it's been shown to be reliable.
            for (int i = 0; i < response.Length; i++)
            {
                response[i] = (byte)(sp.ReadByte());
            }
        }
        #endregion
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }
        //write DO
        public bool SendFc5(byte address, ushort start, short values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
              
                byte[] message = new byte[8];
                //Function 16 response is fixed at 8 bytes
                byte[] response = new byte[8];
               
                //Build outgoing message:
                byte[] CRC = new byte[2];

                message[0] = address;
                message[1] = (byte)5;
                message[2] = (byte)(start >> 8);
                message[3] = (byte)start;
                if (values == 1)
                {
                    message[4] = (byte)(0xff);
                    message[5] = (byte)0x00;
                }
                else
                {
                    message[4] = (byte)0;
                    message[5] = (byte)0;
                }
                GetCRC(message, ref CRC);
                message[message.Length - 2] = CRC[0];
                message[message.Length - 1] = CRC[1];

                //Send Modbus message to Serial Port:
                try
                { 
                    //SendData = ByteArrayToHexString(message);
                    sp.Write(message, 0, message.Length);             
                    GetResponse(ref response);
                    //RecvData = ByteArrayToHexString(response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in write event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    modbusStatus = "Write successful";
                    return true;
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }
        }
        #region Function 16 - Write Multiple Registers
        //write AO
        public bool SendFc16(byte address, ushort start, ushort registers, ushort[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Message is 1 addr + 1 fcn + 2 start + 2 reg + 1 count + 2 * reg vals + 2 CRC
                byte[] message = new byte[9 + 2 * registers];
                //Function 16 response is fixed at 8 bytes
                byte[] response = new byte[8];

                //Add bytecount to message:
                message[6] = (byte)(registers * 2);
                //Put write values into message prior to sending:
                for (int i = 0; i < registers; i++)
                {
                    message[7 + 2 * i] = (byte)(values[i] >> 8);
                    message[8 + 2 * i] = (byte)(values[i]);
                }
                //Build outgoing message:
                BuildMessage(address, (byte)16, start, registers, ref message);
                
                //Send Modbus message to Serial Port:
                try
                { 
                   // SendData = ByteArrayToHexString(message);
                    sp.Write(message, 0, message.Length);              
                    GetResponse(ref response);
                   // RecvData = ByteArrayToHexString(response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in write event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    modbusStatus = "Write successful";
                    return true;
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }
        }
        #endregion
        //read DO 
        public bool SendFc1(byte address, ushort start, ushort registers, ref ushort[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Function 1 request is always 8 bytes:
                byte[] message = new byte[8];
                //Function 3 response buffer:
                byte[] response = new byte[5 + (registers + 7) / 8];
                //Build outgoing modbus message:
                BuildMessage(address, (byte)1, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                   // SendData = ByteArrayToHexString(message);
                    sp.Write(message, 0, message.Length);         
                    GetResponse(ref response);
                   // RecvData = ByteArrayToHexString(response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in read event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    //看看返回的地址和请求的地址是不是相同
                    if ((response[0] != message[0] )|| response[1]!=message[1])
                    {
                        return false;
                    }
                    else
                    {
                        //Return requested register values:  
                        int nBit = 0;
                        int nRespByte = 3;
                        for (int nCoil = 0; nCoil < registers; nCoil++)
                        {

                            if (nBit > 7)
                            {
                                nBit = 0;
                                nRespByte++;
                            }

                            if ((response[nRespByte] & (0x01 << nBit)) > 0)
                            {
                                values[nCoil] = 1;
                            }
                            else
                            {
                                values[nCoil] = 0;
                            }
                            nBit++; //next bit 

                        }// end for 

                    }// else 
                 
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }

        }
        //read DI
        public bool SendFc2(byte address, ushort start, ushort registers, ref ushort[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Function 1 request is always 8 bytes:
                byte[] message = new byte[8];
                //Function 3 response buffer:
                byte[] response = new byte[5 + (registers + 7) / 8];
                //Build outgoing modbus message:
                BuildMessage(address, (byte)2, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                   // SendData = ByteArrayToHexString(message);
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                   // RecvData = ByteArrayToHexString(response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in read event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    //看看返回的地址和请求的地址是不是相同
                    if ((response[0] != message[0]) || response[1] != message[1])
                    {
                        return false;
                    }
                    else
                    {
                        //Return requested register values:  
                        int nBit = 0;
                        int nRespByte = 3;
                        for (int nCoil = 0; nCoil < registers; nCoil++)
                        {

                            if (nBit > 7)
                            {
                                nBit = 0;
                                nRespByte++;
                            }

                            if ((response[nRespByte] & (0x01 << nBit)) > 0)
                            {
                                values[nCoil] = 1;
                            }
                            else
                            {
                                values[nCoil] = 0;
                            }
                            nBit++; //next bit 

                        }// end for 

                    }// else 

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }

        }
        #region Function 3 - Read Registers
        //read AO
        public bool SendFc3(byte address, ushort start, ushort registers, ref ushort[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Function 3 request is always 8 bytes:
                byte[] message = new byte[8];
                //Function 3 response buffer:
                byte[] response = new byte[5 + 2 * registers];
                //Build outgoing modbus message:
                BuildMessage(address, (byte)3, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                  //  SendData = ByteArrayToHexString(message);
                    sp.Write(message, 0, message.Length);            
                    GetResponse(ref response);
                  //  RecvData = ByteArrayToHexString(response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in read event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    //Return requested register values:
                    for (int i = 0; i < (response.Length - 5) / 2; i++)
                    {
                        values[i] = response[2 * i + 3];
                        values[i] <<= 8;
                        values[i] += response[2 * i + 4];
                    }
                    modbusStatus = "Read successful";
                    return true;
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }

        }
        #endregion
        //read AI
        public bool SendFc4(byte address, ushort start, ushort registers, ref ushort[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Function 3 request is always 8 bytes:
                byte[] message = new byte[8];
                //Function 3 response buffer:
                byte[] response = new byte[5 + 2 * registers];
                //Build outgoing modbus message:
                BuildMessage(address, (byte)4, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                 //   SendData = ByteArrayToHexString(message);
                    sp.Write(message, 0, message.Length);             
                    GetResponse(ref response);
                  //  RecvData = ByteArrayToHexString(response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in read event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    //Return requested register values:
                    for (int i = 0; i < (response.Length - 5) / 2; i++)
                    {
                        values[i] = response[2 * i + 3];
                        values[i] <<= 8;
                        values[i] += response[2 * i + 4];
                    }
                    modbusStatus = "Read successful";
                    return true;
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }

        }
    }
}
