using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration;

namespace Wexflow.Clients.Manager
{
    public static class Program
    {
        public static string WEXFLOW_SERVICE_NAME = ConfigurationManager.AppSettings["WEXFLOW_SERVICE_NAME"];
        public static bool DEBUG_MODE = false;

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("debug"))
            {
                DEBUG_MODE = true;
                RunForm1();
            }
            else
            {
                if (IsWexflowWindowsServiceRunning())
                {
                    RunForm1();
                }
                else
                {
                    MessageBox.Show("Wexflow Windows Service is not running. Please run it to start Wexflow Manager.");
                }
            }
        }

        static void RunForm1()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static bool IsWexflowWindowsServiceRunning()
        {
            ServiceController sc = new ServiceController(WEXFLOW_SERVICE_NAME);
            return sc.Status == ServiceControllerStatus.Running;
        }
    }
}
