using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.CsvToXml
{
    public class CsvToXml:Task
    {
        public CsvToXml(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override void Run()
        {
            this.Info("Creating XML files...");
            foreach (FileInf file in this.SelectFiles())
            {
                try
                {
                    string xmlPath = Path.Combine(this.Workflow.WorkflowTempFolder,
                        string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.xml", Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now));
                    CreateXml(file.Path, xmlPath);
                    this.Files.Add(new FileInf(xmlPath, this.Id));
                    this.InfoFormat("XML file {0} created from {1}", xmlPath, file.Path);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the XML from {0} Please check this XML file according to the documentation of the task.", file.Path);
                }
            }
            this.Info("Task finished.");
        }

        private void CreateXml(string csvPath, string xmlPath)
        {
            XDocument xdoc = new XDocument(new XElement("Lines"));

            foreach (string line in File.ReadAllLines(csvPath))
            {
                XElement xLine = new XElement("Line");
                foreach (string col in line.Split(';'))
                {
                    if(!string.IsNullOrEmpty(col)) xLine.Add(new XElement("Column", col));
                }
                xdoc.Root.Add(xLine);
            }

            xdoc.Save(xmlPath);
        }
    }
}
