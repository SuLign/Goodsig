using System.Windows.Controls;
using WashingStatusRouter.Functions;

namespace WashingStatusRouter.GUIModule
{
    public partial class MessageHall : UserControl
    {
        PopMessage Pop;
        public MessageHall()
        {
            InitializeComponent();
            Pop = new PopMessage(this);
            MQTTEventTrigger.SetHall(this);
            ServerIP.Text = AMiddle.Middle.ServerIPAddress;
            ServerPort.Text = AMiddle.Middle.Port.ToString();
        }
    }
}
