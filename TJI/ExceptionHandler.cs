using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TJI
{
    public static class ExceptionHandler
    {
        public static bool LogExceptions { get; set; }
        public static string ExceptionInfoPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TJI\\Exceptions");
            }
        }

        public static void HandleException(WebException we)
        {
            try
            {
                if (ShouldLogException())
                {
                    WriteExceptionToFile(string.Format("{0} ({1})", we.Message, we.Status.ToString()), we);
                }
            }
            catch (Exception) { }
        }
        
        public static void HandleException(Exception e)
        {
            try
            {
                if (ShouldLogException())
                {
                    WriteExceptionToFile(e.Message, e);
                }
            }
            catch (Exception) { }
        }

        private static bool ShouldLogException()
        {
            return LogExceptions;
        }

        private static void WriteExceptionToFile(string message, Exception we)
        {
            string fileName = ExceptionInfoPath.TrimEnd(new char[] { '/', '\\' }) + "\\ExceptionInfo_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_";
            int i = 0;

            DirectoryInfo logDir = new DirectoryInfo(ExceptionInfoPath);
            if (!logDir.Exists)
            {
                Directory.CreateDirectory(logDir.FullName);
            }

            while (System.IO.File.Exists(fileName + i + ".txt"))
            {
                i++;
            }

            fileName += i + ".txt";

            StringBuilder exceptionInfo = new StringBuilder(message);
            exceptionInfo.AppendLine("");
            exceptionInfo.AppendLine(we.StackTrace);

            System.IO.File.WriteAllText(fileName, exceptionInfo.ToString(), System.Text.Encoding.UTF8);
        }
    }
}
