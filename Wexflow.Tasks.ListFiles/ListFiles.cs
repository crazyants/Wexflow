using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;

namespace Wexflow.Tasks.ListFiles
{
    public class ListFiles:Task
    {
        public ListFiles(XElement xe, Workflow wf)
            : base(xe, wf)
        {}

        public override void Run()
        {
            this.Info("Listing files...");
            //System.Threading.Thread.Sleep(10 * 1000);
            foreach (List<FileInf> files in this.Workflow.FilesPerTask.Values)
            {
                foreach (FileInf file in files)
                {
                    this.InfoFormat("{{taskId: {0}, path: {1}}}", file.TaskId, file.Path);
                }
            }
            this.Info("Task finished.");
        }
    }
}
