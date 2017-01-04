using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;

namespace Wexflow.Tasks.FilesSender
{
    public class FilesSender: Task
    {
        private PluginBase _plugin;

        public FilesSender(XElement xe, Workflow wf): base(xe, wf)
        {
            string server = this.GetSetting("server");
            int port = int.Parse(this.GetSetting("port"));
            string user = this.GetSetting("user");
            string password = this.GetSetting("password");
            string path = this.GetSetting("path");
            Protocol protocol = (Protocol)Enum.Parse(typeof(Protocol), this.GetSetting("protocol"), true);
            switch (protocol)
            { 
                case Protocol.FTP:
                    _plugin = new PluginFTP(this, server, port, user, password, path);
                    break;
                case Protocol.FTPS:
                    _plugin = new PluginFTPS(this, server, port, user, password, path);
                    break;
                case Protocol.SFTP:
                    string privateKeyPath = this.GetSetting("privateKeyPath", string.Empty);
                    string passphrase = this.GetSetting("passphrase", string.Empty);
                    _plugin = new PluginSFTP(this, server, port, user, password, path, privateKeyPath, passphrase);
                    break;
            }
        }

        public override void Run()
        {
            this.Info("Sending files...");
            this._plugin.send(this.SelectFiles());
            this.Info("Task finished.");
        }
    }
}
