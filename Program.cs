using System;
using System.Windows.Forms;

namespace SysbotMacro
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            using (LoadingScreen loadingScreen = new LoadingScreen())
            {
                loadingScreen.ShowDialog();
            }
            
            Application.Run(new Form1());
        }
    }
}
