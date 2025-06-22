using System;
using System.Drawing;

namespace ZenGardenGenerator.GardenElements
{
    public class FineGravel : ZenElement
    {
        public override char Symbol => '.';
        public override string Name => "Fine Gravel";
        public override string Meaning => "Calm water, tranquil seas";
        // Beige/tan gravel color from authentic Japanese garden paths
        public override Color Color => Color.FromArgb(230, 220, 200);
        public override GenerationRules.GenerationPhase Phase => GenerationRules.GenerationPhase.GravelGarden;
        public override ElementCategory Category => ElementCategory.Surface;

        public override bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context)
        {
            // Fine gravel is the base element and can go anywhere that's empty
            return currentGarden[row, col] == '\0';
        }

        public override double CalculateProbability(int row, int col, GardenZone zone, GardenContext context)
        {
            // Base element - always place where nothing else exists
            return 1.0;
        }

        public override void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            // Only place if the cell is empty (background element)
            if (garden[row, col] == '\0')
            {
                garden[row, col] = Symbol;
            }
        }
    }
}