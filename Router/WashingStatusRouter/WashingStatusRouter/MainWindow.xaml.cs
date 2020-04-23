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
                        status = "Input water";
                        break;
                    case "2":
                        status = "jinshuiwancheng";
                        break;
                    case "3":
                        status = "zhengzaixiyi";
                        break;
                    case "4":
                        status = "xiyiwancheng";
                        break;
                    case "5":
                        status = "zhengzai paishui";
                        break;
                    case "6":
                        status = "paishui wancheng";
                        break;
                    case "7":
                        status = "zhengzai shuaigan ";
                        break;
                    case "8":
                        status = "tuo shui wan cheng";
                        break;
                    case "9":
                        status = "Done!";
                        break;
                    case "0":
                        status = "Waiting...";
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
        PubSubClient pubSub;
        public MainWindow()
        {
            InitializeComponent();
            pubSub = new PubSubClient(this);
        }
        static byte[] pass = Encoding.UTF8.GetBytes("passwd");
        

        private void ReceivedMessage(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e) 
        {

        }

        private void ConnectServer(object sender, RoutedEventArgs e)
        {
            pubSub.Init();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Publish(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CheckCOM(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
