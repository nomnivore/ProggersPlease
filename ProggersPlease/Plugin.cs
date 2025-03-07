using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ProggersPlease.Windows;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ProggersPlease;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/proggers";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Proggers, Please!");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private PContextMenu PContextMenu { get; init; }

    private DalamudLinkPayload WebLinkPayload { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // initialize lodestone client
        LodestoneClientProvider.Initialize();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        // TODO: add commands (if needed)

        // CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        // {
        //     HelpMessage = "A useful message to display in /xlhelp"
        // });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // TODO: make UIs

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        WebLinkPayload = PluginInterface.AddChatLinkHandler(333, (x, z) => Utils.OpenUrl(z.ToString()));
        // adds a context menu item to the context menu for players
        PContextMenu = new PContextMenu(ContextMenu, ChatGui, WebLinkPayload);
        PContextMenu.Enable();
    }


    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        // CommandManager.RemoveHandler(CommandName);

        PContextMenu.Dispose();
        PluginInterface.RemoveChatLinkHandler(333);

        // dispose lodestone client
        LodestoneClientProvider.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
