using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class CurvedRaked : ZenElement
    {
        public override char Symbol => '~';
        public override string Name => "Curved Raked";
        public override string Meaning => "Ocean waves, natural water movement";
        // Flowing blue-gray for curved wave patterns
        public override Color Color => Color.FromArgb(140, 160, 180);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.FlowPatterns;
        public override ElementCategory Category => ElementCategory.Pattern;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: Cannot generate on gravel garden zones
            if (zone.Type == ZoneType.GravelGarden) return false;

            // Rule 2: Only place on fine gravel
            if (currentGarden[row, col] != '.') return false;

            // Rule 3: Minimum distance from stone elements
            if (context.IsNearElement(row, col, typeof(LargeRocks), 4) ||
                context.IsNearElement(row, col, typeof(MediumRocks), 3) ||
                context.IsNearElement(row, col, typeof(StoneLantern), 6)) return false;

            return true;
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.06;

            // Prefer flow and center zones
            probability *= zone.Type switch
            {
                ZoneType.Flow => 3.0,
                ZoneType.Center => 2.0,
                _ => 0.8
            };

            // Create natural wave patterns
            double waveValue = Math.Sin(row * 0.2) * Math.Cos(col * 0.15);
            probability *= 1.0 + waveValue * 0.5;

            return probability;
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            // Create curved patterns of 2-5 characters
            int curveLength = context.Random.Next(2, 6);
            
            for (int i = 0; i < curveLength; i++)
            {
                int waveRow = row + (int)(Math.Sin(i * 0.5) * 2);
                int waveCol = col + i;
                
                if (waveRow >= 0 && waveRow < garden.GetLength(0) && 
                    waveCol >= 0 && waveCol < garden.GetLength(1) &&
                    garden[waveRow, waveCol] == '.')
                {
                    garden[waveRow, waveCol] = '~';
                    context.RecordPlacement(GetType(), waveRow, waveCol);
                }
            }
        }
    }
}