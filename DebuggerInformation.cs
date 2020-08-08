using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttachDebugger
{
    public class DebuggerInformation
    {
        public string? SolutionFilePath { get; set; }

        public Process? IdeProcess { get; set; }

        public DebuggerKind Kind { get; set; }
    }

    public enum DebuggerKind
    {
        VisualStudio,
        WinDbg
    }
}
