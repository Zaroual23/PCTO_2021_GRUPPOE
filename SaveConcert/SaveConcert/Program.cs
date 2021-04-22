using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace progetto_pcto
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;
                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                info.Arguments = "";
                try
                {
                    if (Process.Start(info) != null)
                    {
                        // The user accepted the UAC prompt
                        Environment.Exit(0);
                    }
                }
                catch (System.ComponentModel.Win32Exception e)
                {

                }
            }
            Application.Run(form);
            form.gv.Dispose();
        }
    }
}
