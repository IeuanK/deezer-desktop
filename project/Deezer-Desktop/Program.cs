using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deezer_Desktop
{
    static class Program
    {
        public static Mutex mutex;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            var mutex = new Mutex(true, Application.ProductName, out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Another instance of Deezer Desktop is already open", "Warning");
                Environment.Exit(1);
            }
            else
            {
                Program.mutex = mutex;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
