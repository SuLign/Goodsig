using System.Windows;
using System.Windows.Input;

namespace WashingStatusRouter
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            Functions.AMiddle.InitLoginWindow(this);
            if (!App.ReadStatus)
            {
                SnackbarThree.MessageQueue.Enqueue("加载用户配置文件错误");
            }
        }
        public void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FormFunction.MoveForm(sender, e, this);
        }
    }
}
