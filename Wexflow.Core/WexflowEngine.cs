using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Threading;

namespace Wexflow.Core
{
    public class WexflowEngine
    {
        public string SettingsFile { get; private set; }
        public string WorkflowsFolder { get; private set; }
        public string TempFolder { get; private set; }
        public Workflow[] Workflows { get; private set; }

        public WexflowEngine(string settingsFile) 
        {
            this.SettingsFile = settingsFile;
            LoadSettings();
            LoadWorkflows();
        }

        private void LoadSettings()
        {
            XDocument xdoc = XDocument.Load(this.SettingsFile);
            this.WorkflowsFolder = GetWexflowSetting(xdoc, "workflowsFolder");
            this.TempFolder = GetWexflowSetting(xdoc, "tempFolder");
        }

        private string GetWexflowSetting(XDocument xdoc, string name)
        {
            return xdoc.XPathSelectElement(string.Format("/Wexflow/Setting[@name='{0}']", name)).Attribute("value").Value;    
        }

        private void LoadWorkflows()
        { 
            List<Workflow> workflows = new List<Workflow>();
            foreach (string file in Directory.GetFiles(this.WorkflowsFolder))
            {
                try
                {
                    Workflow workflow = new Workflow(file, this.TempFolder);
                    workflows.Add(workflow);
                    Logger.InfoFormat("Workflow loaded: {0}", workflow);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while loading the workflow : {0} Please check the workflow configuration.", file);
                }
            }
            this.Workflows = workflows.ToArray();
        }

        public void Run()
        {
            foreach (Workflow workflow in this.Workflows)
            {
                if (workflow.IsEnabled)
                {
                    if (workflow.LaunchType == LaunchType.Startup)
                    {
                        workflow.Start();
                    }
                    else if (workflow.LaunchType == LaunchType.Periodic)
                    {
                        Action<object> callback = o =>
                        {
                            Workflow wf = (Workflow)o;
                            if (!wf.IsRunning) wf.Start();
                        };
                        
                        WexflowTimer timer = new WexflowTimer(new TimerCallback(callback), workflow, workflow.Period);
                        timer.Start();
                    }
                }
            }
        }

        public Workflow GetWorkflow(int workflowId)
        {
            return this.Workflows.FirstOrDefault(wf => wf.Id == workflowId);
        }

        public void StartWorkflow(int workflowId)
        {
            Workflow wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else 
            {
                if (wf.IsEnabled) wf.Start();
            }
        }

        public void StopWorkflow(int workflowId)
        {
            Workflow wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Stop();
            }
        }

        public void PauseWorkflow(int workflowId)
        {
            Workflow wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Pause();
            }
        }

        public void ResumeWorkflow(int workflowId)
        {
            Workflow wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Resume();
            }
        }

    }
}
