using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;

namespace Wexflow.Clients.Manager
{
    public class WorkflowInfo:IComparable
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public LaunchType LaunchType { get; private set; }
        public bool IsEnabled { get; private set; }
        public string Description { get; private set; }

        public WorkflowInfo(int id, string name, LaunchType launchType, bool isEnabled, string desc)
        {
            this.Id = id;
            this.Name = name;
            this.LaunchType = launchType;
            this.IsEnabled = isEnabled;
            this.Description = desc;
        }

        public int CompareTo(object obj)
        {
            WorkflowInfo wfi = (WorkflowInfo)obj;
            return wfi.Id.CompareTo(this.Id);
        }
    }
}
