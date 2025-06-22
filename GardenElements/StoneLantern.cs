using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class StoneLantern : ZenElement
    {
        public override char Symbol => '*';
        public override string Name => "Stone Lantern";
        public override string Meaning => "Enlightenment, peace, meditation";
        // Warm golden lantern glow from traditional Japanese garden imagery
        public override Color Color => Color.FromArgb(255, 200, 100);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.Decoration;
        public override ElementCategory Category => ElementCategory.Decoration;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: Cannot generate on gravel garden zones or flow zones
            if (zone.Type == ZoneType.GravelGarden || zone.Type == ZoneType.Flow) return false;

            // Rule 2: Only place on fine gravel
            if (currentGarden[row, col] != '.') return false;

            // Rule 3: Maximum 2 stone lanterns per garden
            if (context.GetPlacementCount(GetType()) >= 2) return false;

            // Rule 4: Must maintain large distance from garden edges
            int height = currentGarden.GetLength(0);
            int width = currentGarden.GetLength(1);
            if (row < 10 || row >= height - 10 || col < 10 || col >= width - 10) return false;

            // Rule 5: Minimum 20 units distance from other lanterns
            if (context.IsNearElement(row, col, GetType(), 20)) return false;

            // Rule 6: Minimum distance from other spiritual elements
            if (context.IsNearElement(row, col, typeof(WaterFeature), 8) ||
                context.IsNearElement(row, col, typeof(BridgePath), 8)) return false;

            // Rule 7: Must have large clear space around (no other elements within 6 units)
            return HasSufficientClearSpace(row, col, currentGarden, 6);
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.001; // Very rare

            // Strongly prefer focal points
            probability *= zone.Type switch
            {
                ZoneType.FocalPoint => 20.0,
                _ => 0.1
            };

            // Reduce probability if one already exists
            if (context.GetPlacementCount(GetType()) > 0)
                probability *= 0.1;

            return probability * zone.GetDistanceInfluence(row, col);
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            base.PlaceElement(row, col, garden, context);

            // Create small clear area around lantern
            CreateClearArea(row, col, garden);
        }

        private static void CreateClearArea(int row, int col, char[,] garden)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r >= 0 && r < garden.GetLength(0) && c >= 0 && c < garden.GetLength(1) &&
                        (r != row || c != col) && garden[r, c] == '.')
                    {
                        // Keep clear space as fine gravel
                    }
                }
            }
        }
    }
}