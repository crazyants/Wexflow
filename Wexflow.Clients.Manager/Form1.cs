using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Wexflow.Core;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using Wexflow.Core.Service.Contracts;
using System.Windows.Threading;
using System.Diagnostics;

namespace Wexflow.Clients.Manager
{
    // TODO Wexflow Editor
    // TODO WebApp
    // TODO FilesRenamer?, YouTube?

    public partial class Form1 : Form
    {     
        private const string COLUMN_ID = "Id";
        private const string COLUMN_ENABLED = "Enabled";
        private const int TIMER_INTERVAL = 100; // ms

        private WexflowServiceClient _wexflowServiceClient;
        private WorkflowInfo[] _workflows;
        private Dictionary<int, Timer> _timers;
        private Dictionary<int, bool> _previousIsRunning;
        private Dictionary<int, bool> _previousIsPaused;
        private bool _windowsServiceWasStopped;

        public Form1()
        {
            InitializeComponent();

            this.textBoxInfo.Text = "Loading workflows...";

            this._timers = new Dictionary<int, Timer>();
            this._previousIsRunning = new Dictionary<int, bool>();
            this._previousIsPaused = new Dictionary<int, bool>();

            this.backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
            {
                this._wexflowServiceClient = new WexflowServiceClient();
                this._workflows = _wexflowServiceClient.GetWorkflows();
            }
            else 
            {
                this._workflows = new WorkflowInfo[] { };
                this.textBoxInfo.Text = "";
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindDataGridView();   
        }

        private void BindDataGridView()
        {
            SortableBindingList<WorkflowDataInfo> workflows = new SortableBindingList<WorkflowDataInfo>();
            foreach (WorkflowInfo workflow in this._workflows)
            {
                workflows.Add(new WorkflowDataInfo(workflow.Id, workflow.Name, workflow.LaunchType, workflow.IsEnabled, workflow.Description));
            }
            this.dataGridViewWorkflows.DataSource = workflows;

            this.dataGridViewWorkflows.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewWorkflows.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewWorkflows.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewWorkflows.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewWorkflows.Columns[3].Name = COLUMN_ENABLED;
            this.dataGridViewWorkflows.Columns[3].HeaderText = COLUMN_ENABLED;
            this.dataGridViewWorkflows.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            this.dataGridViewWorkflows.Sort(this.dataGridViewWorkflows.Columns[0], ListSortDirection.Ascending);
        }

        private int GetSlectedWorkflowId()
        {
            int wfId = -1;
            if (dataGridViewWorkflows.SelectedRows.Count > 0)
            {
                if(Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
                {
                    wfId = int.Parse(dataGridViewWorkflows.SelectedRows[0].Cells[COLUMN_ID].Value.ToString());
                }
                else
                {
                    HandleNonRunningWindowsService();
                }
            }
            return wfId;
        }

        private WorkflowInfo GetWorkflow(int id)
        {
            if (Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
            {
                if (this._windowsServiceWasStopped)
                {
                    this._wexflowServiceClient = new WexflowServiceClient();
                    this._windowsServiceWasStopped = false;
                    this.UpdateButtons(id, true);
                }
                return this._wexflowServiceClient.GetWorkflow(id);
            }
            else
            {
                this._windowsServiceWasStopped = true;
                HandleNonRunningWindowsService();
            }

            return null;
        }

        private void HandleNonRunningWindowsService()
        {
            this.buttonStart.Enabled = this.buttonPause.Enabled = this.buttonResume.Enabled = this.buttonStop.Enabled = false;
            this.textBoxInfo.Text = "Wexflow Windows Service is not running.";
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this._wexflowServiceClient.StartWorkflow(wfId);
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this._wexflowServiceClient.SuspendWorkflow(wfId);
                this.UpdateButtons(wfId, true);
            }
        }

        private void buttonResume_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this._wexflowServiceClient.ResumeWorkflow(wfId);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this._wexflowServiceClient.StopWorkflow(wfId);
                this.UpdateButtons(wfId, true);
            }
        }

