using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesCopier
{
    public class FilesCopier:Task
    {
        public string DestFolder { get; private set; }
        public bool Overwrite { get; private set; }

        public FilesCopier(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.DestFolder = this.GetSetting("destFolder");
            this.Overwrite = bool.Parse(this.GetSetting("overwrite"));
        }

        public override void Run()
        {
            this.Info("Copying files...");
            foreach (FileInf file in this.SelectFiles())
            {
                string destPath = Path.Combine(this.DestFolder, file.FileName);
                try
                {
                    File.Copy(file.Path, destPath, this.Overwrite);
                    this.Files.Add(new FileInf(destPath, this.Id));
                    this.InfoFormat("File copied: {0} -> {1}", file.Path, destPath);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while copying the file {0} to {1}.", e, file.Path, destPath);
                }
            }
            this.Info("Task finished.");
        }
    }
}
