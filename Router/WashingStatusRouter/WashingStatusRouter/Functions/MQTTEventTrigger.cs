using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WashingStatusRouter.Functions
{
    class MQTTEventTrigger
    {
        public static WashingStatusRouter.GUI.HomeWindow home;
        public static PubSubClient Client;
        public static void Transfer(WashingStatusRouter.GUI.HomeWindow home, PubSubClient client)
        {
            MQTTEventTrigger.home = home;
            Client = client;
            Client.GetReceive = new PubSubClient.ReceiveMessage(Action);
        }
        public static void Action(string Topic, string Message) 
        {
            
        }


    }
}
