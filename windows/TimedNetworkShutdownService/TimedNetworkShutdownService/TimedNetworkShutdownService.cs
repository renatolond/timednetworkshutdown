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

namespace TimedShutdownService
{
    public partial class TimedShutdownService : ServiceBase
    {
        Logger log;
        bool itsMeShuttingDown;
        const string basePagina = "http://www.dcc.ufrj.br/~lond/shutdown.php";
        string nomeMaquina;

        public TimedShutdownService()
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
                timerWakeUp.Interval = 30 * 60 * 1000; // 30 minutos
                timerWakeUp.Enabled = true;
                log.Write("ShutdownService foi ligado em " + NowFormatado());
            }
        }

        private bool PerguntaServidor()
        {
            string resp;
            //string laboratorio = "LEP";
            string pagina = basePagina + "?nome=" + nomeMaquina;//+"&lab="+laboratorio;
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
            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();
            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");
            // Flag 1 means we want to shut down the system
            mboShutdownParams["Flags"] = "1";
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
            }
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

        private void timerWakeUp_Tick(object sender, EventArgs e)
        {
            bool itsTime;

            itsTime = PerguntaServidor();
            if (itsTime)
                ShutDownNow();
        }
    }
}
