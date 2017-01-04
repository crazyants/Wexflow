using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using FluentFTP;
using System.Net;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesSender
{
    public class PluginFTP : PluginBase
    {
        static ManualResetEvent m_reset = new ManualResetEvent(false);

        public PluginFTP(Task task, string server, int port, string user, string password, string path)
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

                client.Connect();
                client.SetWorkingDirectory(this.Path);

                foreach (FileInf file in files)
                {
                    try
                    {
                        client.BeginOpenWrite(file.FileName, FtpDataType.Binary,
                            new AsyncCallback(BeginOpenWriteCallback), new AsyncCallbackState(client, file));

                        m_reset.WaitOne();
                        this.Task.InfoFormat("[PluginFTP] file {0} sent to {1}.", file.Path, this.Server);
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        this.Task.ErrorFormat("[PluginFTP] An error occured while sending the file {0} to {1}. Error message: {2}", file.Path, this.Server, e.Message);
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
                this.Task.ErrorFormat("[PluginFTP] An error occured while sending files.", e);
            }
        }

        public static void BeginOpenWriteCallback(IAsyncResult ar)
        {
            AsyncCallbackState state = ar.AsyncState as AsyncCallbackState;
            FtpClient client = state.FtpCLient;
            FileInf file = state.File;
            Stream istream = null, ostream = null;
            byte[] buf = new byte[8192];
            int read = 0;

            try
            {
                if (client == null) throw new InvalidOperationException("The FtpControlConnection object is null!");

                ostream = client.EndOpenWrite(ar);
                istream = new FileStream(file.Path, FileMode.Open, FileAccess.Read);

                while ((read = istream.Read(buf, 0, buf.Length)) > 0)
                {
                    ostream.Write(buf, 0, read);
                }
            }
            finally
            {
                if (istream != null) istream.Close();

                if (ostream != null) ostream.Close();

                m_reset.Set();
            }
        }
    }
}
