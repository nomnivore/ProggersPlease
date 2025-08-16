using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace ProggersPlease.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    private readonly IPartyList _partyList;
    private readonly LodestoneStore _lodestoneStore;

    public MainWindow(Plugin plugin, IPartyList partyList, LodestoneStore lodestoneStore)

        : base("Proggers, Please##ProggersMainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)

    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
        _partyList = partyList;
        _lodestoneStore = lodestoneStore;
    }

    public void Dispose() { }

    private void DrawPartyTab() {

            // get the current list of party members
            var partyMembers = _partyList;

            ImGui.BeginTable("##party-list", 3, ImGuiTableFlags.Borders);
            ImGui.TableSetupColumn("Party Member");
            ImGui.TableSetupColumn("World");
            ImGui.TableSetupColumn("Tomestone");
            ImGui.TableHeadersRow();
            foreach (var member in partyMembers)
            {
                var memberRef = _partyList.CreatePartyMemberReference(member.Address);

                var name = member.Name.TextValue;
                var world = memberRef?.World.ValueNullable?.Name.ExtractText() ?? "Unknown";

                ImGui.TableNextColumn();
                ImGui.Text(name);
                ImGui.TableNextColumn();
                ImGui.Text(world);
                ImGui.TableNextColumn();

                // TODO: logic to check if its cached already
                var buttonText = "View";
                if (_lodestoneStore.GetCachedId(name, world) is { }) {
                    buttonText = "View (Cached)";
                }

                if (ImGui.Button($"{buttonText}##view-{member.Address}"))
                {
                    Task.Run(async () => await Plugin.OpenTomestone(name, world));
                }
            }

            ImGui.EndTable();

            ImGui.Spacing();
            
            if (ImGui.Button("Fetch All##fetch-all")) {
                Task.Run(async () => {
                    foreach (var member in partyMembers) {
                        var memberRef = _partyList.CreatePartyMemberReference(member.Address);

                        var name = member.Name.TextValue;
                        var world = memberRef?.World.ValueNullable?.Name.ExtractText() ?? "Unknown";
                        await _lodestoneStore.GetLodestoneId(name, world);
                    }
                });
            }
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Fetches all party members' lodestone IDs and caches them");
            }
    }

    private void DrawRecentTab()
    {
        ImGui.Text("Left Click to open Tomestone\nRight Click to remove from cache");

        ImGui.Spacing();

        var tableHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing();

        ImGui.BeginTable("##recent-list", 1, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY, new Vector2(0, tableHeight));
        foreach (var key in _lodestoneStore.GetCacheKeys())
        {
            var (name, world) = Utils.FromKey(key.ToString() ?? "error_unknown");
            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{name} ({world})")) {
                Task.Run(async () => await Plugin.OpenTomestone(name, world));
            }
            // if right clicked, delete
            if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                _lodestoneStore.RemoveCachedId(key.ToString());
            }
        }

        // entries for testing size only
        // for (var i = 0; i < 100; i++) {
        //     ImGui.TableNextColumn();
        //     ImGui.Text("Test " + i);
        // }
        ImGui.EndTable();

        ImGui.Spacing();

        if (ImGui.Button("Clear Cache##clear-cache")) {
            _lodestoneStore.EmptyCache();
        }

        ImGui.Spacing();
    }

    public override void Draw()
    {

        ImGui.BeginTabBar("##main-tabs");
        if (ImGui.BeginTabItem("Current Party##current-party"))
        {
            DrawPartyTab();

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Recently Viewed##recent"))
        {
            DrawRecentTab();
            ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }
}
