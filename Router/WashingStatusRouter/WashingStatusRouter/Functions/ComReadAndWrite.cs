using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO.Ports;

namespace WashingStatusRouter.Functions
{
    class ComReadAndWrite
    {
        private GUI.HomeWindow window;
        private SerialPort ComPort;
        private string[] coms;
        private ReceiveMessage ReceiveMessageAction;
        public delegate void ReceiveMessage(string Message);

        public string[] ComSerialPort
        {
            get { return coms; }
        }
 
        RegistryKey registry = Registry.LocalMachine.OpenSubKey("Hardware//DeviceMap//SerialComm");
        /// <summary>
        /// 创建串口监听实例
        /// </summary>
        /// <param name="window"></param>
        public ComReadAndWrite(GUI.HomeWindow window)
        {
            coms = registry.GetValueNames();
            this.window = window;
        }
        /// <summary>
        /// 选择端口
        /// </summary>
        /// <param name="Port"></param>
        /// <param name="BaudRate"></param>
        /// <returns></returns>
        public bool DetectPort(string Port, int BaudRate)
        {
            ComPort = new SerialPort(Port);
            ComPort.BaudRate = BaudRate;
            try
            {
                ComPort.Open();
                ComPort.DataReceived += Listening;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 串口发送数据
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool SendDataToComPort(string Data)
        {
            try
            {
                ComPort.Write(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 接收消息处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Listening(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] buffer = new byte[ComPort.BytesToRead];
                ComPort.Read(buffer, 0, ComPort.BytesToRead);
                ReceiveMessageAction(Encoding.UTF8.GetString(buffer));
            }
            catch
            {
                throw new NotImplementedException();
            }
        }
    }
}
