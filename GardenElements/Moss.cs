using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class Moss : ZenElement
    {
        public override char Symbol => '^';
        public override string Name => "Moss";
        public override string Meaning => "Life, growth, endurance, harmony";
        // Rich green moss color from Japanese garden imagery
        public override Color Color => Color.FromArgb(85, 120, 60);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.Decoration;
        public override ElementCategory Category => ElementCategory.Decoration;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: Cannot generate on gravel garden zones
            if (zone.Type == ZoneType.GravelGarden) return false;

            // Rule 2: Only place on fine gravel
            if (currentGarden[row, col] != '.') return false;

            // Rule 3: Maximum 15 moss patches per garden
            if (context.GetPlacementCount(GetType()) >= 15) return false;

            // Rule 4: Must be near rocks, edges, or water (moss grows in moist areas)
            bool nearRock = context.IsNearElement(row, col, typeof(LargeRocks), 3) ||
                           context.IsNearElement(row, col, typeof(MediumRocks), 2) ||
                           context.IsNearElement(row, col, typeof(SmallStones), 2);

            bool nearWater = context.WaterPaths.Any(path =>
                path.Points.Any(p => Math.Abs(p.row - row) <= 3 && Math.Abs(p.col - col) <= 3));

            bool nearEdge = row < 5 || row >= currentGarden.GetLength(0) - 5 ||
                           col < 5 || col >= currentGarden.GetLength(1) - 5;

            return nearRock || nearWater || nearEdge;
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.03;

            // Prefer corners and edges
            probability *= zone.Type switch
            {
                ZoneType.Corner => 3.5,
                ZoneType.Edge => 2.8,
                ZoneType.FocalPoint => 1.5,
                _ => 0.5
            };

            // Higher probability near rocks and water
            if (context.IsNearElement(row, col, typeof(LargeRocks), 4))
                probability *= 2.0;

            if (context.WaterPaths.Any(path =>
                path.Points.Any(p => Math.Abs(p.row - row) <= 4 && Math.Abs(p.col - col) <= 4)))
                probability *= 1.8;

            return probability * zone.GetDistanceInfluence(row, col);
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            base.PlaceElement(row, col, garden, context);

            // Create small moss clusters (1-3 additional patches)
            int clusterSize = context.Random.Next(1, 4);

            for (int i = 0; i < clusterSize; i++)
            {
                int offsetRow = row + context.Random.Next(-2, 3);
                int offsetCol = col + context.Random.Next(-2, 3);

                if (offsetRow >= 0 && offsetRow < garden.GetLength(0) &&
                    offsetCol >= 0 && offsetCol < garden.GetLength(1) &&
                    garden[offsetRow, offsetCol] == '.' &&
                    context.GetPlacementCount(GetType()) < 15)
                {
                    garden[offsetRow, offsetCol] = '^';
                    context.RecordPlacement(GetType(), offsetRow, offsetCol);
                }
            }
        }
    }
}