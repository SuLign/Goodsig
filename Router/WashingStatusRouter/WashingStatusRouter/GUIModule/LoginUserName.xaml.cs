using System.Windows;
using System.Windows.Controls;
using WashingStatusRouter.Functions;

namespace WashingStatusRouter.GUIModule
{
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
