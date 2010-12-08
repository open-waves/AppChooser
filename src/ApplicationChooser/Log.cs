using System;
using System.IO;

namespace ApplicationChooser
{
    public class Log
    {
        private static string logPath = "appChooser.log";

        public static void WriteLine(string text)
        {
            File.AppendAllText(logPath, string.Format("{0}\r\n", text));
        }

        public static void WriteLine(string text, Exception ex)
        {
            WriteLine(string.Format("{0}\r\n{1}", text, ex.Message));
        }
    }
}