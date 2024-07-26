using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;
using System.Collections.Generic;

namespace WhereTheCirclesAt;

public class WhereTheCirclesAtSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);
    public ToggleNode DisableDrawRegionLimiting { get; set; } = new ToggleNode(false);
    public ToggleNode DisableDrawOnLeftOrRightPanelsOpen { get; set; } = new ToggleNode(false);
    public ToggleNode IgnoreFullscreenPanels { get; set; } = new ToggleNode(false);
    public ToggleNode IgnoreLargePanels { get; set; } = new ToggleNode(false);

    public List<CircleData> Circles = [];

    public class CircleData
    {
        public string Name;
        public float Size;
        public int Thickness;
        public int Segments;
        public bool LargeMap;
        public bool EnableLargeMapColor;
        public Color LargeMapColor;
        public bool World;
        public Color GameWorldColor;

        public CircleData()
        {
            Name = "Rename Me";
            Size = 100;
            Thickness = 7;
            Segments = 40;
            LargeMap = false;
            EnableLargeMapColor = false;
            LargeMapColor = Color.White;
            World = true;
            GameWorldColor = Color.White;
        }
    }
}