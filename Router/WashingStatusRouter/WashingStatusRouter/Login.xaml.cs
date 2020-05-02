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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WashingStatusRouter.Functions;
using System.Windows.Shapes;

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
