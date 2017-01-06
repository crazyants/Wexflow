using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace Wexflow.Tasks.Tgz
{
    public class Tgz:Task
    {
        public string TgzFileName { get; private set; }

        public Tgz(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.TgzFileName = this.GetSetting("tgzFileName");
        }

        public override void Run()
        {
            this.Info("Creating tgz archive...");

            FileInf[] files = this.SelectFiles();
            if (files.Length > 0)
            {
                string tgzPath = Path.Combine(this.Workflow.WorkflowTempFolder, this.TgzFileName);

                try
                {
                    using (GZipOutputStream gz = new GZipOutputStream(File.Create(tgzPath)))
                    using (TarOutputStream tar = new TarOutputStream(gz))
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
                        tar.Close();

                        // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                        // the created file would be invalid.
                        gz.Finish();

                        // Close is important to wrap things up and unlock the file.
                        gz.Close();

                        this.InfoFormat("Tgz {0} created.", tgzPath);
                        this.Files.Add(new FileInf(tgzPath, this.Id));
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the Tar {0}", e, tgzPath);
                }
            }

            this.Info("Task finished.");
        }
    }
}
