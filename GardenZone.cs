using System;

namespace ZenGardenGenerator
{
    /// <summary>
    /// Enhanced zone system with proper Karesansui principles
    /// </summary>
    public class GardenZone
    {
        public ZoneType Type { get; set; }
        public int StartRow { get; set; }
        public int StartCol { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int CenterRow => StartRow + Height / 2;
        public int CenterCol => StartCol + Width / 2;
        public double Influence { get; set; } = 1.0;
        
        public GardenZone(ZoneType type, int startRow, int startCol, int width, int height, double influence = 1.0)
        {
            Type = type;
            StartRow = startRow;
            StartCol = startCol;
            Width = width;
            Height = height;
            Influence = influence;
        }
        
        public bool Contains(int row, int col)
        {
            return row >= StartRow && row < StartRow + Height &&
                   col >= StartCol && col < StartCol + Width;
        }
        
        public double GetDistanceInfluence(int row, int col)
        {
            double distance = Math.Sqrt(Math.Pow(row - CenterRow, 2) + Math.Pow(col - CenterCol, 2));
            double maxDistance = Math.Max(Width, Height) / 2.0;
            return Math.Max(0.1, 1.0 - (distance / maxDistance));
        }

        public bool IsNearEdge(int row, int col, int buffer)
        {
            return row < StartRow + buffer || row >= StartRow + Height - buffer ||
                   col < StartCol + buffer || col >= StartCol + Width - buffer;
        }
    }
    
    /// <summary>
    /// Enhanced zone types for authentic Japanese gardens
    /// </summary>
    public enum ZoneType
    {
        FocalPoint,    // Areas meant to draw attention (rocks, lanterns)
        Center,        // Open central areas (water features, gravel)
        Edge,          // Border areas (moss, stones)
        Corner,        // Corner areas (rocks, moss)
        Flow,          // Areas for movement patterns (paths, raked)
        GravelGarden   // Pure gravel areas (no other elements allowed)
    }
}