        private void dataGridViewWorkflows_SelectionChanged(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();

            if (wfId > -1)
            {
                WorkflowInfo workflow = this.GetWorkflow(wfId);

                foreach (Timer timer in this._timers.Values) timer.Stop();

                if (workflow.IsEnabled)
                {
                    if (!this._timers.ContainsKey(wfId))
                    {
                        Timer timer = new Timer();
                        timer.Interval = TIMER_INTERVAL;
                        timer.Tick += new EventHandler((o, ea) =>
                            {
                                this.UpdateButtons(wfId, false);
                            });
                        this._timers.Add(wfId, timer);
                    }

                    this.UpdateButtons(wfId, true);
                    this._timers[wfId].Start();
                }
                else
                {
                    this.UpdateButtons(wfId, true);
                }
            }
        }

        private bool WorkflowStatusChanged(WorkflowInfo workflow)
        {
            bool changed = false;
            if (!this._previousIsRunning.ContainsKey(workflow.Id))
            {
                this._previousIsRunning.Add(workflow.Id, workflow.IsRunning);
                changed = true;
            }
            if (!this._previousIsPaused.ContainsKey(workflow.Id))
            {
                this._previousIsPaused.Add(workflow.Id, workflow.IsPaused);
                changed = true;
            }
            if (changed)
            {
                return true;
            }
            else
            {
                if (this._previousIsRunning[workflow.Id] != workflow.IsRunning)
                {
                    changed = true;
                }
                if (this._previousIsPaused[workflow.Id] != workflow.IsPaused)
                {
                    changed = true;
                }
                this._previousIsRunning[workflow.Id] = workflow.IsRunning;
                this._previousIsPaused[workflow.Id] = workflow.IsPaused;
                return changed;
            }
        }

        private void UpdateButtons(int wfId, bool force)
        {
            if (wfId > -1)
            {
                WorkflowInfo workflow = this.GetWorkflow(wfId);

                if (workflow != null)
                {
                    if (!workflow.IsEnabled)
                    {
                        this.textBoxInfo.Text = "This workflow is disabled.";
                        this.buttonStart.Enabled = this.buttonPause.Enabled = this.buttonResume.Enabled = this.buttonStop.Enabled = false;
                    }
                    else
                    {
                        if (!force && !WorkflowStatusChanged(workflow)) return;

                        buttonStart.Enabled = !workflow.IsRunning;
                        buttonStop.Enabled = workflow.IsRunning && !workflow.IsPaused;
                        buttonPause.Enabled = workflow.IsRunning && !workflow.IsPaused;
                        buttonResume.Enabled = workflow.IsPaused;

                        if (workflow.IsRunning && !workflow.IsPaused)
                        {
                            this.textBoxInfo.Text = "This workflow is running...";
                        }
                        else if (workflow.IsPaused)
                        {
                            this.textBoxInfo.Text = "This workflow is paused.";
                        }
                        else
                        {
                            this.textBoxInfo.Text = "";
                        }
                    }
                }
            }
        }

        private void dataGridViewWorkflows_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int wfId = this.GetSlectedWorkflowId();
            if (wfId > -1)
            { 
                WorkflowInfo workflow = this.GetWorkflow(wfId);

                if (workflow != null && workflow.IsEnabled)
                {
                    if (!workflow.IsRunning && !workflow.IsPaused)
                    {
                        buttonStart_Click(this, null);
                    }
                    else if(workflow.IsPaused)
                    {
                        buttonResume_Click(this, null);
                    }
                }
            }
        }

        private void WriteEventLog(string msg, EventLogEntryType eventLogEntryType)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(msg, eventLogEntryType, 101, 1);
            }
        }

        private void WriteEventLogError(string msg)
        {
            this.WriteEventLog(msg, EventLogEntryType.Error);
        }
    }
}
