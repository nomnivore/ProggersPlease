using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Network.Structures.InfoProxy;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using NetStone.Search.Character;

namespace ProggersPlease;

public class PContextMenu : IDisposable
{
    private readonly IContextMenu _contextMenu;
    private readonly MenuItem _menuItem;
    private readonly IChatGui _chatGui;

    private CharacterData? _character;
    private readonly DalamudLinkPayload _webLinkPayload;

    public PContextMenu(IContextMenu contextMenu, IChatGui chatGui, DalamudLinkPayload webLinkPayload)
    {
        _contextMenu = contextMenu;
        _chatGui = chatGui;
        _webLinkPayload = webLinkPayload;
        
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
        if (_character is { } && LodestoneClientProvider.GetClient() is { } client) {
            // get lodestone character id
            var charName = _character.Name.ToString();
            var charWorld = _character.HomeWorld.ValueNullable?.Name.ExtractText() ?? "Unknown";

            // TODO: cache character id results
            var response = await client.SearchCharacter(new CharacterSearchQuery() {
                CharacterName = charName,
                World = charWorld
            });

            var lodestoneResult = response?.Results.Where(c => c.Name == charName).FirstOrDefault();
            if (lodestoneResult is { }) {

                // format: https://tomestone.gg/character/{id}/{name}
                var sanitizedName = Utils.SanitizeName(charName);
                var link = $"https://tomestone.gg/character/{lodestoneResult.Id}/{sanitizedName}";
                Utils.OpenUrl(link);

                // add link to chat
                var payloads = new List<Payload>
                {
                    new UIForegroundPayload(579),
                    new TextPayload($"{charName}'s Tomestone: \n    "),
                    _webLinkPayload,
                    new TextPayload(link),
                    RawPayload.LinkTerminator,
                    UIForegroundPayload.UIForegroundOff
                };
                var sestringurl = new SeString(payloads);
                _chatGui.Print(sestringurl);
            } else {
                _chatGui.Print($"Character [{charName} - {charWorld}] not found on Lodestone. Unable to open Tomestone.");
            }
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
            if (LodestoneClientProvider.GetClient() != null) {
                args.AddMenuItem(_menuItem);
            }
        }

    }
}
