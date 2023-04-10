using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Common.Logging
{
    public class DefaultLogger : ILogger
    {
        public void Debug(string message)
        {
            Console.WriteLine($"[Debug][{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]{message}");
        }

        public void Error(string message)
        {
            Console.WriteLine($"[Error][{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]{message}");
        }

        public void Error(string message, Exception ex)
        {
            Console.WriteLine($"[Error][{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]{message}\r\n{ex.Message}\r\n{ex.StackTrace}");
        }

        public void Error(Exception ex)
        {
            Console.WriteLine($"[Error][{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]{ex.Message}\r\n{ex.StackTrace}");
        }

        public void Info(string message)
        {
            Console.WriteLine($"[Info][{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]{message}");
        }

        public void Warn(string message)
        {
            Console.WriteLine($"[Warn][{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]{message}");
        }
    }
}
