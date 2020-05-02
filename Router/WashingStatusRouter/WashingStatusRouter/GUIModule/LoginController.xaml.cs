using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WashingStatusRouter.Functions;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WashingStatusRouter.GUIModule
{
    /// <summary>
    /// LoginController.xaml 的交互逻辑
    /// </summary>
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
