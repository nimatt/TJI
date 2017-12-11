/*
 * This file is part of TJI.
 * 
 * TJI is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * TJI is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with TJI.  If not, see <http://www.gnu.org/licenses/>.
 */

using log4net;
using System;
using System.Drawing;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "TJI.exe.config", Watch = true)]  
namespace TJI
{
    public partial class MainWindow : Form
    {
        private static readonly Log Logger = Log.GetLogger(typeof(MainWindow));
        private delegate void UpdateStatus(string message, SyncronizerStatus status);

        Syncronizer _syncronizer;
        private static readonly Font HeaderFont = new Font(FontFamily.GenericSansSerif, 12);
        private static readonly Font StandardFont = new Font(FontFamily.GenericSansSerif, 10);

        public MainWindow()
        {
            Logger.Debug("Initializing window");
            InitializeComponent();
            AddContextMenu(output);
            SetStartupText();
            Logger.Debug("Main window initialized");
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            _syncronizer = new Syncronizer();
            _syncronizer.StatusChange += syncronizer_StatusChange;
            startStopButton.Enabled = _syncronizer.Settings.HasSettings;

            FormClosing += MainWindow_FormClosing;
            Log.Logging += LogEvent;

            if (_syncronizer.Settings.HasSettings)
            {
                StartSyncronization();
            }
            else
            {
                ShowNoSettingsInfo();
            }
        }

        private void LogEvent(string message)
        {
            AppendLine(DateTime.Now.ToShortTimeString() + ": " + message);
        }

        void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _syncronizer.Stop();
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
        }

        void syncronizer_StatusChange(string message, SyncronizerStatus status)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateStatus(syncronizer_StatusChange), new object[] {message, status});
                return;
            }

            TrayIcon.BalloonTipText = message;
            TrayIcon.ShowBalloonTip(3000);
            switch (status)
            {
                case SyncronizerStatus.Stopped:
                    SetIcon(Properties.Resources.StandardIcon);
                    break;
                case SyncronizerStatus.Processing:
                    SetIcon(Properties.Resources.RunningIcon);
                    break;
                case SyncronizerStatus.Sleeping:
                    SetIcon(Properties.Resources.RunningIcon);
                    break;
                case SyncronizerStatus.Error:
                    SetIcon(Properties.Resources.ErrorIcon);
                    break;
                default:
                    SetIcon(Properties.Resources.ErrorIcon);
                    break;
            }
        }

        private void SetIcon(Icon newIcon)
        {
            TrayIcon.Icon = newIcon;
            Icon = newIcon;
        }

        private void startStopButton_Click(object sender, EventArgs e)
        {
            if (_syncronizer.IsRunning)
            {
                StopSyncronization();
            }
            else
            {
                StartSyncronization();
            }
        }

        private void StopSyncronization()
        {
            Logger.Info("Stopping synchronization");
            startStopButton.Text = "Start";
            settingsButton.Enabled = true;
            _syncronizer.Stop();
        }

        private void StartSyncronization()
        {
            Logger.Info("Starting synchronization");
            startStopButton.Text = "Stop";
            settingsButton.Enabled = false;
            _syncronizer.Start();
        }

        private void hide_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void settings_Click(object sender, EventArgs e)
        {
            var window = new SettingsWindow(_syncronizer.Settings);
            window.Closed += (o, args) => startStopButton.Enabled = _syncronizer.Settings.HasSettings;
            window.Show();
        }

        private void AppendLine(string text)
        {
            AppendLine(text, StandardFont);
        }

        private void AppendLine(string text, Font font)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action<string, Font>(AppendLine), new object[] {text, font});
                }
                catch (ObjectDisposedException)
                {
                    // This might happen on shutdown  
                }
                return;
            }

            output.Select(output.TextLength, 0);
            output.SelectionFont = font;
            output.SelectedText = text + Environment.NewLine;
            output.ScrollToCaret();
        }

        private void SetStartupText()
        {
            string startupText = "Welcome to TJI"+ Environment.NewLine;
            output.Text = startupText;
            output.Select(0, startupText.Length);
            output.SelectionFont = HeaderFont;
            output.Select(startupText.Length - 3 - Environment.NewLine.Length, 3);
            output.SelectionFont = new Font(HeaderFont, FontStyle.Bold);
            output.Select(startupText.Length, 0);
            output.SelectionFont = StandardFont;
        }

        private void ShowNoSettingsInfo()
        {
            AppendLine("You do not seem to have valid settings at the moment.");
            AppendLine("Please click 'Settings' below.");
        }

        private static void AddContextMenu(RichTextBox rtb)
        {
            if (rtb.ContextMenuStrip != null)
                return;

            ContextMenuStrip cms = new ContextMenuStrip { ShowImageMargin = false };
            ToolStripMenuItem tsmiCopy = new ToolStripMenuItem("Copy");
            tsmiCopy.Click += (sender, e) => rtb.Copy();
            cms.Items.Add(tsmiCopy);
            rtb.ContextMenuStrip = cms;
        }
    }
}

