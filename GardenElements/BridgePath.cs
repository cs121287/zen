using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class BridgePath : ZenElement
    {
        public override char Symbol => '=';
        public override string Name => "Bridge/Path";
        public override string Meaning => "Transition, journey, enlightenment";
        // Natural wood bridge color from Japanese garden imagery
        public override Color Color => Color.FromArgb(160, 120, 80);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.Infrastructure;
        public override ElementCategory Category => ElementCategory.Structure;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: Only one bridge per garden
            if (context.BridgePlaced) return false;

            // Rule 2: No bridges in gravel garden zones
            if (zone.Type == ZoneType.GravelGarden) return false;

            // Rule 3: Must maintain distance from garden edges
            int height = currentGarden.GetLength(0);
            int width = currentGarden.GetLength(1);
            if (row < 3 || row >= height - 3 || col < 10 || col >= width - 10) return false;

            // Rule 4: Minimum distance from stone lanterns
            if (context.IsNearElement(row, col, typeof(StoneLantern), 8)) return false;

            return true;
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            if (context.BridgePlaced) return 0;

            double probability = 0.015;

            // Prefer flow and center zones
            probability *= zone.Type switch
            {
                ZoneType.Flow => 4.0,
                ZoneType.Center => 3.0,
                ZoneType.FocalPoint => 2.0,
                _ => 0.5
            };

            // Higher probability if there are water features to cross
            if (context.WaterPaths.Count > 0)
            {
                probability *= 2.0;
            }

            return probability * zone.GetDistanceInfluence(row, col);
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            // Create a bridge/path of 5-12 units long
            int length = context.Random.Next(5, 13);
            bool isHorizontal = context.Random.NextDouble() < 0.6; // 60% horizontal bridges

            // Check if path would intersect water
            if (context.WouldIntersectWater(row, col, length, isHorizontal))
            {
                // This is good - bridge should cross water
                length = Math.Min(length, isHorizontal ?
                    garden.GetLength(1) - col - 2 :
                    garden.GetLength(0) - row - 2);
            }

            // Place the bridge/path
            for (int i = 0; i < length; i++)
            {
                int placeRow = isHorizontal ? row : row + i;
                int placeCol = isHorizontal ? col + i : col;

                if (placeRow >= 0 && placeRow < garden.GetLength(0) &&
                    placeCol >= 0 && placeCol < garden.GetLength(1))
                {
                    // Only place on gravel or cross water
                    if (garden[placeRow, placeCol] == '.' || garden[placeRow, placeCol] == '+')
                    {
                        garden[placeRow, placeCol] = '=';
                        context.RecordPlacement(GetType(), placeRow, placeCol);
                    }
                }
            }
        }
    }
}