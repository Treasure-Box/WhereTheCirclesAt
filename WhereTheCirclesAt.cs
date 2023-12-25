using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using System;
using System.Linq;
using static WhereTheCirclesAt.WhereTheCirclesAtSettings;
using Vector2N = System.Numerics.Vector2;
using Vector3N = System.Numerics.Vector3;
using Vector4N = System.Numerics.Vector4;

namespace WhereTheCirclesAt;

public class WhereTheCirclesAt : BaseSettingsPlugin<WhereTheCirclesAtSettings>
{
    private ImDrawListPtr _backGroundWindowPtr;
    private RectangleF _rect;

    public WhereTheCirclesAt()
    {
        Name = "Where The Circles At";
    }

    public Vector3N PlayerPos { get; set; }
    public IngameData InGameData { get; set; }

    public override bool Initialise() => true;

    public override void DrawSettings()
    {
        base.DrawSettings();

        if (Settings.Circles.Count > 0)
        {
            foreach (var (item, i) in Settings.Circles.Select((x, i) => (x, i)).ToList())
                if (ImGui.CollapsingHeader($@"##{i}", ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Indent();
                    ImGui.InputText($"Name##Name{i}", ref item.Name, 200);
                    ImGui.DragFloat($"Closely Resembles in game distance##Size{i}", ref item.Size);
                    ImGui.DragInt($"Width##Width{i}", ref item.Thickness);
                    ImGui.DragInt($"Segments (high = bad)##segments{i}", ref item.Segments);
                    item.Color = ColorPicker($"Color##Width{i}", item.Color);

                    if (ImGui.Button($"Delete##{i}"))
                    {
                        Settings.Circles.RemoveAt(i);
                    }

                    ImGui.Unindent();
                    ImGui.Separator();
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

        return ImGui.ColorEdit4(labelName, ref colorToVect4, ImGuiColorEditFlags.AlphaBar) ? new Color(
            colorToVect4.X,
            colorToVect4.Y,
            colorToVect4.Z,
            colorToVect4.W
        ) : inputColor;
    }

    public override Job Tick()
    {
        InGameData = GameController.IngameState.Data;
        var Player = GameController?.Player;

        if (Player == null)
        {
            return null;
        }

        if (InGameData != null)
        {
            PlayerPos = InGameData.ToWorldWithTerrainHeight(Player.PosNum);
        }

        return null;
    }

    public override void Render()
    {
        if (!Settings.Enable.Value || !GameController.InGame || InGameData == null || PlayerPos == Vector3N.Zero)
        {
            return;
        }

        var inGameUi = GameController.Game.IngameState.IngameUi;

        if (Settings.DisableDrawOnLeftOrRightPanelsOpen &&
            (inGameUi.OpenLeftPanel.IsVisible || inGameUi.OpenRightPanel.IsVisible))
        {
            return;
        }

        if (!Settings.IgnoreFullscreenPanels && inGameUi.FullscreenPanels.Any(x => x.IsVisible))
        {
            return;
        }

        if (!Settings.IgnoreLargePanels && inGameUi.LargePanels.Any(x => x.IsVisible))
        {
            return;
        }

        _rect = GameController.Window.GetWindowRectangle() with
        {
            Location = Vector2.Zero
        };

        if (!Settings.DisableDrawRegionLimiting)
        {
            if (inGameUi.OpenRightPanel.IsVisible)
            {
                _rect.Right = inGameUi.OpenRightPanel.GetClientRectCache.Left;
            }

            if (inGameUi.OpenLeftPanel.IsVisible)
            {
                _rect.Left = inGameUi.OpenLeftPanel.GetClientRectCache.Right;
            }
        }

        ImGui.SetNextWindowSize(new Vector2N(_rect.Width, _rect.Height));
        ImGui.SetNextWindowPos(new Vector2N(_rect.Left, _rect.Top));

        ImGui.Begin(
            "wherethecirclesat_drawregion",
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoBackground
        );

        _backGroundWindowPtr = ImGui.GetWindowDrawList();

        foreach (var drawing in Settings.Circles)
            DrawData(drawing.Color, drawing.Size, drawing.Thickness, drawing.Segments);

        ImGui.End();
        return;

        void DrawData(Color color, float size, int thickness, int segments)
        {
            DrawCircleInWorld(PlayerPos, size*10f, color, thickness, segments);
        }
    }

    #region Graphics.cs Code

    public void DrawCircleInWorld(Vector3N worldCenter, float radius, Color color, float thickness, int segmentCount)
    {
        var circlePoints = GetInWorldCirclePoints(worldCenter, radius, segmentCount, false);
        DrawPolyLine(circlePoints, color, thickness, ImDrawFlags.Closed);
    }

    public void DrawPolyLine(Vector2N[] points, Color color, float thickness, ImDrawFlags drawFlags)
    {
        _backGroundWindowPtr.AddPolyline(ref points[0], points.Length, color.ToImgui(), drawFlags, thickness);
    }

    private static Vector2N[] GetInWorldCirclePoints(Vector3N worldCenter, float radius, int segmentCount,
        bool addFinalPoint)
    {
        var circlePoints = new Vector2N[segmentCount + (addFinalPoint ? 1 : 0)];
        var segmentAngle = 2f * MathF.PI / segmentCount;

        for (var i = 0; i < segmentCount + (addFinalPoint ? 1 : 0); i++)
        {
            var angle = i * segmentAngle;
            var currentOffset = Vector2N.UnitX.RotateRadians(angle) * radius;
            var currentWorldPos = worldCenter + new Vector3N(currentOffset, 0);
            circlePoints[i] = RemoteMemoryObject.pTheGame.IngameState.Camera.WorldToScreen(currentWorldPos);
        }

        return circlePoints;
    }

    #endregion Graphics.cs Code
}