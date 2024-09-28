using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RumiTool.Service;
using RumiTool.Windows;

namespace RumiTool;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    // Command List

    private const string StickyNoteCommandName = "/rtsticky";
    private const string StickyNoteListCommandName = "/rtstickies";

    // Config

    public Configuration Configuration { get; init; }

    // Windows

    public readonly WindowSystem WindowSystem = new("RumiTool");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private StickyNoteListWindow StickyNoteListWindow { get; init; }
    private Dictionary<string, StickyNoteWindow> StickyNotes { get; } = new();

    // Services

    private readonly IStickyNoteService stickyNoteService = new StickyNoteService();

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        //var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, "");
        StickyNoteListWindow = new StickyNoteListWindow(stickyNoteService, this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(StickyNoteListWindow);

        CommandManager.AddHandler(StickyNoteCommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle a sticky note window, /rtsticky or /rtsticky <name> for a specific note"
        });

        CommandManager.AddHandler(StickyNoteListCommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "List all sticky notes, /rtstickies"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        //PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        //PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        StickyNoteListWindow.Dispose();

        foreach (var (_, window) in StickyNotes)
        {
            window.Dispose();
        }

        StickyNotes.Clear();

        CommandManager.RemoveHandler(StickyNoteCommandName);
        CommandManager.RemoveHandler(StickyNoteListCommandName);
    }

    private void OnCommand(string command, string args)
    {
        switch (command)
        {
            case StickyNoteCommandName:
                if (string.IsNullOrEmpty(args))
                {
                    args = "default";
                }
                OpenStickyNoteWindow(StickyNoteService.SanitiseName(args));
                break;
            case StickyNoteListCommandName:
                StickyNoteListWindow.Toggle();
                StickyNoteListWindow.Refresh();
                break;
        }
    }

    public void OpenStickyNoteWindow(string name)
    {
        if (StickyNotes.TryGetValue(name, out var window))
        {
            window.Toggle();
        }
        else
        {
            window = new StickyNoteWindow(name, stickyNoteService);
            StickyNotes[name] = window;
            WindowSystem.AddWindow(window);
            window.Toggle();
        }
    }
    
    public void CloseStickyNoteWindow(string name)
    {
        if (StickyNotes.TryGetValue(name, out var window))
        {
            WindowSystem.RemoveWindow(window);
            window.Dispose();
            StickyNotes.Remove(name);
        }
    }

    public void RefreshStickyNoteWindow(string name)
    {
        if (StickyNotes.TryGetValue(name, out var window))
        {
            window.Refresh();
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
