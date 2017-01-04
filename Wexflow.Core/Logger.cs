using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Wexflow.Core
{
    public static class Logger
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Logger));

        public static void Info(string msg)
        {
            logger.Info(msg);
        }

        public static void InfoFormat(string msg, params object[] args)
        {
            logger.InfoFormat(msg, args);
        }

        public static void Debug(string msg)
        {
            logger.Debug(msg);
        }

        public static void DebugFormat(string msg, params object[] args)
        {
            logger.DebugFormat(msg, args);
        }

        public static void Error(string msg)
        {
            logger.Error(msg);
        }

        public static void ErrorFormat(string msg, params object[] args)
        {
            logger.ErrorFormat(msg, args);
        }

        public static void Error(string msg, Exception e)
        {
            logger.Error(msg, e);
        }

        public static void ErrorFormat(string msg, Exception e, params object[] args)
        {
            logger.Error(string.Format(msg, args), e);
        }
    }
}
