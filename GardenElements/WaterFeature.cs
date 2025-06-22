using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class WaterFeature : ZenElement
    {
        public override char Symbol => '+';
        public override string Name => "Water Feature";
        public override string Meaning => "Purity, flow of life, intersection";
        public override Color Color => Color.FromArgb(0, 191, 255);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.Water;
        public override ElementCategory Category => ElementCategory.Water;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Rule 1: No water features in gravel garden zones or corners
            if (zone.Type == ZoneType.GravelGarden || zone.Type == ZoneType.Corner) return false;

            // Rule 2: Must maintain distance from garden edges
            int height = currentGarden.GetLength(0);
            int width = currentGarden.GetLength(1);
            if (row < 8 || row >= height - 8 || col < 8 || col >= width - 8) return false;

            // Rule 3: Maximum 3 water features per garden
            if (context.GetPlacementCount(GetType()) >= 3) return false;

            // Rule 4: Minimum 12 units distance from other water features
            if (context.IsNearElement(row, col, GetType(), 12)) return false;

            // Rule 5: Must have sufficient clear space
            return HasSufficientClearSpace(row, col, currentGarden, 6);
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            double probability = 0.01;

            // Prefer center and focal point zones
            switch (zone.Type)
            {
                case ZoneType.Center:
                    probability *= 5.0;
                    break;
                case ZoneType.FocalPoint:
                    probability *= 4.0;
                    break;
                default:
                    probability *= 0.2;
                    break;
            }

            return probability * zone.GetDistanceInfluence(row, col);
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            base.PlaceElement(row, col, garden, context);
            
            // Create a small pond or river system
            var waterPath = new WaterPath();
            
            // Decide if this is a pond or river start
            if (context.Random.NextDouble() < 0.6) // 60% chance for pond
            {
                CreatePond(row, col, garden, context, waterPath);
            }
            else
            {
                CreateRiver(row, col, garden, context, waterPath);
            }
            
            context.RecordWaterPath(waterPath);
        }

        private void CreatePond(int row, int col, char[,] garden, GardenContext context, WaterPath waterPath)
        {
            waterPath.IsPond = true;
            waterPath.AddPoint(row, col);
            
            // Small pond 2-3 units in diameter
            var pondSize = context.Random.Next(1, 3);
            
            for (int r = row - pondSize; r <= row + pondSize; r++)
            {
                for (int c = col - pondSize; c <= col + pondSize; c++)
                {
                    if (r >= 0 && r < garden.GetLength(0) && c >= 0 && c < garden.GetLength(1))
                    {
                        double distance = Math.Sqrt(Math.Pow(r - row, 2) + Math.Pow(c - col, 2));
                        if (distance <= pondSize && garden[r, c] == '.')
                        {
                            garden[r, c] = '+';
                            waterPath.AddPoint(r, c);
                        }
                    }
                }
            }
        }

        private void CreateRiver(int row, int col, char[,] garden, GardenContext context, WaterPath waterPath)
        {
            waterPath.AddPoint(row, col);
            
            // Create winding river
            int currentRow = row;
            int currentCol = col;
            int length = context.Random.Next(8, 20);
            
            // Random initial direction
            double direction = context.Random.NextDouble() * Math.PI * 2;
            
            for (int i = 0; i < length; i++)
            {
                // Gradually change direction for natural winding
                direction += (context.Random.NextDouble() - 0.5) * 0.8;
                
                int nextRow = currentRow + (int)Math.Round(Math.Sin(direction));
                int nextCol = currentCol + (int)Math.Round(Math.Cos(direction));
                
                // Bounds check
                if (nextRow < 1 || nextRow >= garden.GetLength(0) - 1 || 
                    nextCol < 1 || nextCol >= garden.GetLength(1) - 1)
                    break;
                
                // Check for intersection with existing water
                if (context.WaterPaths.Any(path => path.ContainsPoint(nextRow, nextCol)))
                    break;
                
                // Place water if area is clear
                if (garden[nextRow, nextCol] == '.')
                {
                    garden[nextRow, nextCol] = '+';
                    waterPath.AddPoint(nextRow, nextCol);
                    currentRow = nextRow;
                    currentCol = nextCol;
                }
                else
                {
                    break; // Stop if we hit an obstacle
                }
            }
        }
    }
}