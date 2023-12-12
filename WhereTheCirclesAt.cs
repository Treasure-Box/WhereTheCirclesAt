using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ImGuiNET;
using SharpDX;
using System.Linq;
using static WhereTheCirclesAt.WhereTheCirclesAtSettings;
using Vector3N = System.Numerics.Vector3;
using Vector4N = System.Numerics.Vector4;

namespace WhereTheCirclesAt;

public class WhereTheCirclesAt : BaseSettingsPlugin<WhereTheCirclesAtSettings>
{
    public Vector3N PlayerPos { get; set; } = new Vector3N();
    public IngameData IngameData { get; set; }

    public override bool Initialise() => true;

    public WhereTheCirclesAt()
    {
        Name = "Where The Circles At";
    }

    public override void DrawSettings()
    {
        base.DrawSettings();

        if (Settings.Circles.Count > 0)
        {
            foreach (var (item, i) in Settings.Circles.Select((x, i) => (x, i)).ToList())
            {
                if (ImGui.CollapsingHeader($@"##{i}", ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    // Start Indent
                    ImGui.Indent();

                    ImGui.InputText($"Name##Name{i}", ref item.Name, 200);
                    ImGui.DragFloat($"Size##Size{i}", ref item.Size);
                    ImGui.DragInt($"Width##Width{i}", ref item.Thickness);
                    ImGui.DragInt($"Segments (high = bad)##segments{i}", ref item.Segments);
                    item.Color = ColorPicker($"Color##Width{i}", item.Color);

                    if (ImGui.Button($"Delete##{i}"))
                    {
                        Settings.Circles.RemoveAt(i);
                    }

                    // End Indent
                    ImGui.Unindent();
                    ImGui.Separator();
                }
            }
        }
        if (ImGui.Button("New"))
        {
            Settings.Circles.Add(new CircleData());
        }
    }

    public static Color ColorPicker(string labelName, Color inputColor)
    {
        var color = inputColor.ToVector4();
        var colorToVect4 = new Vector4N(color.X, color.Y, color.Z, color.W);
        if (ImGui.ColorEdit4(labelName, ref colorToVect4, ImGuiColorEditFlags.AlphaBar)) return new Color(colorToVect4.X, colorToVect4.Y, colorToVect4.Z, colorToVect4.W);
        return inputColor;
    }

    public override Job Tick()
    {
        IngameData = GameController.IngameState.Data ?? null;
        var Player = GameController?.Player ?? null;
        if (Player == null) return null;

        PlayerPos = IngameData.ToWorldWithTerrainHeight(Player.PosNum);
        return null;
    }

    public override void Render()
    {
        if (!Settings.Enable.Value || !GameController.InGame || IngameData == null || PlayerPos == Vector3N.Zero) return;

        var ingameUi = GameController.Game.IngameState.IngameUi;
        if (!Settings.IgnoreFullscreenPanels && ingameUi.FullscreenPanels.Any(x => x.IsVisible))
        {
            return;
        }

        if (!Settings.IgnoreLargePanels && ingameUi.LargePanels.Any(x => x.IsVisible))
        {
            return;
        }

        foreach (var drawing in Settings.Circles)
        {
            DrawData(drawing.Color, drawing.Size, drawing.Thickness, drawing.Segments);
        }

        void DrawData(Color color, float size, int thickness, int segments)
        {
            Graphics.DrawCircleInWorld(PlayerPos, size, color, thickness, segments);
        }
    }
}