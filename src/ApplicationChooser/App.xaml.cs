using System.Collections.Generic;
using System.Windows;

namespace ApplicationChooser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<string> Args = new List<string>();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Args.AddRange(e.Args);
        }
    }
}
