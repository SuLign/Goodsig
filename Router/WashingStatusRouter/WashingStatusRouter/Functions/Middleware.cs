using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WashingStatusRouter.Functions
{
    class MiddleWare
    {
        public string UserName;
        public string Password;
        public string ServerIPAddress;
        public int Port;
        public PubSubClient client;
        public MiddleWare(){}
        public MiddleWare(string UserName, string Password, string IPAddress, int Port)
        {
            this.UserName = UserName;
            this.Password = Password;
            this.ServerIPAddress = IPAddress;
            this.Port = Port;
        }
        public bool ConnectWithServer()
        {
            client = new PubSubClient();
            return (client.Connect("MiddleWare", ServerIPAddress, UserName, Password));
        }
    }
    class AMiddle
    {
        private static Login loginForm;
        public static void InitLoginWindow(Login login)
        {
            loginForm = login;
        }
        public static MiddleWare Middle = new MiddleWare();
        public static WashingStatusRouter.GUI.HomeWindow home;
        public static void StartConnection()
        {
            loginForm.ProgressLine.Visibility = System.Windows.Visibility.Visible;
            loginForm.ProgressLine.IsIndeterminate = true;
            if (Middle.ConnectWithServer())
            {
                home = new GUI.HomeWindow();
                MQTTEventTrigger.Transfer(home, Middle.client);
                home.Show();
                home.Activate(); 
                loginForm.Close();
            }
            else
            {
                var MessageQueue = loginForm.SnackbarThree.MessageQueue;
                Task.Factory.StartNew(() => MessageQueue.Enqueue("未能连接到服务器,请检查服务器地址和端口以及用户名密码是否正确"));
                loginForm.ProgressLine.IsIndeterminate = false;
            }
        }
    }
}
