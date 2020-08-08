using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AttachDebugger
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] argv)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int appProcessId = 0;
            if (!ParseArguments(argv, out appProcessId, out var eventHandle))
                return 1;

            var debuggerInfo = GetDebuggerInformation();

            var window = new MainWindow(debuggerInfo);
            Application.Run(window);

            var selectedDebugger = window.AcceptedSelection;
            if (selectedDebugger == null)
                return 2;

            if (selectedDebugger.IdeProcess != null)
            {
                VisualStudioAttacher.AttachToDebuggee(selectedDebugger.IdeProcess!, appProcessId);
            }

            return 0;
        }

        private static List<DebuggerInformation> GetDebuggerInformation()
        {
            var ideProcesses = VisualStudioAttacher.GetRunningIdeProcesses();

            var debuggerInfo = new List<DebuggerInformation>();

            foreach (var ideProcess in ideProcesses)
            {
                string? solution = VisualStudioAttacher.GetOpenSolutionFilePath(ideProcess);
                debuggerInfo.Add(new DebuggerInformation
                {
                    SolutionFilePath = solution,
                    IdeProcess = ideProcess,
                    Kind = DebuggerKind.VisualStudio
                });
            }

            /*
            var vsPaths = VisualStudioAttacher.QueryVsWhereProperty("Microsoft.VisualStudio.Workload.NativeDesktop", "installationPath");
            var devenvPaths = vsPaths.Select(path => Path.Join(path, "Common7", "IDE", "devenv.exe"));

            foreach (var path in devenvPaths)
            {
                debuggerInfo.Add(new DebuggerInformation
                {
                    Kind = DebuggerKind.VisualStudio,
                    SolutionFilePath = path
                });
            }
            */

            return debuggerInfo;
        }

        private static bool ParseArguments(string[] argv, 
                                           out int processId,
                                           out SafeWaitHandle? eventHandle)
        {
            eventHandle = null;
            processId = 0;

            if (argv.Length < 2 || argv[0] != "-p")
            {
                MessageBox.Show("Invalid arguments passed to debugger attacher. ",
                                "Attach Debugger", MessageBoxButtons.OK);
                return false;
            }

            if (!int.TryParse(argv[1], out processId) || processId <= 0)
            {
                MessageBox.Show("Invalid process ID passed to debugger attacher. ",
                                "Attach Debugger", MessageBoxButtons.OK);
                return false;
            }

            if (argv.Length >= 4 && argv[2] == "-e")
            {
                if (!int.TryParse(argv[3], out var eventHandleInt) || eventHandleInt <= 0)
                {
                    MessageBox.Show("Invalid event handle passed to debugger attacher. ",
                                    "Attach Debugger", MessageBoxButtons.OK);
                    return false;
                }

                eventHandle = new SafeWaitHandle((IntPtr)eventHandleInt, true);
            }

            return true;
        }
    }
}
