using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AttachDebugger
{
    public static class Program
    {

        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, SetLastError = true, ExactSpelling = true)]
        private static extern bool SetEvent(SafeWaitHandle hEvent);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] argv)
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                int appProcessId = 0;
                if (!ParseArguments(argv, out appProcessId, out var eventHandle))
                    return 1;

                // When just registering this program
                if (appProcessId == 0)
                    return 0;

                try
                {
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
                }
                finally
                {
                    if (eventHandle != null)
                    {
                        bool success = SetEvent(eventHandle);
                        eventHandle.Close();

                        if (!success)
                        { 
                            throw new Win32Exception(Marshal.GetLastWin32Error(), 
                                "Could not signal event handle telling the program to be debugged to continue. ");
                        }
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("Encountered failure: \n\n" + e.ToString(), 
                                "Attach Debugger",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 3;
            }
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

        #region Registration of this program as JIT debugger

        /// <summary>
        /// Backing field for <see cref="ExecutablePath"/> property.
        /// </summary>
        private static volatile string? _exePath;

        /// <summary>
        /// Get the full filesystem path to the current executable.
        /// </summary>
        public static string ExecutablePath
        {
            get
            {
                var exePath = _exePath;
                if (exePath == null)
                {
                    exePath = new Uri(Assembly.GetEntryAssembly()!.CodeBase!, UriKind.Absolute).LocalPath;
                    if (Path.GetExtension(exePath) == ".dll")
                        exePath = Path.ChangeExtension(exePath, ".exe");

                    _exePath = exePath;
                }

                return exePath;
            }
        }

        /// <summary>
        /// Run all actions in sequence, even if any earlier action fails.
        /// </summary>
        private static void RunActions(IEnumerable<Action> actions)
        {
            List<Exception>? exceptions = null;
            foreach (var action in actions)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    exceptions = exceptions ?? new List<Exception>();
                    exceptions.Add(e);
                }
            }

            if (exceptions != null)
                throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Run all actions in sequence, even if any earlier action fails.
        /// </summary>
        private static void RunActions(params Action[] actions)
            => RunActions((IEnumerable<Action>)actions);

        /// <summary>
        /// Register or unregister this program as a JIT debugger on Windows.
        /// </summary>
        /// <param name="enable">
        /// Whether to register or unregister this program.
        /// </param>
        private static void RegisterProgram(bool enable)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                RunActions(() => RegisterProgram(RegistryView.Registry64, enable),
                           () => RegisterProgram(RegistryView.Registry32, enable));
            }
            else
            {
                RegisterProgram(RegistryView.Default, enable);
            }
        }

        /// <summary>
        /// Register or unregister this program as a JIT debugger on Windows.
        /// </summary>
        /// <param name="registryView">
        /// Select which Windows Registry to manipulate: 32-bit or 64-bit. 
        /// </param>
        /// <param name="enable">
        /// Whether to register or unregister this program.
        /// </param>
        private static void RegisterProgram(RegistryView registryView, bool enable)
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView);
            using var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AeDebug", true);
            
            if (enable)
            {
                string exePath = ExecutablePath;
                object? oldValue = key.GetValue("Debugger");
                var newValue = $@"""{exePath}"" -p %ld -e %ld";
                if (newValue.Equals(oldValue))
                    return;
                if (oldValue != null)
                    key.SetValue("DebuggerBackup", oldValue);
                key.SetValue("Debugger", newValue);
            }
            else
            {
                object? oldValue = key.GetValue("DebuggerBackup");
                if (oldValue != null)
                {
                    key.SetValue("Debugger", oldValue);
                    key.DeleteValue("DebuggerBackup");
                }
                else
                {
                    key.DeleteValue("Debugger");
                }
            }
        }

        #endregion

        private static bool ParseArguments(string[] argv, 
                                           out int processId,
                                           out SafeWaitHandle? eventHandle)
        {
            eventHandle = null;
            processId = 0;

            if (argv.Length == 1 && argv[0] == "-r")
            {
                RegisterProgram(true);
                return true;
            }

            if (argv.Length == 1 && argv[0] == "-u")
            {
                RegisterProgram(false);
                return true;
            }

            if (argv.Length < 2 || argv[0] != "-p")
            {
                MessageBox.Show("Invalid arguments passed to debugger attacher. ",
                                "Attach Debugger", 
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            if (!int.TryParse(argv[1], out processId) || processId <= 0)
            {
                MessageBox.Show("Invalid process ID passed to debugger attacher. ",
                                "Attach Debugger", 
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            if (argv.Length >= 4 && argv[2] == "-e")
            {
                if (!int.TryParse(argv[3], out var eventHandleInt) || eventHandleInt <= 0)
                {
                    MessageBox.Show("Invalid event handle passed to debugger attacher. ",
                                    "Attach Debugger", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                eventHandle = new SafeWaitHandle((IntPtr)eventHandleInt, true);
            }

            return true;
        }
    }
}
