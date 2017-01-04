using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core
{
    public class FileInf
    {
        public string Path { get; private set; }
        public string FileName { get; private set; }
        public int TaskId { get; private set; }

        public FileInf(string path, int taskId)
        {
            this.Path = path;
            this.FileName = System.IO.Path.GetFileName(this.Path);
            this.TaskId = taskId;
        }
    }
}