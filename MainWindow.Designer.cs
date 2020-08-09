namespace AttachDebugger
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.acceptButton = new System.Windows.Forms.Button();
            this.debuggersListBox = new System.Windows.Forms.ListBox();
            this.debuggersLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.acceptButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.debuggersListBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.debuggersLabel, 0, 0);
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(904, 543);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // acceptButton
            // 
            this.acceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.acceptButton.AutoSize = true;
            this.acceptButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.acceptButton.Location = new System.Drawing.Point(728, 12);
            this.acceptButton.Margin = new System.Windows.Forms.Padding(18, 12, 18, 12);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.acceptButton.Size = new System.Drawing.Size(158, 43);
            this.acceptButton.TabIndex = 0;
            this.acceptButton.Text = "Attach Debugger";
            this.acceptButton.UseVisualStyleBackColor = true;
            this.acceptButton.Click += new System.EventHandler(this.acceptButton_Click);
            // 
            // debuggersListBox
            // 
            this.debuggersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.debuggersListBox, 2);
            this.debuggersListBox.FormattingEnabled = true;
            this.debuggersListBox.ItemHeight = 25;
            this.debuggersListBox.Location = new System.Drawing.Point(3, 70);
            this.debuggersListBox.Name = "debuggersListBox";
            this.debuggersListBox.Size = new System.Drawing.Size(898, 479);
            this.debuggersListBox.TabIndex = 1;
            this.debuggersListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.debuggersListBox_MouseDoubleClick);
            // 
            // debuggersLabel
            // 
            this.debuggersLabel.AutoSize = true;
            this.debuggersLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debuggersLabel.Location = new System.Drawing.Point(3, 0);
            this.debuggersLabel.Name = "debuggersLabel";
            this.debuggersLabel.Size = new System.Drawing.Size(268, 67);
            this.debuggersLabel.TabIndex = 2;
            this.debuggersLabel.Text = "Visual Studio instances available:";
            this.debuggersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainWindow
            // 
            this.AcceptButton = this.acceptButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(904, 543);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(480, 320);
            this.Name = "MainWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Attach Debugger";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.ListBox debuggersListBox;
        private System.Windows.Forms.Label debuggersLabel;
    }
}

