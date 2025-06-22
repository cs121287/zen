using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class MediumRocks : ZenElement
    {
        public override char Symbol => '@';
        public override string Name => "Medium Rocks";
        public override string Meaning => "Natural rock formations, endurance";
        // Medium gray stone color from authentic Japanese gardens
        public override Color Color => Color.FromArgb(105, 105, 105);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.Terrain;
        public override ElementCategory Category => ElementCategory.Terrain;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: No medium rocks in gravel garden zones
            if (zone.Type == ZoneType.GravelGarden) return false;

            // Rule 2: Must maintain minimum distance from garden edges
            int height = currentGarden.GetLength(0);
            int width = currentGarden.GetLength(1);
            if (row < 3 || row >= height - 3 || col < 3 || col >= width - 3) return false;

            // Rule 3: Maximum 8 medium rocks per garden
            if (context.GetPlacementCount(GetType()) >= 8) return false;

            // Rule 4: Minimum 4 units distance from other medium rocks
            if (context.IsNearElement(row, col, GetType(), 4)) return false;

            // Rule 5: Can be near large rocks but not too close (minimum 3 units)
            if (context.IsNearElement(row, col, typeof(LargeRocks), 2)) return false;

            // Rule 6: Must have some clear space around
            return HasSufficientClearSpace(row, col, currentGarden, 2);
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.03;

            // Prefer focal points and edges
            probability *= zone.Type switch
            {
                ZoneType.FocalPoint => 3.0,
                ZoneType.Edge => 2.5,
                ZoneType.Corner => 2.0,
                _ => 0.7
            };

            // Bonus if near large rocks (but not too close)
            double distanceToLargeRocks = context.GetDistanceToNearestElement(row, col, typeof(LargeRocks));
            if (distanceToLargeRocks > 3 && distanceToLargeRocks < 8)
            {
                probability *= 1.5;
            }

            return probability * zone.GetDistanceInfluence(row, col);
        }
    }
}