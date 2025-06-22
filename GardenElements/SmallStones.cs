using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class SmallStones : ZenElement
    {
        public override char Symbol => 'o';
        public override string Name => "Small Stones";
        public override string Meaning => "Diverse elements of nature, harmony";
        public override Color Color => Color.FromArgb(169, 169, 169);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.Terrain;
        public override ElementCategory Category => ElementCategory.Terrain;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: No small stones in gravel garden zones
            if (zone.Type == ZoneType.GravelGarden) return false;

            // Rule 2: Must maintain minimum distance from garden edges
            int height = currentGarden.GetLength(0);
            int width = currentGarden.GetLength(1);
            if (row < 2 || row >= height - 2 || col < 2 || col >= width - 2) return false;

            // Rule 3: Maximum 20 small stones per garden
            if (context.GetPlacementCount(GetType()) >= 20) return false;

            // Rule 4: Minimum 2 units distance from other small stones
            if (context.IsNearElement(row, col, GetType(), 2)) return false;

            // Rule 5: Can cluster near larger rocks
            return true;
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.04;

            // Prefer edges and corners, scatter behavior
            switch (zone.Type)
            {
                case ZoneType.Edge:
                    probability *= 2.5;
                    break;
                case ZoneType.Corner:
                    probability *= 2.0;
                    break;
                case ZoneType.FocalPoint:
                    probability *= 1.5;
                    break;
                default:
                    probability *= 1.0;
                    break;
            }

            // Higher probability near larger rocks
            double distanceToLargeRocks = context.GetDistanceToNearestElement(row, col, typeof(LargeRocks));
            double distanceToMediumRocks = context.GetDistanceToNearestElement(row, col, typeof(MediumRocks));
            
            if (distanceToLargeRocks < 6 || distanceToMediumRocks < 4)
            {
                probability *= 1.8;
            }

            return probability * zone.GetDistanceInfluence(row, col);
        }
    }
}