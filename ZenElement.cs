using System;
using System.Drawing;

namespace ZenGardenGenerator
{
    /// <summary>
    /// Base class for all Zen garden elements with enhanced rule system
    /// </summary>
    public abstract class ZenElement : IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// The ASCII character symbol representing this element
        /// </summary>
        public abstract char Symbol { get; }

        /// <summary>
        /// The display name of the element
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The traditional meaning and symbolism of the element
        /// </summary>
        public abstract string Meaning { get; }

        /// <summary>
        /// The color used to display this element (based on authentic Japanese garden imagery)
        /// </summary>
        public abstract Color Color { get; }

        /// <summary>
        /// The generation phase when this element should be placed
        /// </summary>
        public abstract GenerationRules.GenerationPhase Phase { get; }

        /// <summary>
        /// The category this element belongs to
        /// </summary>
        public abstract ElementCategory Category { get; }

        /// <summary>
        /// The visual density weight for ASCII art hierarchy
        /// </summary>
        public virtual int VisualDensity => GenerationRules.VisualDensity.GetValueOrDefault(Symbol, 0);

        /// <summary>
        /// Determines if this element can be placed at the specified coordinates
        /// </summary>
        public abstract bool CanPlaceAt(int row, int col, GardenZone zone, char[,] currentGarden, GardenContext context);

        /// <summary>
        /// Calculates the probability of this element appearing at specific coordinates
        /// </summary>
        public abstract double CalculateProbability(int row, int col, GardenZone zone, GardenContext context);

        /// <summary>
        /// Places the element and any required supporting elements
        /// </summary>
        public virtual void PlaceElement(int row, int col, char[,] garden, GardenContext context)
        {
            garden[row, col] = Symbol;
            context.RecordPlacement(GetType(), row, col);
        }

        /// <summary>
        /// Gets the minimum required clear space around this element
        /// </summary>
        public virtual int RequiredClearSpace => GenerationRules.Limits.TryGetValue(GetType(), out var limits) ? limits.MinDistance : 1;

        protected bool HasNearbyElement(int row, int col, char[,] garden, char element, int radius)
        {
            for (int r = Math.Max(0, row - radius); r <= Math.Min(garden.GetLength(0) - 1, row + radius); r++)
            {
                for (int c = Math.Max(0, col - radius); c <= Math.Min(garden.GetLength(1) - 1, col + radius); c++)
                {
                    if (r != row || c != col && garden[r, c] == element)
                        return true;
                }
            }
            return false;
        }

        protected bool HasSufficientClearSpace(int row, int col, char[,] garden, int requiredSpace, char excludeElement = '.')
        {
            for (int r = Math.Max(0, row - requiredSpace); r <= Math.Min(garden.GetLength(0) - 1, row + requiredSpace); r++)
            {
                for (int c = Math.Max(0, col - requiredSpace); c <= Math.Min(garden.GetLength(1) - 1, col + requiredSpace); c++)
                {
                    if (r != row || c != col)
                    {
                        char element = garden[r, c];
                        if (element != excludeElement && element != '\0')
                            return false;
                    }
                }
            }
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                disposed = true;
            }
        }
    }

    /// <summary>
    /// Categories of Zen garden elements
    /// </summary>
    public enum ElementCategory
    {
        Terrain,    // Rocks and stones
        Water,      // Water features and rivers
        Structure,  // Bridges and paths
        Surface,    // Gravel and base
        Pattern,    // Raked patterns
        Decoration  // Moss and lanterns
    }
}