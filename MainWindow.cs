using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AttachDebugger
{
    public partial class MainWindow : Form
    {
        private List<DebuggerInformation> _debuggerInfo;

        public DebuggerInformation? AcceptedSelection { get; private set; }

        public MainWindow()
        {
            _debuggerInfo = new List<DebuggerInformation>();
            InitializeComponent();
        }

        public MainWindow(string title, List<DebuggerInformation> debuggerInfo)
        {
            _debuggerInfo = debuggerInfo;

            InitializeComponent();

            Text = title;

            foreach (var item in debuggerInfo)
            {
                string description;

                if (item.IdeProcess != null && item.SolutionFilePath != null)
                    description = item.SolutionFilePath;
                else if (item.IdeProcess != null)
                    description = "No solution open: process ID {item.IdeProcess.Id}";
                else
                    description = item.Kind switch
                    {
                        DebuggerKind.VisualStudio => "(New Visual Studio instance)",
                        DebuggerKind.WinDbg => "(New WinDbg instance)",
                        _ => throw new NotImplementedException()
                    };

                debuggersListBox.Items.Add(description);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (debuggersListBox.SelectedIndex >= 0)
                AcceptedSelection = _debuggerInfo[debuggersListBox.SelectedIndex];

            Close();
        }

        private void debuggersListBox_DoubleClick(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }
    }
}
