using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Wexflow.Core
{
    public enum LaunchType
    { 
        Startup,
        Trigger,
        Periodic
    }

    public class Workflow
    {
        public string WorkflowFilePath { get; private set; }
        public string WexflowTempFolder { get; private set; }
        public string WorkflowTempFolder { get; private set; }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public LaunchType LaunchType { get; private set; }
        public TimeSpan Period { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public Task[] Taks { get; private set; }
        public Dictionary<int, List<FileInf>> FilesPerTask { get; private set; }
        public Dictionary<int, List<Entity>> EntitiesPerTask { get; private set; }
        public int JobId { get; private set; }
        public string LogTag { get { return string.Format("[{0} / {1}]", this.Name, this.JobId); } }

        private Thread _thread;

        public Workflow(string path, string wexflowTempFolder)
        {
            this.JobId = 1;
            this._thread = null;
            this.WorkflowFilePath = path;
            this.WexflowTempFolder = wexflowTempFolder;
            this.FilesPerTask = new Dictionary<int, List<FileInf>>();
            this.EntitiesPerTask = new Dictionary<int, List<Entity>>();
            Load();
        }

        public override string ToString()
        {
            return string.Format("{{id: {0}, name: {1}, enabled: {2}, launchType: {3}}}"
                , this.Id, this.Name, this.IsEnabled, this.LaunchType);
        }

        private void Load()
        {
            XDocument xdoc = XDocument.Load(this.WorkflowFilePath);
            this.Id = int.Parse(GetWorkflowAttribute(xdoc, "id"));
            this.Name = GetWorkflowAttribute(xdoc, "name");
            this.Description = GetWorkflowAttribute(xdoc, "description");
            this.LaunchType = (LaunchType)Enum.Parse(typeof(LaunchType), GetWorkflowSetting(xdoc, "launchType"), true);
            if(this.LaunchType == Core.LaunchType.Periodic) this.Period = TimeSpan.Parse(GetWorkflowSetting(xdoc, "period"));
            this.IsEnabled = bool.Parse(GetWorkflowSetting(xdoc, "enabled"));

            List<Task> tasks = new List<Task>();
            foreach (XElement xTask in xdoc.XPathSelectElements("/Workflow/Tasks/Task"))
            {
                string name = xTask.Attribute("name").Value;
                string assemblyName = "Wexflow.Tasks." + name;
                string typeName = "Wexflow.Tasks." + name + "." + name + ", " + assemblyName;
                Task task = (Task)Activator.CreateInstance(Type.GetType(typeName), xTask, this);
                tasks.Add(task);
            }
            this.Taks = tasks.ToArray();
        }

        private string GetWorkflowAttribute(XDocument xdoc, string attr)
        {
            return xdoc.XPathSelectElement("/Workflow").Attribute(attr).Value;
        }

        private string GetWorkflowSetting(XDocument xdoc, string name)
        {
            return xdoc.XPathSelectElement(string.Format("/Workflow[@id='{0}']/Settings/Setting[@name='{1}']", this.Id, name)).Attribute("value").Value;
        }

        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        this.IsRunning = true;
                        Logger.InfoFormat("{0} Workflow started.", this.LogTag);

                        // Create the temp folder
                        CreateTempFolder();

                        // Run the tasks
                        foreach (Task task in this.Taks)
                        {
                            if (task.IsEnabled)
                            {
                                task.Run();
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorFormat("An error occured while running the workflow : {0}", e, this);
                    }
                    finally
                    {
                        // Cleanup
                        foreach (List<FileInf> files in this.FilesPerTask.Values) files.Clear();
                        foreach (List<Entity> entities in this.EntitiesPerTask.Values) entities.Clear();
                        this._thread = null;
                        this.IsRunning = false;
                        GC.Collect();

                        Logger.InfoFormat("{0} Workflow finished.", this.LogTag);
                        this.JobId++;
                    }
                }));

            this._thread = thread;
            thread.Start();
        }

        public void Stop()
        {
            if (this._thread != null && this.IsRunning && !this.IsPaused)
            {
                try
                {
                    this._thread.Abort();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while stopping the workflow : {0}", e, this);
                }
                finally
                {
                    this.IsRunning = false;
                    Logger.InfoFormat("{0} Workflow finished.", this.LogTag);
                    this.JobId++;
                }
            }
        }

        public void Pause()
        {
            if (this._thread != null)
            {
                try
                {
                    this._thread.Suspend();
                    this.IsPaused = true;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while suspending the workflow : {0}", e, this);
                }
            }
        }

        public void Resume()
        {
            if (this._thread != null && this.IsPaused)
            {
                try
                {
                    this._thread.Resume();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while resuming the workflow : {0}", e, this);
                }
                finally
                {
                    this.IsPaused = false;
                }
            }
        }

        private void CreateTempFolder()
        { 
            // WorkflowId/dd-MM-yyyy/HH-mm-ss-fff
            string wfTempFolder = Path.Combine(this.WexflowTempFolder, this.Id.ToString());
            if (!Directory.Exists(wfTempFolder)) Directory.CreateDirectory(wfTempFolder);
            
            string wfDayTempFolder = Path.Combine(wfTempFolder, string.Format("{0:yyyy-MM-dd}", DateTime.Now));
            if (!Directory.Exists(wfDayTempFolder)) Directory.CreateDirectory(wfDayTempFolder);

            
            string wfJobTempFolder = Path.Combine(wfDayTempFolder, string.Format("{0:HH-mm-ss-fff}", DateTime.Now));
            if (!Directory.Exists(wfJobTempFolder)) Directory.CreateDirectory(wfJobTempFolder);

            this.WorkflowTempFolder = wfJobTempFolder;
        }
    }
}
