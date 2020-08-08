using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using DTEProcess = EnvDTE.Process;
using Process = System.Diagnostics.Process;

namespace AttachDebugger
{
    // --------------------------------------------------------------------------------------------------------------------
    // Based on code from https://gist.github.com/atruskie/3813175:
    // 
    // Copyright QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
    // --------------------------------------------------------------------------------------------------------------------

    public static class VisualStudioAttacher
    {
        [DllImport("ole32", ExactSpelling = true)]
        private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32", ExactSpelling = true)]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        [DllImport("user32", SetLastError = true, ExactSpelling = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32", ExactSpelling = true)]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        public static string? GetOpenSolutionFilePath(Process visualStudioProcess)
        {
            _DTE? visualStudioInstance;
            if (TryOpenIde(visualStudioProcess.Id, out visualStudioInstance))
            {
                try
                {
                    return visualStudioInstance.Solution.FullName;
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        public static Process? GetAttachedVisualStudio(int applicationProcessId)
        {
            IEnumerable<Process> visualStudios = GetRunningIdeProcesses();

            foreach (Process visualStudio in visualStudios)
            {
                _DTE? visualStudioInstance;
                if (TryOpenIde(visualStudio.Id, out visualStudioInstance))
                {
                    try
                    {
                        foreach (Process debuggedProcess in visualStudioInstance.Debugger.DebuggedProcesses)
                        {
                            if (debuggedProcess.Id == applicationProcessId)
                            {
                                return debuggedProcess;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The method to use to attach visual studio to a specified process.
        /// </summary>
        /// <param name="visualStudioProcess">
        /// The visual studio process to attach to.
        /// </param>
        /// <param name="applicationProcess">
        /// The application process that needs to be debugged.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the application process is null.
        /// </exception>
        public static void AttachToDebuggee(Process visualStudioProcess, int appProcessId)
        {
            _DTE? visualStudioInstance;

            if (TryOpenIde(visualStudioProcess.Id, out visualStudioInstance))
            {
                // Find the process you want the VS instance to attach to...
                DTEProcess? processToAttachTo =
                    visualStudioInstance.Debugger.LocalProcesses.Cast<DTEProcess>()
                                        .FirstOrDefault(process => process.ProcessID == appProcessId);

                // Attach to the process.
                if (processToAttachTo != null)
                {
                    processToAttachTo.Attach();

                    ShowWindow((int)visualStudioProcess.MainWindowHandle, 3);
                    SetForegroundWindow(visualStudioProcess.MainWindowHandle);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Visual Studio process cannot find specified application with process ID {appProcessId}. ");
                }
            }
        }

        /// <summary>
        /// The get visual studio for solutions.
        /// </summary>
        /// <param name="solutionNames">
        /// The solution names.
        /// </param>
        /// <returns>
        /// The <see cref="Process"/>.
        /// </returns>
        public static Process? GetVisualStudioForSolutions(List<string> solutionNames)
        {
            foreach (string solution in solutionNames)
            {
                Process? visualStudioForSolution = GetVisualStudioForSolution(solution);
                if (visualStudioForSolution != null)
                {
                    return visualStudioForSolution;
                }
            }

            return null;
        }

        /// <summary>
        /// The get visual studio process that is running and has the specified solution loaded.
        /// </summary>
        /// <param name="solutionName">
        /// The solution name to look for.
        /// </param>
        /// <returns>
        /// The visual studio <see cref="Process"/> with the specified solution name.
        /// </returns>
        public static Process? GetVisualStudioForSolution(string solutionName)
        {
            IEnumerable<Process> visualStudios = GetRunningIdeProcesses();

            foreach (Process visualStudio in visualStudios)
            {
                _DTE? visualStudioInstance;
                if (TryOpenIde(visualStudio.Id, out visualStudioInstance))
                {
                    try
                    {
                        string actualSolutionName = Path.GetFileName(visualStudioInstance.Solution.FullName);

                        if (string.Compare(
                            actualSolutionName,
                            solutionName,
                            StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            return visualStudio;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<Process> GetRunningIdeProcesses()
        {
            Process[] processes = Process.GetProcesses();

            bool matchesVisualStudioFileName(string? path)
            {
                if (path == null)
                    return false;

                return "devenv.exe".Equals(Path.GetFileName(path), StringComparison.OrdinalIgnoreCase);
            }

            return processes.Where(p => p.ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase) &&
                                        matchesVisualStudioFileName(p.MainModule?.FileName));
        }

        public static List<string> QueryVsWhereProperty(string requires, string property)
        {
            var vsWherePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                                        "Microsoft Visual Studio",
                                        "Installer",
                                        "vswhere.exe");

            var process = new Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = vsWherePath,
                Arguments = $"-sort -requires {requires} -property {property}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.Start();
            string? line;
            var lines = new List<string>();
            while ((line = process.StandardOutput.ReadLine()) != null)
                lines.Add(line);

            process.WaitForExit();
            return lines;
        }

        private static bool TryOpenIde(int processId, [NotNullWhen(true)] out _DTE? instance)
        {
            IntPtr numFetched = IntPtr.Zero;
            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            IMoniker[] monikers = new IMoniker[1];

            GetRunningObjectTable(0, out runningObjectTable);
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                object runningObjectVal;
                runningObjectTable.GetObject(monikers[0], out runningObjectVal);

                if (runningObjectVal is _DTE && runningObjectName.StartsWith("!VisualStudio"))
                {
                    int currentProcessId = int.Parse(runningObjectName.Split(':')[1]);

                    if (currentProcessId == processId)
                    {
                        instance = (_DTE)runningObjectVal;
                        return true;
                    }
                }
            }

            instance = null;
            return false;
        }
    }
}
