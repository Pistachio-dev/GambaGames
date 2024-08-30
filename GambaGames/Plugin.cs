using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using GambaGames.Windows;
using ECommons;

namespace GambaGames
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] internal ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPartyList PartyList { get; private set; } = null!;
        [PluginService] internal static IChatGui Chat { get; private set; } = null!;
        [PluginService] internal static IClientState Client { get; private set; } = null!;
        
        private const string CommandName = "/gambagames";
        
        public WindowSystem WindowSystem = new("GambaGames");
        public Configuration Configuration { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin()
        {
            try
            {
                ECommonsMain.Init(PluginInterface, this);
                    
                Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                Configuration.Initialize(PluginInterface);
                
                Chat.Print(PluginInterface.AssemblyLocation.Directory?.FullName!);
            
                var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "gambaicon.png");
                
                MainWindow = new MainWindow(this, imagePath);
            
                WindowSystem.AddWindow(MainWindow);

                CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
                {
                    HelpMessage = "/GambaGames: Open the gambling games window"
                });

                PluginInterface.UiBuilder.Draw += DrawUi;
                
                PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
            }
            catch (Exception e)
            {
                Chat.Print($"Critical Error, {e.Message}!", "GambaGames");
            }
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            try
            {
                ToggleMainUi();
            }
            catch (Exception e)
            {
                Chat.Print($"Critical Error, {e.Message}!", "GambaGames");
            }
        }

        private void DrawUi() => WindowSystem.Draw();
        public void ToggleMainUi() => MainWindow.Toggle();
    }
}
