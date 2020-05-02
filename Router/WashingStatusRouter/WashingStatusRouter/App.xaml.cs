using System.Windows;

namespace WashingStatusRouter
{
    public partial class App : Application
    {
        public static bool ReadStatus;
        public App()
        {
            if (Functions.Configurations.LoadLoginInfo()) ReadStatus = true; ;
        }
    }
}
