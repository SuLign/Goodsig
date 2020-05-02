using System;
using System.Windows;
using System.Windows.Controls;
using WashingStatusRouter.Functions;

namespace WashingStatusRouter.GUIModule
{
    public partial class LoginController : UserControl
    {
        public LoginController()
        {
            InitializeComponent();
            if (Configurations.ServerIsSaved)
            {
                ServerIP.Text = Configurations.ServerIP;
                ServerPort.Text = Configurations.ServerPort;
                RemmenberServer.IsChecked = true;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Functions.AMiddle.Middle.ServerIPAddress = ServerIP.Text == ""?"":ServerIP.Text;
            Functions.AMiddle.Middle.Port = Convert.ToInt32(ServerPort.Text==""?"0": ServerPort.Text);
            if ((bool)RemmenberServer.IsChecked)
            {
                Configurations.SaveServerInfo(ServerIP.Text, ServerPort.Text);
            }
        }
    }
}
