using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using Renci.SshNet;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesSender
{
    public class PluginSFTP:PluginBase
    {
        public string PrivateKeyPath { get; private set; }
        public string Passphrase { get; private set; }

        public PluginSFTP(Task task, string server, int port, string user, string password, string path, string privateKeyPath, string passphrase)
            :base(task, server, port, user, password, path)
        {
            this.PrivateKeyPath = privateKeyPath;
            this.Passphrase = passphrase;
        }

        public override void send(FileInf[] files)
        {
            try
            {
                // Setup Credentials and Server Information
                ConnectionInfo connInfo;

                if (!string.IsNullOrEmpty(this.PrivateKeyPath) && !string.IsNullOrEmpty(this.Passphrase))
                {
                    connInfo = new ConnectionInfo(this.Server, this.Port, this.User,
                        new AuthenticationMethod[]{

                        // Pasword based Authentication
                        new PasswordAuthenticationMethod(this.User, this.Password),

                        // Key Based Authentication (using keys in OpenSSH Format)
                        new PrivateKeyAuthenticationMethod(this.User,new PrivateKeyFile[]{ 
                            new PrivateKeyFile(this.PrivateKeyPath, this.Passphrase)
                        })
                });
                }
                else
                {
                    connInfo = new ConnectionInfo(this.Server, this.Port, this.User,
                           new AuthenticationMethod[]{

                        // Pasword based Authentication
                        new PasswordAuthenticationMethod(this.User, this.Password)
                });
                }

                using (SftpClient sftpClient = new SftpClient(connInfo))
                {
                    sftpClient.Connect();
                    sftpClient.ChangeDirectory(this.Path);

                    foreach (FileInf file in files)
                    {
                        try
                        {
                            using (FileStream fileStream = File.OpenRead(file.Path))
                            {
                                sftpClient.UploadFile(fileStream, file.FileName, true);
                            }
                            this.Task.InfoFormat("[PluginSFTP] file {0} sent to {1}.", file.Path, this.Server);
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            this.Task.ErrorFormat("[PluginSFTP] An error occured while sending the file {0} to {1}. Error message: {2}", file.Path, this.Server, e.Message);
                        }
                    }

                    sftpClient.Disconnect();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.Task.ErrorFormat("[PluginSFTP] An error occured while sending files.", e);
            }
        }

    }
}
