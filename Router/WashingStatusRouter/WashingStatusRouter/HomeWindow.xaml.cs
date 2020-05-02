using System.Windows;
using System.Windows.Input;

namespace WashingStatusRouter.GUI
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }
        public void Exit(object sender, RoutedEventArgs e)
        {
            FormFunction.Exit();
        }
        public void Setting(object sender, RoutedEventArgs e)
        {
            HomePageTransitioner.SelectedIndex = HomePageTransitioner.SelectedIndex == 1?0:1;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FormFunction.MoveForm(sender, e, this);
        }
    }
}
