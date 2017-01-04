using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Xml.Xsl;
using Saxon.Api;
using System.Threading;

namespace Wexflow.Tasks.Xslt
{
    public class Xslt:Task
    {
        public string XsltPath { get; private set; }
        public string Version { get; private set; }

        public Xslt(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.XsltPath = this.GetSetting("xsltPath");
            this.Version = this.GetSetting("version");
        }

        public override void Run()
        {
            this.Info("Transforming files...");

            foreach (FileInf file in this.SelectFiles())
            {
                string destPath = Path.Combine(this.Workflow.WorkflowTempFolder, file.FileName);

                try
                {
                    switch (this.Version)
                    {
                        case "1.0":
                            XslCompiledTransform xslt = new XslCompiledTransform();
                            xslt.Load(this.XsltPath);
                            xslt.Transform(file.Path, destPath);
                            this.InfoFormat("File transformed: {0} -> {1}", file.Path, destPath);
                            this.Files.Add(new FileInf(destPath, this.Id));
                            break;
                        case "2.0":
                            // Create a Processor instance.
                            Processor processor = new Processor();

                            // Load the source document.
                            XdmNode input = processor.NewDocumentBuilder().Build(new Uri(file.Path));

                            // Create a transformer for the stylesheet.
                            XsltTransformer transformer = processor.NewXsltCompiler().Compile(new Uri(this.XsltPath)).Load();

                            // Set the root node of the source document to be the initial context node.
                            transformer.InitialContextNode = input;

                            // Create a serializer.
                            Serializer serializer = new Serializer();
                            serializer.SetOutputFile(destPath);

                            // Transform the source XML to System.out.
                            transformer.Run(serializer);
                            this.InfoFormat("File transformed: {0} -> {1}", file.Path, destPath);

                            this.Files.Add(new FileInf(destPath, this.Id));
                            break;
                        default:
                            this.Error("Error in version option. Available options: 1.0 or 2.0");
                            break;
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while transforming the file {0}", e, file.Path);
                }
            }

            this.Info("Task finished.");
        }
    }
}
