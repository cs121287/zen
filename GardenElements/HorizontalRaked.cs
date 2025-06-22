using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class HorizontalRaked : ZenElement
    {
        public override char Symbol => '-';
        public override string Name => "Horizontal Raked";
        public override string Meaning => "Water currents, waves, flow";
        // Subtle blue-gray for raked gravel patterns from garden imagery
        public override Color Color => Color.FromArgb(180, 190, 200);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.FlowPatterns;
        public override ElementCategory Category => ElementCategory.Pattern;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: Cannot generate on gravel garden zones
            if (zone.Type == ZoneType.GravelGarden) return false;

            // Rule 2: Only place on fine gravel
            if (currentGarden[row, col] != '.') return false;

            // Rule 3: Minimum distance from stone elements
            if (context.IsNearElement(row, col, typeof(LargeRocks), 3) ||
                context.IsNearElement(row, col, typeof(MediumRocks), 2) ||
                context.IsNearElement(row, col, typeof(StoneLantern), 5)) return false;

            return true;
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.08;

            // Prefer flow and center zones
            probability *= zone.Type switch
            {
                ZoneType.Flow => 4.0,
                ZoneType.Center => 2.5,
                _ => 0.5
            };

            // Create wave patterns
            probability *= 1.0 + Math.Sin(row * 0.3) * 0.4;

            return probability;
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            // Create horizontal lines of 3-8 characters
            int lineLength = context.Random.Next(3, 9);

            for (int i = 0; i < lineLength && col + i < garden.GetLength(1); i++)
            {
                if (garden[row, col + i] == '.')
                {
                    garden[row, col + i] = '-';
                    context.RecordPlacement(GetType(), row, col + i);
                }
                else
                {
                    break; // Stop if we hit an obstacle
                }
            }
        }
    }
}