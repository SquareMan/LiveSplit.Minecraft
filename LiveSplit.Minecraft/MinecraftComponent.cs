﻿// fNbt built from https://github.com/flori-schwa/fNbt since that includes support for TAG_Long_Array
using fNbt;
using LiveSplit.Model;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Minecraft
{
    public class MinecraftComponent : UI.Components.IComponent
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);
        [DllImport("user32.dll")]
        private static extern int UnhookWinEvent(IntPtr hWinEventHook);
        private delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);
        
        private IntPtr focusHook;
        private WinEventProc focusHookCallback;

        private void EventFunc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild,
            int dwEventThread, int dwmsEventTime)
        {
            // Get minecraft processes
            var javaProcesses = Process.GetProcessesByName("javaw");
            var minecraftProcesses = javaProcesses.Where(process => process.MainWindowTitle.Contains("Minecraft"));

            // See if focused window is a minecraft instance
            // TODO: store previous pid or something
            var activeMinecraft = minecraftProcesses.FirstOrDefault(proc => proc.MainWindowHandle == hWnd);
            if (activeMinecraft == null || hWnd == latestMinecraftWindow)
            {
                return;
            }

            latestMinecraftWindow = hWnd;

            // Find game directory
            var query = $"select CommandLine from Win32_Process where ProcessId='{activeMinecraft.Id}'";
            var result = new ManagementObjectSearcher(query).Get().Cast<ManagementBaseObject>().First();
            if (result == null)
            {
                // This really shouldn't happen
                return;
            }
            
            var cmd = result["commandLine"].ToString();
            // gameDir will be surrounded with quotes if the path contains a space
            var match = Regex.Match(cmd ?? string.Empty, @"--gameDir (?:""(.+?)""|([^\s]+))");
            if (!match.Success)
            {
                // Failed to determine game directory of focused minecraft
                return;
            }

            var gameDir = match.Groups.Cast<Group>().Skip(1).FirstOrDefault(group => group.Value != string.Empty)?.Value;
            if (gameDir == null)
            {
                // Failed to extract directory from regex
                return;
            }

            // Update saves path
            Properties.Settings.Default.SavesPath = gameDir + "\\saves";
            
            //Cleanup process list
            foreach (var proc in javaProcesses)
            {
                proc.Dispose();
            }
            
            timer.Reset();
            timer.Start();
        }
        
        private readonly TimerModel timer;
        private readonly MinecraftSettings settings;

        // Limit the rate at which some operations are done since they are too expensive to run on every udpate()
        private const int AUTOSPLITTER_CHECK_DELAY = 500;
        private DateTime nextAutosplitterCheck;
        private const int IGT_CHECK_DELAY = 1000;
        private DateTime nextIGTCheck;

        private IntPtr latestMinecraftWindow = IntPtr.Zero;
        private string latestSavePath;
        private string latestSaveStatsPath;
        private long worldTime = -1;

        public MinecraftComponent(LiveSplitState state)
        {
            //Set a Windows Event Hook to fire when a window is focused
            const uint WINEVENT_OUTOFCONTEXT = 0;
            const uint EVENT_SYSTEM_FOREGROUND = 3;
            focusHookCallback = EventFunc;
            focusHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, focusHookCallback, 0, 0, WINEVENT_OUTOFCONTEXT);
            
            settings = new MinecraftSettings(this, state);

            timer = new TimerModel() { CurrentState = state };
            state.OnStart += OnStart;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            state.IsGameTimePaused = true;

            if (ShouldCheckIGT())
            {
                UpdateIGT();
            }

            if (Properties.Settings.Default.AutosplitterEnabled && ShouldCheckAutosplitter())
            {
                UpdateAutosplitter();
            }
        }

        private bool ShouldCheckIGT()
        {
            if (nextIGTCheck != null && DateTime.Now < nextIGTCheck)
            {
                // Not yet
                return false;
            }
            else
            {
                // Haven't attempted yet or it's time to do so
                nextIGTCheck = DateTime.Now.AddMilliseconds(IGT_CHECK_DELAY);
                return true;
            }
        }

        private void UpdateIGT()
        {
            // If the timer is not running yet or if the stats folder doesn't exist (still on world creation) skip
            if (timer.CurrentState.CurrentPhase == TimerPhase.NotRunning || !Directory.Exists(latestSaveStatsPath)) return;

            // Update IGT, it uses the stats.json file since level.dat is considered inaccurate
            var igt = TimeSpan.FromSeconds(ExtractTicks() / 20.0);
            if (timer.CurrentState.CurrentPhase == TimerPhase.Running || timer.CurrentState.CurrentPhase == TimerPhase.Paused)
            {
                // Run in process, update time normally
                timer.CurrentState.SetGameTime(igt);
            }
            else if (timer.CurrentState.CurrentPhase == TimerPhase.Ended)
            {
                // Run has ended and IGT has changed, update time with ugly hack
                timer.CurrentState.CurrentSplitIndex--;
                var newSplitTime = new Time
                {
                    RealTime = timer.CurrentState.CurrentSplit.SplitTime.RealTime,
                    GameTime = igt
                };
                timer.CurrentState.CurrentSplit.SplitTime = newSplitTime;
                timer.CurrentState.CurrentSplitIndex++;
                timer.CurrentState.Run.HasChanged = true;
            }
        }

        private bool ShouldCheckAutosplitter()
        {
            if (nextAutosplitterCheck != null && DateTime.Now < nextAutosplitterCheck)
            {
                // Not yet
                return false;
            }
            else
            {
                // Haven't attempted yet or it's time to do so
                nextAutosplitterCheck = DateTime.Now.AddMilliseconds(AUTOSPLITTER_CHECK_DELAY);
                return true;
            }
        }

        private void UpdateAutosplitter()
        {
            var previousLatestSavePath = latestSavePath;
            latestSavePath = FindLatestSavePath();
            var levelDat = new NbtFile(Path.Combine(latestSavePath, "level.dat"));

            if (levelDat.RootTag.First()["DataVersion"].IntValue < 1122)
            {
                timer.Reset();

                Properties.Settings.Default.AutosplitterEnabled = false;
                Properties.Settings.Default.Save();

                MessageBox.Show("Autosplitting is not supported for versions under 1.12 and has been disabled",
                    ComponentName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var previousWorldTime = worldTime;
            worldTime = levelDat.RootTag.First()["Time"].LongValue;

            // We don't have a previous time to compare with yet, skip
            if (previousWorldTime == -1) return;

            if (timer.CurrentState.CurrentPhase != TimerPhase.NotRunning && worldTime == 0)
            {
                timer.Reset();
            }

            if (timer.CurrentState.CurrentPhase == TimerPhase.NotRunning && worldTime > 0 && previousWorldTime != worldTime && latestSavePath == previousLatestSavePath)
            {
                timer.Start();
            }

            if (timer.CurrentState.CurrentPhase == TimerPhase.Running && previousWorldTime != worldTime && latestSavePath == previousLatestSavePath
                && levelDat.RootTag.First()["Player"]["seenCredits"].ByteValue == 1)
            {
                timer.Split();
            }
        }

        private string FindLatestSavePath()
        {
            try
            {
                return new DirectoryInfo(Properties.Settings.Default.SavesPath)
                    .GetDirectories()
                    .OrderByDescending(x => x.LastWriteTime)
                    .First().FullName;
            }
            catch
            {
                timer.Reset();
                MessageBox.Show("Couldn't find the Minecraft world save.\n\n" +
                    "Check that your saves folder location is correct on the settings page and that there is at least one save on the folder.",
                    ComponentName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        private int ExtractTicks()
        {
            var statsFile = Directory.EnumerateFiles(latestSaveStatsPath, "*.json").FirstOrDefault();
            var statsText = File.ReadAllLines(statsFile)[0];
            var statStart = statsText.IndexOf("play_time\":") + 11; //1.17+
            if (statStart == 10) //string not found
            {
                statStart = statsText.IndexOf("inute\":") + 7; //1.16-
            }

            var statEnd = statsText.IndexOf(",", statStart);

            return int.Parse(statsText.Substring(statStart, statEnd - statStart));
        }

        private void OnStart(object sender, EventArgs e)
        {
            latestSaveStatsPath = Path.Combine(FindLatestSavePath(), "stats");
        }

        public void Dispose()
        {
            // Remove hook
            UnhookWinEvent(focusHook);
        }

        public Control GetSettingsControl(LayoutMode mode) => settings;

        // Unused since the settings are stored as .NET user settings
        public XmlNode GetSettings(XmlDocument document) => document.CreateElement("Settings");
        // Unused since the settings are stored as .NET user settings
        public void SetSettings(XmlNode settings) { }

        public string ComponentName => "Minecraft IGT";

        public IDictionary<string, Action> ContextMenuControls { get; }

        // We take up no space visually, so we return nothing/zero for visual calls from LiveSplit
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
        public float HorizontalWidth { get { return 0; } }
        public float MinimumWidth { get { return 0; } }
        public float VerticalHeight { get { return 0; } }
        public float MinimumHeight { get { return 0; } }
        public float PaddingBottom { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }
        public float PaddingTop { get { return 0; } }

    }
}
