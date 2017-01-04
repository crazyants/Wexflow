using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Zip
{
    public class Zip:Task
    {
        public string ZipFileName { get; private set; }

        public Zip(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.ZipFileName = this.GetSetting("zipFileName");
        }

        public override void Run()
        {
            this.Info("Zipping files...");

            FileInf[] files = this.SelectFiles();
            if(files.Length > 0)
            {
                string zipPath = Path.Combine(this.Workflow.WorkflowTempFolder, this.ZipFileName);

                try
                {
                    using (ZipOutputStream s = new ZipOutputStream(File.Create(zipPath)))
                    {
                        s.SetLevel(9); // 0 - store only to 9 - means best compression

                        byte[] buffer = new byte[4096];

                        foreach (FileInf file in files)
                        {
                            // Using GetFileName makes the result compatible with XP
                            // as the resulting path is not absolute.
                            ZipEntry entry = new ZipEntry(Path.GetFileName(file.Path));

                            // Setup the entry data as required.

                            // Crc and size are handled by the library for seakable streams
                            // so no need to do them here.

                            // Could also use the last write time or similar for the file.
                            entry.DateTime = DateTime.Now;
                            s.PutNextEntry(entry);

                            using (FileStream fs = File.OpenRead(file.Path))
                            {

                                // Using a fixed size buffer here makes no noticeable difference for output
                                // but keeps a lid on memory usage.
                                int sourceBytes;
                                do
                                {
                                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                    s.Write(buffer, 0, sourceBytes);
                                } while (sourceBytes > 0);
                            }
                        }

                        // Finish/Close arent needed strictly as the using statement does this automatically

                        // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                        // the created file would be invalid.
                        s.Finish();

                        // Close is important to wrap things up and unlock the file.
                        s.Close();

                        this.InfoFormat("Zip {0} created.", zipPath);
                        this.Files.Add(new FileInf(zipPath, this.Id));
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the Zip {0}", e, zipPath);
                }
            }

            this.Info("Task finished.");
        }
    }
}
