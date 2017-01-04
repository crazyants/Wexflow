using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Mkdir
{
    public class Mkdir:Task
    {
        public string[] Folders { get; private set; }

        public Mkdir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.Folders = this.GetSettings("folder");
        }

        public override void Run()
        {
            this.Info("Creating folders...");
            foreach (string folder in this.Folders)
            {
                try
                {
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    this.InfoFormat("Folder {0} created.", folder);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the folder {0}", e, folder);
                }
            }
            this.Info("Task finished.");
        }
    }
}
