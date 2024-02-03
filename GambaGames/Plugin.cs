using System;
using System.Drawing;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Style;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using GambaGames.Windows;
using ECommons;
using Lumina.Data.Parsing.Layer;

namespace GambaGames
{
    public sealed class Plugin : IDalamudPlugin
    {
        
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IPartyList PartyList { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;

        public string Name => "GambaGames";
        private const string CommandName = "/gambagames";
        
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("GambaGames");
        
        private MainWindow MainWindow { get; init; }

        public Plugin(IClientState clientState)
        {
            try
            {
                ECommonsMain.Init(PluginInterface, this);

                this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                this.Configuration.Initialize(PluginInterface);
            
                var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "gamba-icon.png");
                var logoImage = PluginInterface.UiBuilder.LoadImage(imagePath);
            
                MainWindow = new MainWindow(logoImage, PartyList, clientState);
            
                WindowSystem.AddWindow(MainWindow);

                CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
                {
                    HelpMessage = "/GambaGames: Open the gambling games window"
                });

                PluginInterface.UiBuilder.Draw += DrawUI;
            }
            catch (Exception e)
            {
                Chat.Print($"Critical Error, Screenshot this and send it to Miles\n{e}\nIf this is spamming shut off the plugin!", "GambaGames");
            }
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }
    }
}
