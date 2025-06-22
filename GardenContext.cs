using System;
using System.Collections.Generic;
using System.Linq;

namespace ZenGardenGenerator
{
    /// <summary>
    /// Tracks garden state and placement history during generation
    /// </summary>
    public class GardenContext
    {
        private readonly Dictionary<Type, List<(int row, int col)>> placements = [];
        private readonly Random random;
        private readonly List<WaterPath> waterPaths = [];
        private bool bridgePlaced = false;

        public GardenContext(Random random)
        {
            this.random = random;
        }

        public Random Random => random;
        public bool BridgePlaced => bridgePlaced;
        public IReadOnlyList<WaterPath> WaterPaths => waterPaths.AsReadOnly();

        public void RecordPlacement(Type elementType, int row, int col)
        {
            if (!placements.ContainsKey(elementType))
                placements[elementType] = [];
            
            placements[elementType].Add((row, col));

            if (elementType == typeof(GardenElements.BridgePath))
                bridgePlaced = true;
        }

        public void RecordWaterPath(WaterPath path)
        {
            waterPaths.Add(path);
        }

        public List<(int row, int col)> GetPlacements(Type elementType)
        {
            return placements.TryGetValue(elementType, out var list) ? list : [];
        }

        public int GetPlacementCount(Type elementType)
        {
            return GetPlacements(elementType).Count;
        }

        public bool IsNearElement(int row, int col, Type elementType, int distance)
        {
            var elementPlacements = GetPlacements(elementType);
            return elementPlacements.Any(p => 
                Math.Abs(p.row - row) <= distance && Math.Abs(p.col - col) <= distance);
        }

        public double GetDistanceToNearestElement(int row, int col, Type elementType)
        {
            var elementPlacements = GetPlacements(elementType);
            if (!elementPlacements.Any()) return double.MaxValue;

            return elementPlacements.Min(p => 
                Math.Sqrt(Math.Pow(p.row - row, 2) + Math.Pow(p.col - col, 2)));
        }

        public bool WouldIntersectWater(int row, int col, int length, bool isHorizontal)
        {
            foreach (var path in waterPaths)
            {
                for (int i = 0; i < length; i++)
                {
                    int checkRow = isHorizontal ? row : row + i;
                    int checkCol = isHorizontal ? col + i : col;
                    
                    if (path.ContainsPoint(checkRow, checkCol))
                        return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Represents a water path (river or stream)
    /// </summary>
    public class WaterPath
    {
        public List<(int row, int col)> Points { get; } = [];
        public bool IsPond { get; set; } = false;

        public void AddPoint(int row, int col)
        {
            Points.Add((row, col));
        }

        public bool ContainsPoint(int row, int col)
        {
            return Points.Contains((row, col));
        }

        public bool IntersectsWith(WaterPath other)
        {
            return Points.Any(p => other.ContainsPoint(p.row, p.col));
        }
    }
}