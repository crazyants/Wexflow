using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.XmlToCsv
{
    public class XmlToCsv:Task
    {
        public XmlToCsv(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override void Run()
        {
            this.Info("Creating csv files...");
            foreach (FileInf file in this.SelectFiles())
            {
                try
                {
                    string csvPath = Path.Combine(this.Workflow.WorkflowTempFolder,
                        string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.csv", Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now));
                    CreateCsv(file.Path, csvPath);
                    this.InfoFormat("Csv file {0} created from {1}", csvPath, file.Path);
                    this.Files.Add(new FileInf(csvPath, this.Id));
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the Csv from the file {0}.", e, file.Path);
                }
            }
            this.Info("Task finished.");
        }

        private void CreateCsv(string xmlPath, string csvPath)
        {
            XDocument xdoc = XDocument.Load(xmlPath);

            List<string> lines = new List<string>();
            foreach (XElement xLine in xdoc.XPathSelectElements("Lines/Line"))
            {
                StringBuilder sb = new StringBuilder();
                foreach (XElement xColumn in xLine.XPathSelectElements("Column"))
                {
                    sb.Append(xColumn.Value).Append(";");
                }
                lines.Add(sb.ToString());
            }
            File.WriteAllLines(csvPath, lines.ToArray());
        }
    }
}
