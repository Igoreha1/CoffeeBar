using CoffeeBar.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CoffeeBar
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            mainWindow.MainFrame.Navigate(new LoginPage());

            mainWindow.Show();
        }
    }
}
