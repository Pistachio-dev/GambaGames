using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace GambaGames
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 8;
        public double WinPercent { get; set; } = 200;
        public double UnNatural21Percent { get; set; } = 200;
        public double Natural21Percent { get; set; } = 250;
        
        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;
        
        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }
        
        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
