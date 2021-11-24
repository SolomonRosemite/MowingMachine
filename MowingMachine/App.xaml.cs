using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int AttachParentProcess = -1;

        protected override void OnStartup(StartupEventArgs e)
        {
            AttachToParentConsole();
            base.OnStartup(e);
        }

        /// <summary>
        ///     Redirects the console output of the current process to the parent process.
        /// </summary>
        private static void AttachToParentConsole()
        {
            AttachConsole(AttachParentProcess);
        }
    }
}