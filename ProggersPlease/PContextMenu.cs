using System;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Network.Structures.InfoProxy;
using Dalamud.Plugin.Services;

namespace ProggersPlease;

public class PContextMenu : IDisposable
{

    private Plugin Plugin;
    private readonly IContextMenu _contextMenu;
    private readonly MenuItem _menuItem;

    private CharacterData? _character;

    public PContextMenu(Plugin plugin, IContextMenu contextMenu)
    {
        Plugin = plugin;
        _contextMenu = contextMenu;
        
        _menuItem = new MenuItem
        {
            IsEnabled = true,
            Name = "Tomestone",
            PrefixChar = 'P',
            IsReturn = false,
            IsSubmenu = false,
            OnClicked = OnClick,
            PrefixColor = 33
        };
    }

    private async void OnClick(IMenuItemClickedArgs args) { 
        if (_character is { } && LodestoneClientSingleton.GetClient() is { }) {
            // get lodestone character id
            var charName = _character.Name.ToString();
            var charWorld = _character.HomeWorld.ValueNullable?.Name.ExtractText() ?? "Unknown";

            await Plugin.OpenTomestone(charName, charWorld);
        }
    }


    public void Dispose() => Disable();

    public void Enable() => _contextMenu.OnMenuOpened += OnContextMenuOpened;
    public void Disable() => _contextMenu.OnMenuOpened -= OnContextMenuOpened;

    private void OnContextMenuOpened(IMenuOpenedArgs args) { 
        // theoretically, we dont care about the AddonName, only if the target is a PC
        // if (args.AddonName != "PartyMemberList") {
        //     return;
        // }
        if (args.Target is MenuTargetDefault menuTarget && menuTarget.TargetCharacter is { } character)
        {
            _character = character;

            // make sure lodestone client is ready
            if (LodestoneClientSingleton.GetClient() != null) {
                args.AddMenuItem(_menuItem);
            }
        }

    }
}
