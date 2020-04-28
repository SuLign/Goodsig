using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using MaterialDesignThemes;
using MaterialDesignColors;
using System.Windows.Input;
using WashingStatusRouter.Functions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WashingStatusRouter.GUI;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WashingStatusRouter.GUIModule
{
    /// <summary>
    /// ComSerial.xaml 的交互逻辑
    /// </summary>
    public partial class ComSerial : UserControl
    {
        ComReadAndWrite coms;
        public ComSerial()
        {
            InitializeComponent();
        }

        private void ComSelector_GotMouseCapture(object sender, MouseEventArgs e)
        {
            coms = new ComReadAndWrite();
            var ComPorts = ComSelector;
            ComPorts.ItemsSource = coms.ComSerialPort;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(coms != null && ComSelector.SelectedItem != null) {
                string Port = ComSelector.SelectedItem.ToString();
                if (coms.DetectPort(Port, 9600)) 
                {
                    var MessageQueue = AMiddle.home.SnackbarThree.MessageQueue;
                    Task.Factory.StartNew(() => MessageQueue.Enqueue("成功连接至" + Port));
                    AMiddle.home.HomePageTransitioner.SelectedIndex = 1;
                }
                else
                {
                    var MessageQueue = AMiddle.home.SnackbarThree.MessageQueue;
                    Task.Factory.StartNew(() => MessageQueue.Enqueue("连接" + Port + "失败"));
                }
            }
        }
    }
}
