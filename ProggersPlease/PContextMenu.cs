using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Network.Structures.InfoProxy;
using Dalamud.Plugin.Services;

namespace SamplePlugin;

public class PContextMenu : IDisposable
{
    private readonly IContextMenu _contextMenu;
    private readonly MenuItem _menuItem;
    private readonly IChatGui _chatGui;

    private CharacterData _character;

    public PContextMenu(IContextMenu contextMenu, IChatGui chatGui)
    {
        _contextMenu = contextMenu;
        _chatGui = chatGui;
        
        _menuItem = new MenuItem
        {
            IsEnabled = true,
            Name = "Show Prog",
            PrefixChar = 'P',
            IsReturn = false,
            IsSubmenu = false,
            OnClicked = OnClick,
            PrefixColor = 33
        };
    }

    private void OnClick(IMenuItemClickedArgs args) { 
        _chatGui.Print("Hello, stupid world!");
    }

    public void Dispose() => Disable();

    public void Enable() => _contextMenu.OnMenuOpened += OnContextMenuOpened;
    public void Disable() => _contextMenu.OnMenuOpened -= OnContextMenuOpened;

    private void OnContextMenuOpened(IMenuOpenedArgs args) { 
        _chatGui.Print(args.AddonName?.ToString() ?? "no addon name");
        if (args.AddonName != "PartyMemberList") {
            return;
        }
        _chatGui.Print(args.Target?.ToString() ?? "no target");
        if (args.Target is MenuTargetDefault menuTarget && menuTarget.TargetCharacter is { } character)
        {
            _chatGui.Print($"Clicked on {character.Name} - {character.HomeWorld.ValueNullable?.Name.ExtractText()}");

            _character = character;

            _chatGui.Print($"{character.ContentId}");

        }

        args.AddMenuItem(_menuItem);
    }
}
