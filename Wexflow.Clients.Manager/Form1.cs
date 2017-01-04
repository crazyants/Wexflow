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

namespace Wexflow.Clients.Manager
{
    public partial class Form1 : Form
    {
        public static string SETTINGS_FILE = 
            ConfigurationManager.AppSettings["WexflowSettingsFile"];
        
        private const string COLUMN_ID = "Id";
        private const string COLUMN_ENABLED = "Enabled";
        private const int TIMER_INTERVAL = 100; // ms

        private WexflowEngine _wexflowEngine;
        private Dictionary<int, Timer> _timers;
        private Dictionary<int, bool> _previousIsRunning;
        private Dictionary<int, bool> _previousIsPaused;

        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        public Form1()
        {
            // Display the logger
            AllocConsole();
          
            // Initialize the form
            InitializeComponent();
            this._wexflowEngine = new WexflowEngine(SETTINGS_FILE);
            this._wexflowEngine.Run();
            BindDataGridView(_wexflowEngine.Workflows);
            this._timers = new Dictionary<int, Timer>();
            this._previousIsRunning = new Dictionary<int, bool>();
            this._previousIsPaused = new Dictionary<int, bool>();
        }

        private void BindDataGridView(Workflow[] workflows)
        {
            List<WorkflowInfo> wfis = new List<WorkflowInfo>();
            foreach (Workflow wf in workflows)
            {
                wfis.Add(new WorkflowInfo(wf.Id, wf.Name, wf.LaunchType, wf.IsEnabled, wf.Description));
            }
            this.dataGridViewWorkflows.DataSource = new SortableBindingList<WorkflowInfo>(wfis);

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
                wfId = int.Parse(dataGridViewWorkflows.SelectedRows[0].Cells[COLUMN_ID].Value.ToString());
            }
            return wfId;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this._wexflowEngine.StartWorkflow(wfId);
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowEngine.PauseWorkflow(wfId);
                this.UpdateButtons(wfId, true);
            }
        }

        private void buttonResume_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowEngine.ResumeWorkflow(wfId);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this._wexflowEngine.StopWorkflow(wfId);
                this.UpdateButtons(wfId, true);
            }
        }

        private void dataGridViewWorkflows_SelectionChanged(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();

            if (wfId > -1)
            {
                Workflow workflow = _wexflowEngine.GetWorkflow(wfId);

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

        private bool WorkflowStatusChanged(Workflow workflow)
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
                Workflow workflow = _wexflowEngine.GetWorkflow(wfId);

                if (workflow != null)
                {
                    if (!workflow.IsEnabled)
                    {
                        this.textBoxInfo.Text = "This workflow is disabled.";
                        this.buttonStart.Enabled = this.buttonPause.Enabled = this.buttonResume.Enabled
                            = this.buttonStop.Enabled = false;
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
                Workflow workflow = _wexflowEngine.GetWorkflow(wfId);

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
    }
}
