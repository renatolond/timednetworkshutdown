using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Management;
using System.Net;
using System.IO;
using System.Timers;
using System.Runtime.InteropServices;

namespace Shutdown
{

    /// <summary>

    /// Summary description for Shutdown.

    /// </summary>

    public class Shutdown
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]

        internal struct TokPriv1Luid
        {

            public int Count;

            public long Luid;

            public int Attr;

        }

        [DllImport("kernel32.dll", ExactSpelling = true)]

        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]

        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]

        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]

        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,

        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InitiateSystemShutdownEx(
            string lpMachineName,
            string lpMessage,
            uint dwTimeout,
            bool bForceAppsClosed,
            bool bRebootAfterShutdown,
            ShutdownReason dwReason);

        [Flags]
        public enum ShutdownReason : uint
        {
            // Microsoft major reasons.
            SHTDN_REASON_MAJOR_OTHER = 0x00000000,
            SHTDN_REASON_MAJOR_NONE = 0x00000000,
            SHTDN_REASON_MAJOR_HARDWARE = 0x00010000,
            SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000,
            SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000,
            SHTDN_REASON_MAJOR_APPLICATION = 0x00040000,
            SHTDN_REASON_MAJOR_SYSTEM = 0x00050000,
            SHTDN_REASON_MAJOR_POWER = 0x00060000,
            SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000,

            // Microsoft minor reasons.
            SHTDN_REASON_MINOR_OTHER = 0x00000000,
            SHTDN_REASON_MINOR_NONE = 0x000000ff,
            SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001,
            SHTDN_REASON_MINOR_INSTALLATION = 0x00000002,
            SHTDN_REASON_MINOR_UPGRADE = 0x00000003,
            SHTDN_REASON_MINOR_RECONFIG = 0x00000004,
            SHTDN_REASON_MINOR_HUNG = 0x00000005,
            SHTDN_REASON_MINOR_UNSTABLE = 0x00000006,
            SHTDN_REASON_MINOR_DISK = 0x00000007,
            SHTDN_REASON_MINOR_PROCESSOR = 0x00000008,
            SHTDN_REASON_MINOR_NETWORKCARD = 0x00000000,
            SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a,
            SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b,
            SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c,
            SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d,
            SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e,
            SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F,
            SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010,
            SHTDN_REASON_MINOR_HOTFIX = 0x00000011,
            SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012,
            SHTDN_REASON_MINOR_SECURITY = 0x00000013,
            SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014,
            SHTDN_REASON_MINOR_WMI = 0x00000015,
            SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016,
            SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017,
            SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018,
            SHTDN_REASON_MINOR_MMC = 0x00000019,
            SHTDN_REASON_MINOR_TERMSRV = 0x00000020,

            // Flags that end up in the event log code.
            SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000,
            SHTDN_REASON_FLAG_PLANNED = 0x80000000,
            SHTDN_REASON_UNKNOWN = SHTDN_REASON_MINOR_NONE,
            SHTDN_REASON_LEGACY_API = (SHTDN_REASON_MAJOR_LEGACY_API | SHTDN_REASON_FLAG_PLANNED),

            // This mask cuts out UI flags.
            SHTDN_REASON_VALID_BIT_MASK = 0xc0ffffff
        }
        
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;

        internal const int TOKEN_QUERY = 0x00000008;

        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        internal static void ShutDown(uint secs, string msg, ShutdownReason flags)
        {
           bool ok;
           TokPriv1Luid tp;
           IntPtr hproc = GetCurrentProcess();
           IntPtr htok = IntPtr.Zero;
           ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
           tp.Count = 1;
           tp.Luid = 0;
           tp.Attr = SE_PRIVILEGE_ENABLED;
           ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
           ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
           ok = InitiateSystemShutdownEx(Environment.MachineName, msg, secs, true, false, flags);
       }

        internal static void Restart(uint secs, string msg, ShutdownReason flags)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = InitiateSystemShutdownEx(Environment.MachineName, msg, secs, true, true, flags);
        }
    }
}

namespace TimedNetowrkShutdownService
{

