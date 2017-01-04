using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Threading;
using FluentFTP;
using System.Net;

namespace Wexflow.Tasks.FilesSender
{
    public class PluginFTPS: PluginBase
    {
        static ManualResetEvent m_reset = new ManualResetEvent(false);

        public PluginFTPS(Task task, string server, int port, string user, string password, string path)
            :base(task, server, port, user, password, path)
        {
        }

        public override void send(FileInf[] files)
        {
            try
            {
                FtpClient client = new FtpClient();
                client.Host = this.Server;
                client.Port = this.Port;
                client.Credentials = new NetworkCredential(this.User, this.Password);
                client.EncryptionMode = FtpEncryptionMode.Explicit; // FTPS

                client.Connect();
                client.SetWorkingDirectory(this.Path);

                foreach (FileInf file in files)
                {
                    try
                    {
                        client.BeginOpenWrite(file.FileName, FtpDataType.Binary,
                            new AsyncCallback(PluginFTP.BeginOpenWriteCallback), new AsyncCallbackState(client, file));

                        m_reset.WaitOne();
                        this.Task.InfoFormat("[PluginFTPS] file {0} sent to {1}.", file.Path, this.Server);
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        this.Task.ErrorFormat("[PluginFTPS] An error occured while sending the file {0} to {1}. Error message: {2}", file.Path, this.Server, e.Message);
                    }
                }

                client.Disconnect();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.Task.ErrorFormat("[PluginFTPS] An error occured while sending files.", e);
            }
        }
    }
}
