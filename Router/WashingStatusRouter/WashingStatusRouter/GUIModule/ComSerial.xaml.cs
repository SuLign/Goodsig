using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WashingStatusRouter.Functions;
using System.Windows.Media;

namespace WashingStatusRouter.GUIModule
{
    public partial class ComSerial : UserControl
    {
        ComReadAndWrite coms;
        public ComSerial()
        {
            coms = new ComReadAndWrite();
            InitializeComponent();
            coms.AsDisposedHandle(new ComReadAndWrite.AsDisposed(AsComDisposed));
        }
        public void AsComDisposed()
        {
            Task.Factory.StartNew(() =>
            {
                AMiddle.home.SnackbarThree.MessageQueue.Enqueue("串口读取错误");
            });
        }
        private void ComSelector_GotMouseCapture(object sender, MouseEventArgs e)
        {
            try
            {
                coms = new ComReadAndWrite();
                var ComPorts = ComSelector;
                ComPorts.ItemsSource = coms.ComSerialPort;
                ConnectWithSerialPort.IsEnabled = true;
                MQTTEventTrigger.LoadSerialPort(coms);
            }
            catch
            {
                Task.Factory.StartNew(() => AMiddle.home.SnackbarThree.MessageQueue.Enqueue("读取串口信息失败"));
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (coms != null && ComSelector.SelectedItem != null)
            {
                string Port = ComSelector.SelectedItem.ToString();
                Task.Factory.StartNew(() =>
                {
                    if (coms.DetectPort(Port, 9600))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            var MessageQueue = AMiddle.home.SnackbarThree.MessageQueue;
                            Task.Factory.StartNew(() => MessageQueue.Enqueue("成功连接至" + Port));
                            AMiddle.home.HomePageTransitioner.SelectedIndex = 1;
                            MQTTEventTrigger.Hall.Ports.Text = Port;
                            MQTTEventTrigger.Hall.NowStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 100, 0));
                            MQTTEventTrigger.Hall.NowStatus.Text = "在线";
                        });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            var MessageQueue = AMiddle.home.SnackbarThree.MessageQueue;
                            Task.Factory.StartNew(() => MessageQueue.Enqueue("连接" + Port + "失败"));
                        });
                    }
                });
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AMiddle.home.HomePageTransitioner.SelectedIndex = 1;
        }
    }
}
