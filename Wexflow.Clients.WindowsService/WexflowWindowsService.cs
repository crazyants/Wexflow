using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Wexflow.Clients.WindowsService
{
    public partial class WexflowWindowsService : ServiceBase
    {
        public static string SETTINGS_FILE = ConfigurationManager.AppSettings["WexflowSettingsFile"];
        public static WexflowEngine WEXFLOW_ENGINE = new WexflowEngine(SETTINGS_FILE);

        private ServiceHost _serviceHost = null;
        
        public WexflowWindowsService()
        {
            InitializeComponent();
            this.ServiceName = "Wexflow";
            WEXFLOW_ENGINE.Run();
        }

        public void OnDebug()
        {
            this.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            if (this._serviceHost != null)
            {
                this._serviceHost.Close();
            }

            // Create a ServiceHost for the WexflowService type and 
            // provide the base address.
            this._serviceHost = new ServiceHost(typeof(WexflowService));
                
            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            this._serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (this._serviceHost != null)
            {
                this._serviceHost.Close();
                this._serviceHost = null;
            }
        }
    }
}
