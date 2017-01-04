using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Touch
{
    public class Touch:Task
    {
        public string[] TFiles { get; private set; }

        public Touch(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.TFiles = this.GetSettings("file");
        }

        public override void Run()
        {
            this.Info("Touching files...");
            foreach (string file in this.TFiles)
            {
                try
                {
                    TouchFile(file);
                    this.InfoFormat("File {0} created.", file);
                    this.Files.Add(new FileInf(file, this.Id));
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the file {0}", e, file);
                }
            }
            this.Info("Task finished.");
        }

        private void TouchFile(string file)
        {
            using (File.Create(file)) { }
        }
    }
}
