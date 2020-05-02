using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace WashingStatusRouter.Functions
{
    class ComReadAndWrite
    {
        private SerialPort ComPort;
        bool Connected = false;
        private string[] coms;
        public delegate void ReceiveMessage(string Message);
        public void ReceiveActionSet(ReceiveMessage receive)
        {
            ReceiveMessageAction = receive;
        }
        public string[] ComSerialPort
        {
            get { return coms; }
        }

        /// <summary>
        /// 创建串口监听实例
        /// </summary>
        /// <param name="window"></param>
        public ComReadAndWrite()
        {
            Connected = false;
            coms = SerialPort.GetPortNames();
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
                if (!Connected)
                {
                    ComPort.Write("100");
                }
                Thread.Sleep(1000);
                if (!Connected) ComPort.Close();
                return Connected;
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
                Console.WriteLine("Bytes length:" + ComPort.BytesToRead.ToString());
                ComPort.Read(buffer, 0, ComPort.BytesToRead);
                Console.WriteLine(Encoding.UTF8.GetString(buffer).Replace("\n", "").Replace("\r", ""));
                Console.WriteLine(Encoding.UTF8.GetString(buffer).Replace("\n", "").Replace("\r", ""));
                if (!Connected)
                {
                    if (Encoding.UTF8.GetString(buffer).Replace("\n", "").Replace("\r", "") == "Y")
                    {
                        Connected = true;
                    }
                }
                else if (Encoding.UTF8.GetString(buffer).Replace("\n", "").Replace("\r", "") != "")
                {
                    ReceiveMessageAction(Encoding.UTF8.GetString(buffer).Replace("\n", "").Replace("\r", ""));
                }
                Thread.Sleep(100);
            }
            catch
            {
                throw new Exception();
            }
        }
        private ReceiveMessage ReceiveMessageAction = new ReceiveMessage((string Message) => {
            MessageDetecter detecter = new MessageDetecter(Message);
            MQTTEventTrigger.Client.SendMessage(detecter.TopicToMQTTServer, detecter.MessageToMQTTServer);
        });
    }
}
