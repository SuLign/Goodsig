using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using WashingStatusRouter.Functions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WashingStatusRouter.GUIModule
{
    /// <summary>
    /// LoginUserName.xaml 的交互逻辑
    /// </summary>
    public partial class LoginUserName : UserControl
    {
        public LoginUserName()
        {
            InitializeComponent();
            if (Configurations.UserIsSaved)
            {
                UserName.Text = Configurations.Username;
                PasswordBox.Password = Configurations.Password;
                RemmenberPassword.IsChecked = true;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Functions.AMiddle.Middle.UserName = UserName.Text;
            Functions.AMiddle.Middle.Password = PasswordBox.Password;
            Functions.AMiddle.StartConnection();
            if ((bool)RemmenberPassword.IsChecked)
            {
                Configurations.SaveUserInfo(UserName.Text, PasswordBox.Password);
                Configurations.SaveLoadInfo();
            }
        }
    }
}
