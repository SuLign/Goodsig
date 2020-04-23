using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Client.Options;
using HslCommunication.MQTT;
using HslCommunication;

namespace WashingStatusRouter
{
    public class PubSubClient
    {
        private MqttClient client = null;
        string message;
        public string Message
        {
            get { return message;}
            set { message = value;}
        }

        MainWindow window = null;
        public PubSubClient(MainWindow window)
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
            }) ;
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
                return true;
            }catch
            {
                return false;
            }
        }

        private void ReceiveMsg(string topic, byte[] payload)
        {
            window.Dispatcher.Invoke(new Action(() =>
            {
                window.ContentBox.Text += topic + " : " + Encoding.UTF8.GetString(payload);
                window.ContentBox.Text += MainWindow.PayloadMessage.PutOut(new MainWindow.PayloadMessage(topic, Encoding.UTF8.GetString(payload)));
            }));
        }
        /*
        public async void ConnectWithMQTTServer()
        {
            
        }*/
    }
}
