using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using WashingStatusRouter;
using System.Threading.Tasks;
using System.Windows;

namespace WashingStatusRouter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static bool ReadStatus;
        public App()
        {
            if (Functions.Configurations.LoadLoginInfo()) ReadStatus = true; ;
        }
    }
}
