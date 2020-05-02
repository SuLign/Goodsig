using System;

namespace WashingStatusRouter.Functions
{
    class MQTTEventTrigger
    {
        public static WashingStatusRouter.GUI.HomeWindow home;
        public static WashingStatusRouter.GUIModule.MessageHall Hall;
        public static PubSubClient Client;
        public static PubSubClient.ReceiveMessage CallBack;
        private static ComReadAndWrite ReadAndWrite;
        public static void Transfer(WashingStatusRouter.GUI.HomeWindow home, PubSubClient client)
        {
            MQTTEventTrigger.home = home;
            Client = client;
            Client.GetReceive = CallBack;
        }
        public static void DisposeCom()
        {
            try
            {
                ReadAndWrite.DisposeCom();
            }
            catch { }
        }
        public static void DisposeMQTT()
        {
            try
            {
                Client.Dispose();
            }
            catch { }
        }
        public static void LoadSerialPort(ComReadAndWrite com)
        {
            ReadAndWrite = com;
        }
        public static void SetHall(WashingStatusRouter.GUIModule.MessageHall hall)
        {
            Hall = hall;
        }
        public static void SetCallBackAction(PubSubClient.ReceiveMessage action) 
        {
            CallBack = action;
        }
        public class StatusMessage
        {
            private string topic, message, description;
            public string Topic { get { return topic; } }
            public string Message { get { return message; } }
            public string Description { get { return description; } }
            public void WriteInTopic(string topic)
            {
                this.topic = topic;
            }
            public void WriteInMessage(string message)
            {
                this.message = message;
            }
            public void WriteInDescription(string description)
            {
                this.description = description;
            }
        }
        public static StatusMessage MQTTMessageDecode(string Topic, string Message)
        {
            StatusMessage status = new StatusMessage();
            status.WriteInMessage(DateTime.Now.ToString());
            switch (Topic)
            {
                case "Arduino/status":
                    switch (Message)
                    {
                        case "0":
                            status.WriteInTopic("状态信息:" + "待机中");
                            status.WriteInDescription("洗衣机当前处于空闲状态，可以发送洗衣指令。");
                            break;
                        case "200":
                            status.WriteInTopic("状态信息:" + "请求成功");
                            status.WriteInDescription("指令发送成功，正在执行");
                            break;
                        case "404":
                            status.WriteInTopic("状态信息:" + "请求失败");
                            status.WriteInDescription("洗衣机当前运行状态不支持您所发送的请求，请稍后再试");
                            break;
                    }
                    break;
                case "Arduino/washing":
                    switch (Message)
                    {
                        case "1":
                            status.WriteInTopic("洗衣中:" + "正在注水");
                            status.WriteInDescription("正在向洗衣机中注水，等待注水完成，自动开始洗衣");
                            break;
                        case "2":
                            status.WriteInTopic("洗衣中:" + "进水完成");
                            status.WriteInDescription("注水完成，即将开始洗衣");
                            break;
                        case "3":
                            status.WriteInTopic("洗衣中:" + "正在洗衣");
                            status.WriteInDescription("洗衣中");
                            break;
                        case "4":
                            status.WriteInTopic("洗衣中:" + "洗衣完成");
                            status.WriteInDescription("洗衣完成，准备排水");
                            break;
                        case "5":
                            status.WriteInTopic("洗衣中:" + "正在排水");
                            status.WriteInDescription("正在排水中，排水完成，自动开始甩干");
                            break;
                        case "6":
                            status.WriteInTopic("洗衣中:" + "排水完成");
                            status.WriteInDescription("排水完成，即将开始甩干");
                            break;
                        default:
                            status.WriteInTopic("ERROR");
                            status.WriteInDescription("未知指令");
                            break;
                    }
                    break;
                case "Arduino/dry":
                    switch (Message)
                    {
                        case "1":
                            status.WriteInTopic("甩干中:" + "正在甩干");
                            status.WriteInDescription("正在甩干中");
                            break;
                        case "2":
                            status.WriteInTopic("甩干中:" + "甩干完成");
                            status.WriteInDescription("甩干完成");
                            break;
                        default:
                            status.WriteInTopic("ERROR");
                            status.WriteInDescription("未知指令");
                            break;
                    }
                    break;
                case "Console":
                    if(ReadAndWrite != null) ReadAndWrite.SendDataToComPort(Message);
                    switch (Message)
                    {
                        case "111":
                            status.WriteInTopic("远端指令:" + "普通洗衣");
                            status.WriteInDescription("收到远程指令：执行 普通洗衣");
                            break;
                        case "112":
                            status.WriteInTopic("远端指令:" + "快速洗衣");
                            status.WriteInDescription("收到远程指令：执行 快速洗衣");
                            break;
                        case "113":
                            status.WriteInTopic("远端指令:" + "慢速洗衣");
                            status.WriteInDescription("收到远程指令：执行 慢速洗衣");
                            break;
                        case "121":
                            status.WriteInTopic("远端指令:" + "甩干2分钟");
                            status.WriteInDescription("收到远程指令：执行 甩干2分钟");
                            break;
                        case "122":
                            status.WriteInTopic("远端指令:" + "甩干5分钟");
                            status.WriteInDescription("收到远程指令：执行 甩干5分钟");
                            break;
                        case "123":
                            status.WriteInTopic("远端指令:" + "甩干7分钟");
                            status.WriteInDescription("收到远程指令：执行 甩干7分钟");
                            break;
                        case "1":
                            status.WriteInTopic("远端指令:" + "暂停");
                            status.WriteInDescription("收到远程指令：暂停当前作业");
                            break;
                        case "2":
                            status.WriteInTopic("远端指令:" + "停止");
                            status.WriteInDescription("收到远程指令：停止当前作业");
                            break;
                        case "3":
                            status.WriteInTopic("远端指令:" + "恢复");
                            status.WriteInDescription("收到远程指令：恢复当前作业");
                            break;
                        case "9":
                            status.WriteInTopic("远端指令:" + "确定");
                            status.WriteInDescription("收到远程指令：确定");
                            break;
                        default:
                            status.WriteInTopic("ERROR");
                            status.WriteInDescription("未知指令");
                            break;
                    }
                    break;
                default:
                    status.WriteInTopic("ERROR");
                    status.WriteInDescription("未知指令");
                    break;
            }
            return status;
        }
    }
}
