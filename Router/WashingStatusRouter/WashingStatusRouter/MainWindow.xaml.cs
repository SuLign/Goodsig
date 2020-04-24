using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MQTTnet.Client.Options;
using System.Configuration;
using System.ComponentModel;

namespace WashingStatusRouter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool StartRouting = false;
        private bool startControl_1 = false, startControl_2 = false, startControl_3 = false;
        private string payload;
        public class PayloadMessage
        {
            string topic = "", message = "";
            public string Topic 
            {
                get { return topic; }
            }
            public string Message 
            {
                get { return message; }
            }
            public PayloadMessage(string payload_Topic, string payload_Message)
            {
                this.topic = payload_Topic;
                this.message = payload_Message;
            }
            public static string PutOut(PayloadMessage payload)
            {
                string _backData = Interpreter(payload);
                
                return _backData;
            }

           

            private static string Interpreter(PayloadMessage payload)
            {
                string user = "";
                string status = "";
                switch (payload.Topic)
                {
                    case "Arduino/washing":
                        user = "Washing machine";
                        break;
                    case "Arduino/Dry":
                        user = "Console";
                        break;
                    default:break;
                }
                switch(payload.Message)
                {
                    case "1":
                        status = "正在进水";
                        break;
                    case "2":
                        status = "进水完成";
                        break;
                    case "3":
                        status = "正在洗衣";
                        break;
                    case "4":
                        status = "洗衣完成";
                        break;
                    case "5":
                        status = "正在排水";
                        break;
                    case "6":
                        status = "排水完成";
                        break;
                    case "7":
                        status = "正在甩干";
                        break;
                    case "8":
                        status = "甩干完成";
                        break;
                    case "9":
                        status = "洗衣完成";
                        break;
                    case "0":
                        status = "待机中";
                        break;
                }
                return user + " : " + status;
            }
        }
        class MsgContentBind : INotifyPropertyChanged
        {
            public string contexts;
            event PropertyChangedEventHandler PropertyChanged;
            public string Contexts
            {
                get { return contexts; }
                set
                {
                    contexts = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Contexts"));
                }
            }

            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }
        }
        public void DisplayPayload (string payload)
        {
            
        }
        public PubSubClient pubSub;
        public SerialReadAndWrite serial;
        public MainWindow()
        {
            InitializeComponent();
            pubSub = new PubSubClient(this);
        }
        public void SendMsg(string topic, string message)
        {
            pubSub.SendMessage(topic, message);
        }
        static byte[] pass = Encoding.UTF8.GetBytes("passwd");
        

        private void ReceivedMessage(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e) 
        {

        }

        private void ConnectServer(object sender, RoutedEventArgs e)
        {
            CheckPort.IsEnabled = true;
            pubSub.Init();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Publish(object sender, RoutedEventArgs e)
        {
            StartRouting = true;
        }

        private void CheckCOM(object sender, RoutedEventArgs e)
        {
            serial = new SerialReadAndWrite(this);
            Publisher.IsEnabled = true;
        }
    }
}
