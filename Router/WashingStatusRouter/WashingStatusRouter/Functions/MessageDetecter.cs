using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WashingStatusRouter.Functions
{
    class MessageDetecter
    {
        private string id;
        private string topic;
        private string result;
        public string ResultToDisplayBack
        {
            get { return result; }
        }
        public string MessageToMQTTServer
        {
            get { return id; }
        }
        public string TopicToMQTTServer
        {
            get { return "Arduino/" + topic; }
        }
        /// <summary>
        /// 解码串口信号
        /// </summary>
        /// <param name="Param"></param>
        /// <returns></returns>
        public MessageDetecter(string Param)
        {
            id = Param;
            switch (Param)
            {
                case "1":
                    result = "正在进水";
                    topic = "/washing";
                    break;
                case "2":
                    result = "进水完成";
                    topic = "/washing";
                    break;
                case "3":
                    result = "正在洗衣";
                    topic = "/washing";
                    break;
                case "4":
                    result = "洗衣完成";
                    topic = "/washing";
                    break;
                case "5":
                    result = "正在排水";
                    topic = "/washing";
                    break;
                case "6":
                    result = "排水完成";
                    topic = "/washing";
                    break;
                case "7":
                    result = "正在甩干";
                    topic = "/dry";
                    id = "1";
                    break;
                case "8":
                    result = "甩干完成";
                    topic = "/dry";
                    id = "2";
                    break;
                case "9":
                    result = "洗衣完成";
                    topic = "/status";
                    break;
                case "0":
                    result = "待机中";
                    topic = "/status";
                    break;
                case "404":
                    result = "请求错误，稍后再试";
                    topic = "/status";
                    break;
                case "200":
                    result = "请求成功";
                    topic = "/status";
                    break;
                default: 
                    result = "未知指令";
                    break;
            }
            id = Convert.ToInt32(id).ToString();
        }
    }
}
