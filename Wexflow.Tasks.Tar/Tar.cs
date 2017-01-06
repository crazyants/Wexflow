using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;

namespace Wexflow.Tasks.Tar
{
    public class Tar:Task
    {
        public string TarFileName { get; private set; }

        public Tar(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.TarFileName = this.GetSetting("tarFileName");
        }

        public override void Run()
        {
            this.Info("Creating tar archive...");

            FileInf[] files = this.SelectFiles();
            if (files.Length > 0)
            {
                string tarPath = Path.Combine(this.Workflow.WorkflowTempFolder, this.TarFileName);

                try
                {
                    using (TarOutputStream tar = new TarOutputStream(File.Create(tarPath)))
                    {
                        byte[] buffer = new byte[4096];

                        foreach (FileInf file in files)
                        {
                            using (Stream inputStream = File.OpenRead(file.Path))
                            {
                                string tarName = file.FileName;

                                long fileSize = inputStream.Length;

                                // Create a tar entry named as appropriate. You can set the name to anything,
                                // but avoid names starting with drive or UNC.
                                TarEntry entry = TarEntry.CreateTarEntry(tarName);

                                // Must set size, otherwise TarOutputStream will fail when output exceeds.
                                entry.Size = fileSize;

                                // Add the entry to the tar stream, before writing the data.
                                tar.PutNextEntry(entry);

                                byte[] localBuffer = new byte[32 * 1024];
                                while (true)
                                {
                                    int numRead = inputStream.Read(localBuffer, 0, localBuffer.Length);
                                    if (numRead <= 0)
                                    {
                                        break;
                                    }
                                    tar.Write(localBuffer, 0, numRead);
                                }
                            }

                            tar.CloseEntry();
                        }

                        // Finish/Close arent needed strictly as the using statement does this automatically

                        // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                        // the created file would be invalid.
                        tar.Finish();

                        // Close is important to wrap things up and unlock the file.
                        tar.Close();

                        this.InfoFormat("Tar {0} created.", tarPath);
                        this.Files.Add(new FileInf(tarPath, this.Id));
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the Tar {0}", e, tarPath);
                }
            }

            this.Info("Task finished.");
        }
    }
}
