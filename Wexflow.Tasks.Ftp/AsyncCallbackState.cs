using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentFTP;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesSender
{
    public class AsyncCallbackState
    {
        public FtpClient FtpCLient { get; private set; }
        public FileInf File { get; private set; }

        public AsyncCallbackState(FtpClient ftpClient, FileInf file)
        {
            this.FtpCLient = ftpClient;
            this.File = file;
        }

    }
}
