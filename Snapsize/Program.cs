using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snapsize
{
    static class Program
    {
        private static readonly Mutex MasterProcessInstanceMutex = new Mutex(false, "{4FF962BE-1D89-438E-BA23-3B15685C6919}");
        private static readonly Mutex Single32BitInstanceMutex = new Mutex(false, "{EA94DE51-451E-4B31-A233-CA4934711727}");
        private static readonly Mutex Single64BitInstanceMutex = new Mutex(false, "{5C6F0CE6-0C54-47F0-B52D-B225FC8750E5}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var architectureMutex = Environment.Is64BitProcess ? Single64BitInstanceMutex : Single32BitInstanceMutex;
            if (architectureMutex.WaitOne(TimeSpan.Zero))
            {
                try
                {
                    Run(args);
                }
                finally
                {
                    architectureMutex.ReleaseMutex();
                }
            }
        }

        private static void Run(string[] args)
        {
            if (MasterProcessInstanceMutex.WaitOne(TimeSpan.Zero))
            {
                Process relayProcess = null;
                try
                {
                    relayProcess = RunFullApplication();
                }
                finally
                {
                    MasterProcessInstanceMutex.ReleaseMutex();
                }
            }
            else
            {
                // we are just relaying messages for the master process
                var window = ParseWindowHandle(args);
                if (window == IntPtr.Zero)
                {
                    // master process should have passed us a window handle
                    return;
                }

                //Debugger.Launch();

                MainForm mainForm = MainForm.CreateForGlobalHookRelayOnly(window);

                // when the main application exits, we should exit
                new Thread(new ThreadStart(() => {
                    try
                    {
                        MasterProcessInstanceMutex.WaitOne();
                    }
                    finally
                    {
                        MasterProcessInstanceMutex.ReleaseMutex();
                        mainForm.CleanUpForShutdown();
                        Application.Exit();
                    }
                })).Start();

                Application.Run(mainForm);
            }
        }

        private static Process RunFullApplication()
        {
            Process relayProcess = null;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = MainForm.CreateForFullApplication();

            if (Environment.Is64BitOperatingSystem)
            {
                var relayProcessName = (Environment.Is64BitProcess ? "snapsize.exe" : "snapsize64.exe");
                relayProcess = Process.Start(relayProcessName, mainForm.Handle.ToString());
            }

            Application.Run(mainForm);

            return relayProcess;
        }

        private static IntPtr ParseWindowHandle(string[] args)
        {
            if (args.Length == 0)
            {
                return IntPtr.Zero;
            }
            if (!int.TryParse(args[0], out int parsed))
            {
                return IntPtr.Zero;
            }
            return (IntPtr)parsed;
        }
    }
}
