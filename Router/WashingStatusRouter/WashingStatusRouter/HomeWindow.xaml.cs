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
using WashingStatusRouter.Functions;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WashingStatusRouter.GUI
{
    /// <summary>
    /// HomeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
            //var MessageQueue = SnackbarThree.MessageQueue;
            //Task.Factory.StartNew(() => MessageQueue.Enqueue("欢迎您！"));
        }
        public void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

}
