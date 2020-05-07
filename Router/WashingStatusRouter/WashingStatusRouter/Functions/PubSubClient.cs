using System;
using System.Text;
using HslCommunication.MQTT;

namespace WashingStatusRouter.Functions
{
    class PubSubClient
    {
        private MqttClient client = null;
        public delegate void ReceiveMessage(string Topic, string Message);
        public ReceiveMessage GetReceive;
        public PubSubClient(){}
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
        public void SubScribe(string[] Topics)
        {
            client.SubscribeMessage(Topics);
        }
        private void ReceiveMsg(string topic, byte[] payload)
        {

            GetReceive(topic, Encoding.UTF8.GetString(payload));
        }
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
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Dispose()
        {
            client.ConnectClose();
        }
    }
}
