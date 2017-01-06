using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.WindowsService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults=true)]
    public class WexflowService:IWexflowService
    {
        public string Hello()
        {
            return "Hello!";
        }

        public WorkflowInfo[] GetWorkflows()
        {
            List<WorkflowInfo> wfis = new List<WorkflowInfo>();
            foreach (Workflow wf in WexflowWindowsService.WEXFLOW_ENGINE.Workflows)
            {
                wfis.Add(new WorkflowInfo(wf.Id, wf.Name, wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused));
            }
            return wfis.ToArray();
        }

        public void StartWorkflow(int workflowId)
        {
            WexflowWindowsService.WEXFLOW_ENGINE.StartWorkflow(workflowId);
        }

        public void StopWorkflow(int workflowId)
        {
            WexflowWindowsService.WEXFLOW_ENGINE.StopWorkflow(workflowId);
        }

        public void SuspendWorkflow(int workflowId)
        {
            WexflowWindowsService.WEXFLOW_ENGINE.PauseWorkflow(workflowId);
        }

        public void ResumeWorkflow(int workflowId)
        {
            WexflowWindowsService.WEXFLOW_ENGINE.ResumeWorkflow(workflowId);
        }

        public WorkflowInfo GetWorkflow(int workflowId)
        {
            Workflow wf = WexflowWindowsService.WEXFLOW_ENGINE.GetWorkflow(workflowId);
            return new WorkflowInfo(wf.Id, wf.Name, wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused);
        }
    }
}
