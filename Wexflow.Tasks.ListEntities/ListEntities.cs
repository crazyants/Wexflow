using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;

namespace Wexflow.Tasks.ListEntities
{
    public class ListEntities:Task
    {
        public ListEntities(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override void Run()
        {
            this.Info("Listing entities...");
            foreach (List<Entity> entities in this.Workflow.EntitiesPerTask.Values)
            {
                foreach (Entity entity in entities)
                {
                    this.InfoFormat("{{taskId: {0}, entity: {1}}}", entity.TaskId, entity);
                }
            }
            this.Info("Task finished.");
        }
    }
}
