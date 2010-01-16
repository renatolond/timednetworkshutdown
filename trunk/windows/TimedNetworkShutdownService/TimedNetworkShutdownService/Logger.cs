using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TimedShutdownService
{
    public class Logger
    {
        internal void Write(string p)
        {
            FileStream fs = new FileStream(@"c:\temp\ShutDownLog.txt", FileMode.Append, FileAccess.Write);
            StreamWriter m_streamWriter = new StreamWriter(fs);
            m_streamWriter.WriteLine(p);
            m_streamWriter.Flush();
            m_streamWriter.Close();
        }
    }
}
