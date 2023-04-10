using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Common.Logging
{
    public static class LogUtil
    {
        private static ILogger logger = LoggerFactory.GetLogger();

        public static void Debug(string message)
        {
            logger.Debug(message);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }

        public static void Error(string message, Exception ex)
        {
            logger.Error(message, ex);
        }

        public static void Error(Exception ex)
        {
            logger.Error(ex);
        }

        public static void Info(string message)
        {
            logger.Info(message);
        }

        public static void Warn(string message)
        {
            logger.Warn(message);
        }
    }
}
