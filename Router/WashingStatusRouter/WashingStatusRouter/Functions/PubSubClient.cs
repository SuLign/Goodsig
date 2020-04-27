using System;
using System.Text;
using HslCommunication.MQTT;
using HslCommunication;
using System.Threading;
using System.IO.Ports;

namespace WashingStatusRouter.Functions
{
    class PubSubClient
    {
        private MqttClient client = null;
        private GUI.HomeWindow window = null;
        private ReceiveMessage ReceiveMessageAction;
        public delegate void ReceiveMessage(string Topic, string Message);
        public ReceiveMessage GetReceive
        {
            set { ReceiveMessageAction = value; }
        }
        /// <summary>
        /// 创建一个通讯服务并绑定当前窗口
        /// </summary>
        /// <param name="window">目标窗口</param>
        public PubSubClient(GUI.HomeWindow window)
        {
            this.window = window;
        }

        /// <summary>
        /// 连接MQTT服务器
        /// </summary>
        /// <param name="ClientID">客户ID</param>
        /// <param name="MqttServerIPAddress">服务器地址</param>
        /// <param name="Credential_Username">连接凭据用户名</param>
        /// <param name="Credential_Password">连接凭据密码</param>
        /// <returns></returns>
        public bool Connect(string ClientID, string MqttServerIPAddress,string Credential_Username, string Credential_Password)
        {
            client = new MqttClient(new MqttConnectionOptions()
            {
                ClientId = ClientID,
                IpAddress = MqttServerIPAddress,
                Credentials = new MqttCredential(Credential_Username, Credential_Password)
            });

            try
            {
                if (client.ConnectServer().IsSuccess) //连接成功
                {
                    client.OnMqttMessageReceived += ReceiveMsg;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 订阅（单条）主题
        /// </summary>
        /// <param name="Topic"></param>
        public void SubScribe(string Topic)
        {
            client.SubscribeMessage(Topic);
        }

        /// <summary>
        /// 订阅（多条）主题
        /// </summary>
        /// <param name="Topics"></param>
        public void SubScribe(string[] Topics)
        {
            client.SubscribeMessage(Topics);
        }

        private void ReceiveMsg(string topic, byte[] payload)
        {
            ReceiveMessageAction(topic, Encoding.UTF8.GetString(payload));
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Topic"></param>
        /// <param name="Payload"></param>
        /// <returns></returns>
        public bool SendMessage(string Topic, string Payload)
        {
            try
            {
                client.PublishMessage(new MqttApplicationMessage()
                {
                    Topic = Topic,
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                    Payload = Encoding.UTF8.GetBytes(Payload),
                    Retain = false
                });
                Console.WriteLine("Send Success");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
