using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class FineGravel : ZenElement
    {
        public override char Symbol => '.';
        public override string Name => "Fine Gravel";
        public override string Meaning => "Calm water, tranquil seas";
        public override Color Color => Color.FromArgb(245, 245, 220);
        public override double BaseProbability => 0.65;
        public override ElementCategory Category => ElementCategory.GravelSand;

        public override double CalculateProbability(int row, int col, GardenZone zone, Random random)
        {
            double probability = BaseProbability;
            
            switch (zone.Type)
            {
                case ZoneType.FocalPoint:
                    probability *= 0.3; // Less gravel around focal points
                    break;
                case ZoneType.Corner:
                    probability *= 0.7;
                    break;
                case ZoneType.Edge:
                    probability *= 0.8;
                    break;
                case ZoneType.Center:
                    probability *= 1.5; // More gravel in open areas
                    break;
                case ZoneType.Flow:
                    probability *= 1.2;
                    break;
            }
            
            return probability;
        }

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden)
        {
            // Fine gravel can go almost anywhere as the base element
            return true;
        }
    }
}