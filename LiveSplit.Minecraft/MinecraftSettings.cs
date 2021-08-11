using LiveSplit.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace LiveSplit.Minecraft
{
    public partial class MinecraftSettings : UserControl
    {
        private readonly MinecraftComponent component;

        // No, I won't learn Windows Forms databinding
        public MinecraftSettings(MinecraftComponent component, LiveSplitState state)
        {
            this.component = component;
            // This (↓) initialize is for the Windows Form, not the MinecraftComponent
            InitializeComponent();

            if (Properties.Settings.Default.FirstLaunch)
            {
                Properties.Settings.Default.FirstLaunch = false;

                // Enable global hotkeys by default
                state.Settings.GlobalHotkeysEnabled = true;

                // Set the saves path to the standard one
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Properties.Settings.Default.SavesPath = Path.Combine(appDataPath, ".minecraft", "saves");

                Properties.Settings.Default.Save();

                // Encourage the user to check the settings page on first launch
                MessageBox.Show($"Minecraft saves folder location has been set to:\n\n" +
                    $"{Properties.Settings.Default.SavesPath}\n\n" +
                    $"It can be changed on the settings page where you will also find other options and instructions. Global hotkeys have been enabled. Good luck in your runs!",
                    this.component.ComponentName, MessageBoxButtons.OK);
            }

            Properties.Settings.Default.PropertyChanged += PropertyChanged;
        }

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            LoadProperties();
        }

        private void MinecraftSettings_Load(object sender, EventArgs e)
        {
            LoadProperties();
            labelVersion.Text = $"Version {Assembly.GetExecutingAssembly().GetName().Version} by Kohru";
        }

        private void LoadProperties()
        {
            txtBoxSavesPath.Text = Properties.Settings.Default.SavesPath;
            checkBoxAutosplitter.Checked = Properties.Settings.Default.AutosplitterEnabled;
        }

        private void BtnChangeSavesPath_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if(result == DialogResult.OK)
            {
                Properties.Settings.Default.SavesPath = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                txtBoxSavesPath.Text = Properties.Settings.Default.SavesPath;
            }
        }


        private void BtnResetSavesPath_Click(object sender, EventArgs e)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Properties.Settings.Default.SavesPath = Path.Combine(appDataPath, ".minecraft", "saves");
            Properties.Settings.Default.Save();

            txtBoxSavesPath.Text = Properties.Settings.Default.SavesPath;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SavesPath = txtBoxSavesPath.Text;

            Properties.Settings.Default.Save();
        }

        private void LinkInstructions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.youtube.com/watch?v=Ij7HDfbv63g");
        }

        private void CheckBoxAutosplitter_CheckedChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AutosplitterEnabled != checkBoxAutosplitter.Checked)
            {
                Properties.Settings.Default.AutosplitterEnabled = checkBoxAutosplitter.Checked;
                Properties.Settings.Default.Save();
            }
        }

    }
}
