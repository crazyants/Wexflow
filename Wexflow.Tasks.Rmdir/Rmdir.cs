using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Rmdir
{
    public class Rmdir:Task
    {
        public string[] Folders { get; private set; }

        public Rmdir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.Folders = this.GetSettings("folder");
        }

        public override void Run()
        {
            this.Info("Removing folders...");
            foreach (string folder in this.Folders)
            {
                try
                {
                    RmdirRec(folder);
                    this.InfoFormat("Folder {0} deleted.", folder);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while deleting the folder {0}", e, folder);
                }
            }
            this.Info("Task finished.");
        }

        private void RmdirRec(string folder)
        {
            foreach (string file in Directory.GetFiles(folder)) File.Delete(file);
            foreach (string dir in Directory.GetDirectories(folder)) RmdirRec(dir);
            Directory.Delete(folder);
        }
    }
}
