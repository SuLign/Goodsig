using WashingStatusRouter.GUIModule;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WashingStatusRouter.Functions
{
    class PopMessage
    {
        private MessageHall Hall;
        public PopMessage(MessageHall Hall)
        {
            this.Hall = Hall;
            MQTTEventTrigger.SetCallBackAction(new PubSubClient.ReceiveMessage(((string Topic, string Message) =>
           {
               if (Message != "" && Topic != "")
               {
                   Task.Factory.StartNew(() =>
                   {
                       Hall.Dispatcher.Invoke(() =>
                       {
                           TextBlock Content = new TextBlock();
                           MQTTEventTrigger.StatusMessage status = MQTTEventTrigger.MQTTMessageDecode(Topic, Message);
                           Content.Text = status.Message;
                           TextBlock Description = new TextBlock();
                           Description.Opacity = 0.68;
                           Description.Text = status.Description;
                           StackPanel panel = new StackPanel();
                           panel.Margin = new Thickness(24, 8, 24, 16);
                           panel.Orientation = Orientation.Vertical;
                           panel.Children.Add(Content);
                           panel.Children.Add(Description);
                           Expander expander = new Expander();
                           expander.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                           expander.Header = status.Topic;
                           expander.Content = panel;
                           Hall.MessageList.Children.Insert(0, expander);
                           Hall.Ondoing.Text = status.Topic + "...";
                       });
                   });
               }
           })));
        }
    }
}