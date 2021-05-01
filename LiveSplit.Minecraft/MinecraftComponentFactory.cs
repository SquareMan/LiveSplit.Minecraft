using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;

namespace LiveSplit.Minecraft
{
    class MinecraftComponentFactory : IComponentFactory
    {
        public string ComponentName => "Minecraft IGT";

        public string Description => "Minecraft IGT originally by Kohru (unmaintained)";

        public ComponentCategory Category => ComponentCategory.Timer;

        public string UpdateName => ComponentName;

        public string XMLURL => "https://raw.githubusercontent.com/LiveSplit/LiveSplit.Minecraft/master/" + "Updates.xml";

        public string UpdateURL => "https://raw.githubusercontent.com/LiveSplit/LiveSplit.Minecraft/master/";

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IComponent Create(LiveSplitState state) => new MinecraftComponent(state);
    }
}
