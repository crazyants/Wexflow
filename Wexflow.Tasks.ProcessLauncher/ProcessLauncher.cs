using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Wexflow.Tasks.ProcessLauncher
{
    public class ProcessLauncher:Task
    {
        public string ProcessPath { get; private set; }
        public string ProcessCmd { get; private set; }
        public bool HideGui { get; private set; }
        public bool GeneratesFiles { get; private set; }

        private const string VAR_FILE_PATH = "$filePath";
        private const string VAR_FILE_NAME = "$fileName";
        private const string VAR_FILE_NAME_WITHOUT_EXTENSION = "$fileNameWithoutExtension";
        private const string VAR_OUTPUT = "$output";

        public ProcessLauncher(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.ProcessPath = this.GetSetting("processPath");
            this.ProcessCmd = this.GetSetting("processCmd");
            this.HideGui = bool.Parse(this.GetSetting("hideGui"));
            this.GeneratesFiles = bool.Parse(this.GetSetting("generatesFiles"));
        }

        public override void Run()
        {
            this.Info("Launching process...");

            if (this.GeneratesFiles && !(this.ProcessCmd.Contains(VAR_FILE_NAME) && (this.ProcessCmd.Contains(VAR_OUTPUT) && (this.ProcessCmd.Contains(VAR_FILE_NAME) || this.ProcessCmd.Contains(VAR_FILE_NAME_WITHOUT_EXTENSION)))))
            {
                this.Error("Error in process command. Please read the documentation.");
                return;
            }

            if (!GeneratesFiles)
            {
                StartProcess(this.ProcessPath, this.ProcessCmd, this.HideGui);
            }
            else
            {
                foreach (FileInf file in this.SelectFiles())
                {
                    string cmd = string.Empty;
                    string outputFilePath = string.Empty;

                    try
                    {
                        cmd = this.ProcessCmd.Replace(string.Format("{{{0}}}", VAR_FILE_PATH), string.Format("\"{0}\"", file.Path));

                        string outputRegexPattern = @"{\$output:(?:\$fileNameWithoutExtension|\$fileName)(?:[a-zA-Z0-9._-]*})";
                        Regex outputRegex = new Regex(outputRegexPattern);
                        Match m = outputRegex.Match(cmd);

                        if (m.Success)
                        {
                            string val = m.Value;
                            outputFilePath = val;
                            if (outputFilePath.Contains(VAR_FILE_NAME_WITHOUT_EXTENSION))
                            {
                                outputFilePath = outputFilePath.Replace(VAR_FILE_NAME_WITHOUT_EXTENSION, Path.GetFileNameWithoutExtension(file.FileName));
                            }
                            else if (outputFilePath.Contains(VAR_FILE_NAME))
                            {
                                outputFilePath = outputFilePath.Replace(VAR_FILE_NAME, file.FileName);
                            }
                            outputFilePath = outputFilePath.Replace("{" + VAR_OUTPUT + ":", this.Workflow.WorkflowTempFolder.Trim('\\') + "\\");
                            outputFilePath = outputFilePath.Trim('}');

                            cmd = cmd.Replace(val, "\"" + outputFilePath + "\"");
                        }
                        else
                        {
                            this.Error("Error in process command. Please read the documentation.");
                            return;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        this.ErrorFormat("Error in process command. Please read the documentation.", e);
                        return;
                    }

                    if (StartProcess(this.ProcessPath, cmd, this.HideGui))
                    {
                        this.Files.Add(new FileInf(outputFilePath, this.Id));
                    }
                }
            }

            this.Info("Task finished.");
        }

        private bool StartProcess(string processPath, string processCmd, bool hideGui)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(this.ProcessPath, processCmd);
                startInfo.CreateNoWindow = hideGui;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                Process process = new Process();
                process.StartInfo = startInfo;
                process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.ErrorFormat("An error occured while launching the process {0}", e, processPath);
                return false;
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            this.InfoFormat("{0}", outLine.Data);
        }

        private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            this.ErrorFormat("{0}", outLine.Data);
        }

    }
}
