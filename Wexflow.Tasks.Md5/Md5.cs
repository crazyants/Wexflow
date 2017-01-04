using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Wexflow.Tasks.Md5
{
    public class Md5:Task
    {
        public Md5(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override void Run()
        {
            this.Info("Generating MD5 sums...");

            FileInf[] files = this.SelectFiles();

            if (files.Length > 0)
            {
                string md5Path = Path.Combine(this.Workflow.WorkflowTempFolder,
                    string.Format("MD5_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                XDocument xdoc = new XDocument(new XElement("Files"));
                foreach (FileInf file in files)
                {
                    try
                    {
                        string md5 = GetMd5(file.Path);
                        xdoc.Root.Add(new XElement("File",
                            new XAttribute("path", file.Path),
                            new XAttribute("name", file.FileName),
                            new XAttribute("md5", md5)));
                        this.InfoFormat("Md5 of the file {0} is {1}", file.Path, md5);
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        this.ErrorFormat("An error occured while generating the md5 of the file {0}", e, file.Path);
                    }
                }
                xdoc.Save(md5Path);
                this.Files.Add(new FileInf(md5Path, this.Id));
            }

            this.Info("Task finished.");
        }

        private string GetMd5(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] bytes = md5.ComputeHash(stream);

                    foreach (byte bt in bytes)
                    {
                        sb.Append(bt.ToString("x2"));
                    }
                }
            }
            return sb.ToString();
        }
    }
}
