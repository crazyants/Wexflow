using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesMover
{
    public class FilesMover:Task
    {
        public string DestFolder { get; private set; }

        public FilesMover(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.DestFolder = this.GetSetting("destFolder");
        }

        public override void Run()
        {
            this.Info("Moving files...");
            
            FileInf[] files = this.SelectFiles();
            for (int i = files.Length - 1; i > -1 ; i--) 
            {
                FileInf file = files[i];
                string destFilePath = Path.Combine(this.DestFolder, Path.GetFileName(file.Path));

                try
                {
                    if (File.Exists(destFilePath))
                    {
                        this.ErrorFormat("Destination file {0} already exists.", destFilePath);
                    }
                    else
                    {
                        File.Move(file.Path, destFilePath);
                        FileInf fi = new FileInf(destFilePath, this.Id);
                        this.Files.Add(fi);
                        this.Workflow.FilesPerTask[file.TaskId].Remove(file);
                        this.InfoFormat("File moved: {0} -> {1}", file.Path, destFilePath);
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                { 
                    this.ErrorFormat("An error occured while moving the file {0} to {1}", e, file.Path, destFilePath);
                }
            }

            this.Info("Task finished.");
        }
    }
}
