using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesSender
{
    public enum Protocol
    { 
        FTP,
        FTPS,
        SFTP
    }

    public abstract class PluginBase
    {
        public string Server { get; private set; }
        public int Port { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public string Path { get; private set; }
        public Task Task { get; private set; }

        public PluginBase(Task task, string server, int port, string user, string password, string path)
        {
            this.Task = task;
            this.Server = server;
            this.Port = port;
            this.User = user;
            this.Password = password;
            this.Path = path;
        }

        public abstract void send(FileInf[] files);
    }
}
