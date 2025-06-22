using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class LargeRocks : ZenElement
    {
        public override char Symbol => '#';
        public override string Name => "Large Rocks";
        public override string Meaning => "Mountains, islands, strength, stability";
        // Authentic Japanese garden stone color - weathered gray stone from images
        public override Color Color => Color.FromArgb(85, 85, 85);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.Terrain;
        public override ElementCategory Category => ElementCategory.Terrain;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: No large rocks in gravel garden zones
            if (zone.Type == ZoneType.GravelGarden) return false;

            // Rule 2: Must maintain minimum distance from garden edges
            int height = currentGarden.GetLength(0);
            int width = currentGarden.GetLength(1);
            if (row < 5 || row >= height - 5 || col < 5 || col >= width - 5) return false;

            // Rule 3: Maximum 3 large rocks per garden
            if (context.GetPlacementCount(GetType()) >= 3) return false;

            // Rule 4: Minimum 8 units distance from other large rocks
            if (context.IsNearElement(row, col, GetType(), 8)) return false;

            // Rule 5: Must have clear space around (no other elements within 4 units)
            return HasSufficientClearSpace(row, col, currentGarden, 4);
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.02;

            // Higher probability in focal points and corners
            probability *= zone.Type switch
            {
                ZoneType.FocalPoint => 4.0,
                ZoneType.Corner => 3.0,
                ZoneType.Edge => 0.5,
                _ => 0.1
            };

            // Reduce probability based on existing count
            int existingCount = context.GetPlacementCount(GetType());
            probability *= Math.Pow(0.3, existingCount);

            return probability * zone.GetDistanceInfluence(row, col);
        }
    }
}