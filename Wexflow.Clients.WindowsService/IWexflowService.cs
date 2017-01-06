using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Wexflow.Core;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.WindowsService
{
    [ServiceContract(Namespace = "http://WexflowService/")]
    public interface IWexflowService
    {
        [OperationContract]
        string Hello();

        [OperationContract]
        WorkflowInfo[] GetWorkflows();

        [OperationContract]
        void StartWorkflow(int workflowId);

        [OperationContract]
        void StopWorkflow(int workflowId);

        [OperationContract]
        void SuspendWorkflow(int workflowId);

        [OperationContract]
        void ResumeWorkflow(int workflowId);

        [OperationContract]
        WorkflowInfo GetWorkflow(int workflowId);
    }
}
