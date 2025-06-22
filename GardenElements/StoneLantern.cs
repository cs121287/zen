using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class StoneLantern : ZenElement
    {
        public override char Symbol => '*';
        public override string Name => "Stone Lantern";
        public override string Meaning => "Enlightenment, peace, meditation";
        public override Color Color => Color.FromArgb(255, 215, 0);
        public override double BaseProbability => 0.002;
        public override ElementCategory Category => ElementCategory.Spiritual;

        public override double CalculateProbability(int row, int col, GardenZone zone, Random random)
        {
            double probability = BaseProbability;
            
            switch (zone.Type)
            {
                case ZoneType.FocalPoint:
                    probability *= 15.0; // Very high preference for focal points
                    break;
                case ZoneType.Corner:
                    probability *= 8.0;
                    break;
                case ZoneType.Edge:
                    probability *= 3.0;
                    break;
                case ZoneType.Center:
                    probability *= 0.1;
                    break;
                case ZoneType.Flow:
                    probability *= 0.05;
                    break;
            }
            
            return probability;
        }

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden)
        {
            // Lanterns need space around them and should be rare
            return !HasNearbySpiritual(row, col, currentGarden, 8) && 
                   !HasNearbyElement(row, col, currentGarden, '*', 15);
        }

        private bool HasNearbySpiritual(int row, int col, char[,] garden, int radius)
        {
            for (int r = Math.Max(0, row - radius); r <= Math.Min(garden.GetLength(0) - 1, row + radius); r++)
            {
                for (int c = Math.Max(0, col - radius); c <= Math.Min(garden.GetLength(1) - 1, col + radius); c++)
                {
                    if (r != row || c != col)
                    {
                        char element = garden[r, c];
                        if (element == '*' || element == '=' || element == '+')
                            return true;
                    }
                }
            }
            return false;
        }

        private bool HasNearbyElement(int row, int col, char[,] garden, char element, int radius)
        {
            for (int r = Math.Max(0, row - radius); r <= Math.Min(garden.GetLength(0) - 1, row + radius); r++)
            {
                for (int c = Math.Max(0, col - radius); c <= Math.Min(garden.GetLength(1) - 1, col + radius); c++)
                {
                    if (r != row || c != col)
                    {
                        if (garden[r, c] == element)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}