using System;
using System.Text;
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
        public delegate void AsDisposed();
        public void AsDisposedHandle(AsDisposed asDisposed)
        {
            disposed += asDisposed;
        }
        public string[] ComSerialPort
        {
            get { return coms; }
        }
        public ComReadAndWrite()
        {
            Connected = false;
            coms = SerialPort.GetPortNames();
        }
        public bool DetectPort(string Port, int BaudRate)
        {
            ComPort = new SerialPort(Port);
            ComPort.BaudRate = BaudRate;
            try
            {
                ComPort.Open();
                ComPort.DataReceived += Listening;
                ComPort.ErrorReceived += Disposed;
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
        private void Listening(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] buffer = new byte[ComPort.BytesToRead];
                Console.WriteLine("Bytes length:" + ComPort.BytesToRead.ToString());
                ComPort.Read(buffer, 0, ComPort.BytesToRead);
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
        private AsDisposed disposed;
        private void Disposed(object sender, EventArgs e)
        {
            disposed();
        }
        public void DisposeCom()
        {
            ComPort.Close();
            ComPort.Dispose();
        }
    }
}