    public partial class TimedNetworkShutdownService : ServiceBase
    {
        Logger log;
        bool itsMeShuttingDown;
        const string basePagina = "http://www.dcc.ufrj.br/~lond/shutdown.php";
        string nomeMaquina;

        public TimedNetworkShutdownService()
        {
            InitializeComponent();
            log = new Logger();
            itsMeShuttingDown = false;
            nomeMaquina = System.Environment.MachineName;
        }

        protected override void OnStart(string[] args)
        {
            bool itsTime = false;

            itsTime = PerguntaServidor();
            if (itsTime)
                ShutDownNow();
            else
            {
                timerWakeUp.Interval = 10 * 60 * 1000; // 10 minutos
                timerWakeUp.Enabled = true;
                timerWakeUp.Elapsed += new System.Timers.ElapsedEventHandler(timerWakeUp_Tick);
                timerWakeUp.AutoReset = true;
                log.Write("ShutdownService foi ligado em " + NowFormatado());
            }
        }

        private bool PerguntaServidor()
        {
            string resp;
            string pagina = basePagina + "?nome=" + nomeMaquina;
            resp = PegaResposta(pagina);
            if (resp.Contains("sim"))
                return true;
            if (!resp.Contains("nao"))
            {
                log.Write("String de resposta inesperada em " + NowFormatado());
                log.Write(resp);
            }
            return false;
        }

        private string PegaResposta(string pagina)
        {
            // used to build entire input
            StringBuilder sb = new StringBuilder();

            // used on each read operation
            byte[] buf = new byte[8192];

            // prepare the web page we will be asking for
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(pagina);
            HttpWebResponse response;

            try
            {
                // execute the request
                response = (HttpWebResponse)
                    request.GetResponse();
            }
            catch ( Exception e )
            {
                log.Write("Erro ao conectar-se ao servidor em " + NowFormatado());
                log.Write("Erro:" + e.Message);
                return "NAO";
            }

            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();

            string tempString = null;
            int count = 0;

            do
            {
                // fill the buffer with data
                count = resStream.Read(buf, 0, buf.Length);

                // make sure we read some data
                if (count != 0)
                {
                    // translate from bytes to ASCII text
                    tempString = Encoding.ASCII.GetString(buf, 0, count);

                    // continue building the string
                    sb.Append(tempString);
                }
            }
            while (count > 0); // any more data to read?

            return sb.ToString();
        }

        protected string NowFormatado()
        {
            return DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
        }

        protected void ShutDownNow()
        {
            log.Write("Desligando o computador em " + NowFormatado());
            itsMeShuttingDown = true;
            //ManagementBaseObject mboShutdown = null;
            //ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            //mcWin32.Get();
            //// You can't shutdown without security privileges
            //mcWin32.Scope.Options.EnablePrivileges = true;
            //ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");
            //// Flag 1 means we want to shut down the system
            //mboShutdownParams["Flags"] = "1";
            //mboShutdownParams["Reserved"] = "0";
            //foreach (ManagementObject manObj in mcWin32.GetInstances())
            //{
            //    mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
            //}
            Shutdown.Shutdown.ShutDown(30, "O sistema será desligado por pedido do servidor de desligamento",
                Shutdown.Shutdown.ShutdownReason.SHTDN_REASON_MAJOR_OTHER | 
                Shutdown.Shutdown.ShutdownReason.SHTDN_REASON_MINOR_OTHER | 
                Shutdown.Shutdown.ShutdownReason.SHTDN_REASON_FLAG_PLANNED);
        }

        protected override void OnStop()
        {
            log.Write("ShutDownService foi parado em " + NowFormatado());
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            string pagina = basePagina + "?nome="+nomeMaquina + "&shutting=y";

            if ( !itsMeShuttingDown )
                PegaResposta(pagina);
        }

        private void timerWakeUp_Tick(object sender, ElapsedEventArgs e)
        {
            bool itsTime;

            itsTime = PerguntaServidor();
            if (itsTime)
                ShutDownNow();
        }
    }
}
