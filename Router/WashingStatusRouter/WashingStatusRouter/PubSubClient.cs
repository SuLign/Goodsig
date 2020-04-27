using System;
using System.Text;
using HslCommunication.MQTT;
using HslCommunication;
using System.Threading;
using System.IO.Ports;

namespace WashingStatusRouter
{
    public class PubSubClientTemp
    {
        private MqttClient client = null;
        string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        MainWindow window = null;
        public PubSubClientTemp(MainWindow window)
        {
            this.window = window;
        }

        public bool Init()
        {
            client = new MqttClient(new MqttConnectionOptions()
            {
                ClientId = "Arduino",
                IpAddress = "121.36.85.182",
                Credentials = new MqttCredential("arduino", "passwd")
            });
            OperateResult connect = client.ConnectServer();
            if (connect.IsSuccess)
            {
                client.OnMqttMessageReceived += ReceiveMsg;
                client.SubscribeMessage(new string[] { "Console/#", "Arduino/#" });
                window.Dispatcher.Invoke(new Action(() =>
                {
                    window.ContentBox.Text = "Connected with MQTT Server.";
                    window.loader.Children.Remove(window.userlabel);
                    window.loader.Children.Remove(window.username);
                    window.loader.Children.Remove(window.serverid);
                    window.loader.Children.Remove(window.password);
                    window.loader.Children.Remove(window.passlabel);
                }));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SendMessage(string topic, string message)
        {
            try
            {
                client.PublishMessage(new MqttApplicationMessage()
                {
                    Topic = topic,
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                    Payload = Encoding.UTF8.GetBytes(message),
                    Retain = false
                });
                Console.WriteLine(topic + "---" + message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReceiveMsg(string topic, byte[] payload)
        {
            if (window.StartRouting)
            {
                window.Dispatcher.Invoke(new Action(() =>
                {
                    window.ContentBox.Text += topic + " : " + Encoding.UTF8.GetString(payload);
                    window.ContentBox.Text += MainWindow.PayloadMessage.PutOut(new MainWindow.PayloadMessage(topic, Encoding.UTF8.GetString(payload)));
                    if (topic.Split('/')[0] == "Console")
                    {
                        window.serial.write(Encoding.UTF8.GetString(payload));
                    }
                }));
            }
        }
        /*
        public async void ConnectWithMQTTServer()
        {
            
        }*/
    }
    public class SerialReadAndWrite
    {
        SerialPort port = new SerialPort("COM7");
        MainWindow window = null;
        public SerialReadAndWrite(MainWindow window)
        {
            port.BaudRate = 9600;
            port.Open();
            this.window = window;
            readThread = new Thread(Read);
            readThread.IsBackground = true;
            readThread.Start();
        }
        Thread readThread;
        private void Read()
        {
            while (true)
            {
                string cont = port.ReadLine();
                Console.WriteLine(cont.Trim() + cont.Trim());
                if (cont != "")
                {
                    DoWithInfo(cont);
                }
            }
        }
        public void write(string message)
        {
            port.Write(message);
        }
        public void DoWithInfo(string info)
        {
            string topic = "Arduino";
            string status = "";
            switch (info.Trim())
            {
                case "1":
                    status = "正在进水";
                    topic += "/washing";
                    break;
                case "2":
                    status = "进水完成";
                    topic += "/washing";
                    break;
                case "3":
                    status = "正在洗衣";
                    topic += "/washing";
                    break;
                case "4":
                    status = "洗衣完成";
                    topic += "/washing";
                    break;
                case "5":
                    status = "正在排水";
                    topic += "/washing";
                    break;
                case "6":
                    status = "排水完成";
                    topic += "/washing";
                    break;
                case "7":
                    status = "正在甩干";
                    topic += "/dry";
                    info = "1";
                    break;
                case "8":
                    status = "甩干完成";
                    topic += "/dry";
                    info = "2";
                    break;
                case "9":
                    status = "洗衣完成";
                    topic += "/washing";
                    break;
                case "0":
                    status = "待机中";
                    topic += "/washing";
                    break;
                case "404":
                    status = "请求错误，稍后再试";
                    topic += "/status";
                    break;
                case "200":
                    status = "请求成功";
                    topic += "/status";
                    break;
                default:return;
            }
            if (window.StartRouting)
            {
                window.Dispatcher.Invoke(new Action(() =>
                {
                    window.SendMsg(topic, Convert.ToInt32(info).ToString());
                    window.ContentBox.Text += status + "\n";
                }));
            }
        }

        public void CloseSerial()
        {
            readThread.Abort();
            port.Close();
        }
    }
}
