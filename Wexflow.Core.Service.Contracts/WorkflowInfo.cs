using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContractAttribute]
    public class WorkflowInfo:IComparable
    {
        [DataMemberAttribute]
        public int Id { get; private set; }
        [DataMemberAttribute]
        public string Name { get; private set; }
        [DataMemberAttribute]
        public LaunchType LaunchType { get; private set; }
        [DataMemberAttribute]
        public bool IsEnabled { get; private set; }
        [DataMemberAttribute]
        public string Description { get; private set; }
        [DataMemberAttribute]
        public bool IsRunning { get; private set; }
        [DataMemberAttribute]
        public bool IsPaused { get; private set; }

        public WorkflowInfo(int id, string name, LaunchType launchType, bool isEnabled, string desc, bool isRunning, bool isPaused)
        {
            this.Id = id;
            this.Name = name;
            this.LaunchType = launchType;
            this.IsEnabled = isEnabled;
            this.Description = desc;
            this.IsRunning = isRunning;
            this.IsPaused = isPaused;
        }

        public int CompareTo(object obj)
        {
            WorkflowInfo wfi = (WorkflowInfo)obj;
            return wfi.Id.CompareTo(this.Id);
        }
    }
}
