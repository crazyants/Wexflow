using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.Template
{
    public class Template:Task
    {
        public Template(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            // Task settings goes here
        }

        public override void Run()
        {
            try
            {
                // Task logic goes here
            }
            catch (ThreadAbortException)
            {
                throw;
            }
        }
    }
}
