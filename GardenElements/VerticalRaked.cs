using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class VerticalRaked : ZenElement
    {
        public override char Symbol => '|';
        public override string Name => "Vertical Raked";
        public override string Meaning => "Flowing rivers, streams";
        // Slightly darker blue-gray for vertical flow patterns
        public override Color Color => Color.FromArgb(160, 175, 190);
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
            double probability = 0.05;

            // Prefer flow zones and edges
            probability *= zone.Type switch
            {
                ZoneType.Flow => 3.5,
                ZoneType.Edge => 2.0,
                _ => 0.7
            };

            // Create stream patterns
            probability *= 1.0 + Math.Sin(col * 0.4) * 0.3;

            return probability;
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            // Create vertical lines of 3-6 characters
            int lineLength = context.Random.Next(3, 7);

            for (int i = 0; i < lineLength && row + i < garden.GetLength(0); i++)
            {
                if (garden[row + i, col] == '.')
                {
                    garden[row + i, col] = '|';
                    context.RecordPlacement(GetType(), row + i, col);
                }
                else
                {
                    break; // Stop if we hit an obstacle
                }
            }
        }
    }
}