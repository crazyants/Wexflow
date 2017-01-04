using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Configuration;

namespace Wexflow.Clients.Cmd
{
    class Program
    {
        // TODO WCF start, stop, pause ...
        // TODO FilesRenamer?, YouTube? ...

        public static string SETTINGS_FILE = 
            ConfigurationManager.AppSettings["WexflowSettingsFile"];

        static void Main(string[] args)
        {
            WexflowEngine wexflowEngine = new WexflowEngine(SETTINGS_FILE);
            wexflowEngine.Run();

            Console.ReadKey();
        }
    }
}