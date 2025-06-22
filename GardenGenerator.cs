using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZenGardenGenerator.GardenElements;

namespace ZenGardenGenerator
{
    /// <summary>
    /// Advanced procedural generation engine with authentic Japanese garden rules
    /// </summary>
    public class GardenGenerator
    {
        private readonly Random random;
        private readonly List<ZenElement> elements;
        private readonly List<GardenZone> zones;
        private const int MAX_PLACEMENT_ATTEMPTS = 1000;
        
        public GardenGenerator(Random random)
        {
            this.random = random;
            this.elements = InitializeElements();
            this.zones = new List<GardenZone>();
        }
        
        private List<ZenElement> InitializeElements()
        {
            return new List<ZenElement>
            {
                // Terrain phase
                new LargeRocks(),
                new MediumRocks(),
                new SmallStones(),
                
                // Water phase
                new WaterFeature(),
                
                // Infrastructure phase
                new BridgePath(),
                
                // Surface phase
                new FineGravel(),
                
                // Flow patterns phase
                new HorizontalRaked(),
                new VerticalRaked(),
                new CurvedRaked(),
                
                // Decoration phase
                new Moss(),
                new StoneLantern()
            };
        }
        
        /// <summary>
        /// Generates a complete garden following authentic Japanese principles
        /// </summary>
        public async Task<char[,]> GenerateGardenAsync(int width, int height, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var garden = new char[height, width];
            var context = new GardenContext(random);
            
            // Step 1: Create zones (5% progress)
            zones.Clear();
            CreateGardenZones(width, height);
            progress?.Report(5);
            cancellationToken.ThrowIfCancellationRequested();
            
            // Step 2: Initialize garden array (10% progress)
            InitializeGarden(garden);
            progress?.Report(10);
            cancellationToken.ThrowIfCancellationRequested();
            
            // Step 3-8: Generate by phases (10-90% progress)
            await GenerateByPhases(garden, context, progress, cancellationToken);
            
            // Step 9: Final refinement (90-100% progress)
            await RefineGardenAsync(garden, context, progress, cancellationToken);
            
            progress?.Report(100);
            return garden;
        }
        
        private void CreateGardenZones(int width, int height)
        {
            // Create gravel garden zones (pure gravel areas)
            zones.Add(new GardenZone(ZoneType.GravelGarden, height/2, width/4, width/8, height/8, 1.0));
            zones.Add(new GardenZone(ZoneType.GravelGarden, height/4, width*3/4, width/10, height/10, 1.0));
            
            // Create focal points (for rocks and lanterns)
            zones.Add(new GardenZone(ZoneType.FocalPoint, height/5, width/6, width/8, height/6, 1.8));
            zones.Add(new GardenZone(ZoneType.FocalPoint, height*3/5, width*2/3, width/10, height/8, 1.5));
            
            // Create flow zones (for patterns and paths)
            zones.Add(new GardenZone(ZoneType.Flow, height/3, width/8, width*3/4, height/8, 1.2));
            zones.Add(new GardenZone(ZoneType.Flow, height/8, width/2, width/6, height*2/3, 1.0));
            
            // Create center zone (for water features)
            zones.Add(new GardenZone(ZoneType.Center, height/3, width/3, width/3, height/3, 1.3));
            
            // Create edge zones (for moss and stones)
            zones.Add(new GardenZone(ZoneType.Edge, 0, 0, width, height/12, 0.8));
            zones.Add(new GardenZone(ZoneType.Edge, height*11/12, 0, width, height/12, 0.8));
            zones.Add(new GardenZone(ZoneType.Edge, 0, 0, width/12, height, 0.8));
            zones.Add(new GardenZone(ZoneType.Edge, 0, width*11/12, width/12, height, 0.8));
            
            // Create corner zones (for moss and small rocks)
            zones.Add(new GardenZone(ZoneType.Corner, 0, 0, width/10, height/10, 1.0));
            zones.Add(new GardenZone(ZoneType.Corner, 0, width*9/10, width/10, height/10, 1.0));
            zones.Add(new GardenZone(ZoneType.Corner, height*9/10, 0, width/10, height/10, 1.0));
            zones.Add(new GardenZone(ZoneType.Corner, height*9/10, width*9/10, width/10, height/10, 1.0));
        }
        
        private void InitializeGarden(char[,] garden)
        {
            // Initialize all cells as empty (will be filled with fine gravel later)
            for (int row = 0; row < garden.GetLength(0); row++)
            {
                for (int col = 0; col < garden.GetLength(1); col++)
                {
                    garden[row, col] = '\0'; // Empty
                }
            }
        }
        
        private async Task GenerateByPhases(char[,] garden, GardenContext context, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var phases = Enum.GetValues<GenerationRules.GenerationPhase>().OrderBy(p => (int)p).ToList();
            
            for (int phaseIndex = 0; phaseIndex < phases.Count; phaseIndex++)
            {
                var currentPhase = phases[phaseIndex];
                var phaseElements = elements.Where(e => e.Phase == currentPhase).ToList();
                
                await GeneratePhase(garden, context, phaseElements, currentPhase, cancellationToken);
                
                // Progress from 10% to 90%
                int currentProgress = 10 + (80 * (phaseIndex + 1) / phases.Count);
                progress?.Report(currentProgress);
                
                // Small delay to make generation visible
                await Task.Delay(200, cancellationToken);
            }
        }
        
        private async Task GeneratePhase(char[,] garden, GardenContext context, List<ZenElement> phaseElements, GenerationRules.GenerationPhase phase, CancellationToken cancellationToken)
        {
            switch (phase)
            {
                case GenerationRules.GenerationPhase.Terrain:
                    await GenerateTerrainPhase(garden, context, phaseElements, cancellationToken);
                    break;
                case GenerationRules.GenerationPhase.Water:
                    await GenerateWaterPhase(garden, context, phaseElements, cancellationToken);
                    break;
                case GenerationRules.GenerationPhase.Infrastructure:
                    await GenerateInfrastructurePhase(garden, context, phaseElements, cancellationToken);
                    break;
                case GenerationRules.GenerationPhase.GravelGarden:
                    await GenerateGravelPhase(garden, context, phaseElements, cancellationToken);
                    break;
                case GenerationRules.GenerationPhase.FlowPatterns:
                    await GenerateFlowPatternsPhase(garden, context, phaseElements, cancellationToken);
                    break;
                case GenerationRules.GenerationPhase.Decoration:
                    await GenerateDecorationPhase(garden, context, phaseElements, cancellationToken);
                    break;
            }
        }
        
        private async Task GenerateTerrainPhase(char[,] garden, GardenContext context, List<ZenElement> elements, CancellationToken cancellationToken)
        {
            // Generate rocks in order: Large -> Medium -> Small
            foreach (var element in elements.OrderBy(e => e.VisualDensity).Reverse())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await PlaceElementWithLimits(garden, context, element, cancellationToken);
            }
        }
        
        private async Task GenerateWaterPhase(char[,] garden, GardenContext context, List<ZenElement> elements, CancellationToken cancellationToken)
        {
            // Generate water features
            foreach (var element in elements)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await PlaceElementWithLimits(garden, context, element, cancellationToken);
            }
        }
        
        private async Task GenerateInfrastructurePhase(char[,] garden, GardenContext context, List<ZenElement> elements, CancellationToken cancellationToken)
        {
            // Generate bridge/path (only one per garden)
            foreach (var element in elements)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await PlaceElementWithLimits(garden, context, element, cancellationToken);
            }
        }
        
        private async Task GenerateGravelPhase(char[,] garden, GardenContext context, List<ZenElement> elements, CancellationToken cancellationToken)
        {
            // Fill all empty spaces with fine gravel
            var fineGravel = elements.FirstOrDefault();
            if (fineGravel != null)
            {
                for (int row = 0; row < garden.GetLength(0); row++)
                {
                    for (int col = 0; col < garden.GetLength(1); col++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (garden[row, col] == '\0')
                        {
                            fineGravel.PlaceElement(row, col, garden, context);
                        }
                    }
                    
                    // Periodic yield for UI updates
                    if (row % 10 == 0)
                        await Task.Yield();
                }
            }
        }
        
        private async Task GenerateFlowPatternsPhase(char[,] garden, GardenContext context, List<ZenElement> elements, CancellationToken cancellationToken)
        {
            // Generate raked patterns
            foreach (var element in elements)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await PlacePatternElement(garden, context, element, cancellationToken);
            }
        }
        
        private async Task GenerateDecorationPhase(char[,] garden, GardenContext context, List<ZenElement> elements, CancellationToken cancellationToken)
        {
            // Generate decorative elements: Moss -> Stone Lanterns
            foreach (var element in elements.OrderBy(e => e.VisualDensity))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await PlaceElementWithLimits(garden, context, element, cancellationToken);
            }
        }
        
        private async Task PlaceElementWithLimits(char[,] garden, GardenContext context, ZenElement element, CancellationToken cancellationToken)
        {
            if (!GenerationRules.Limits.TryGetValue(element.GetType(), out var limits))
                return;
            
            int attempts = 0;
            int placements = 0;
            int maxAttempts = MAX_PLACEMENT_ATTEMPTS * limits.MaxCount;
            
            while (placements < limits.MaxCount && attempts < maxAttempts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                attempts++;
                
                int row = random.Next(garden.GetLength(0));
                int col = random.Next(garden.GetLength(1));
                
                var zone = GetDominantZone(row, col);
                
                if (element.CanPlaceAt(row, col, zone, garden, context))
                {
                    double probability = element.CalculateProbability(row, col, zone, context);
                    if (random.NextDouble() < probability)
                    {
                        element.PlaceElement(row, col, garden, context);
                        placements++;
                    }
                }
                
                // Periodic yield for UI updates
                if (attempts % 100 == 0)
                    await Task.Yield();
            }
            
            // Ensure minimum placements for essential elements
            if (placements < limits.MinCount)
            {
                await ForceMinimumPlacements(garden, context, element, limits.MinCount - placements, cancellationToken);
            }
        }
        
        private async Task PlacePatternElement(char[,] garden, GardenContext context, ZenElement element, CancellationToken cancellationToken)
        {
            int attempts = 0;
            int maxAttempts = MAX_PLACEMENT_ATTEMPTS;
            
            while (attempts < maxAttempts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                attempts++;
                
                int row = random.Next(garden.GetLength(0));
                int col = random.Next(garden.GetLength(1));
                
                var zone = GetDominantZone(row, col);
                
                if (element.CanPlaceAt(row, col, zone, garden, context))
                {
                    double probability = element.CalculateProbability(row, col, zone, context);
                    if (random.NextDouble() < probability)
                    {
                        element.PlaceElement(row, col, garden, context);
                    }
                }
                
                // Periodic yield for UI updates
                if (attempts % 50 == 0)
                    await Task.Yield();
            }
        }
        
        private async Task ForceMinimumPlacements(char[,] garden, GardenContext context, ZenElement element, int needed, CancellationToken cancellationToken)
        {
            int attempts = 0;
            int placements = 0;
            int maxAttempts = MAX_PLACEMENT_ATTEMPTS * 2;
            
            while (placements < needed && attempts < maxAttempts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                attempts++;
                
                int row = random.Next(garden.GetLength(0));
                int col = random.Next(garden.GetLength(1));
                
                var zone = GetDominantZone(row, col);
                
                if (element.CanPlaceAt(row, col, zone, garden, context))
                {
                    element.PlaceElement(row, col, garden, context);
                    placements++;
                }
                
                // Periodic yield for UI updates
                if (attempts % 100 == 0)
                    await Task.Yield();
            }
        }
        
        private GardenZone GetDominantZone(int row, int col)
        {
            // Find the zone with the strongest influence at this position
            var containingZones = zones.Where(z => z.Contains(row, col)).ToList();
            
            if (containingZones.Any())
            {
                // Return the zone with highest influence
                return containingZones.OrderByDescending(z => z.Influence).First();
            }
            
            // If not in any specific zone, find the closest one
            return zones.OrderBy(z => 
                Math.Sqrt(Math.Pow(row - z.CenterRow, 2) + Math.Pow(col - z.CenterCol, 2))
            ).First();
        }
        
        private async Task RefineGardenAsync(char[,] garden, GardenContext context, IProgress<int> progress, CancellationToken cancellationToken)
        {
            // Enhance flow patterns (92% progress)
            EnhanceFlowPatterns(garden, context);
            progress?.Report(92);
            cancellationToken.ThrowIfCancellationRequested();
            
            // Clean up isolated elements (95% progress)
            CleanupIsolatedElements(garden, context);
            progress?.Report(95);
            cancellationToken.ThrowIfCancellationRequested();
            
            // Final ASCII density optimization (98% progress)
            OptimizeVisualDensity(garden, context);
            progress?.Report(98);
            cancellationToken.ThrowIfCancellationRequested();
            
            await Task.Delay(100, cancellationToken); // Brief pause for visual effect
        }
        
        private void EnhanceFlowPatterns(char[,] garden, GardenContext context)
        {
            int height = garden.GetLength(0);
            int width = garden.GetLength(1);
            
            // Enhance horizontal flow patterns
            for (int row = 0; row < height; row++)
            {
                for (int col = 1; col < width - 1; col++)
                {
                    if (garden[row, col] == '-' && garden[row, col-1] == '.' && garden[row, col+1] == '.')
                    {
                        // Extend horizontal lines naturally
                        if (random.NextDouble() < 0.4)
                        {
                            garden[row, col-1] = '-';
                        }
                        if (random.NextDouble() < 0.4)
                        {
                            garden[row, col+1] = '-';
                        }
                    }
                }
            }
            
            // Enhance vertical flow patterns
            for (int col = 0; col < width; col++)
            {
                for (int row = 1; row < height - 1; row++)
                {
                    if (garden[row, col] == '|' && garden[row-1, col] == '.' && garden[row+1, col] == '.')
                    {
                        // Extend vertical lines naturally
                        if (random.NextDouble() < 0.4)
                        {
                            garden[row-1, col] = '|';
                        }
                        if (random.NextDouble() < 0.4)
                        {
                            garden[row+1, col] = '|';
                        }
                    }
                }
            }
            
            // Create natural flow around rocks
            for (int row = 1; row < height - 1; row++)
            {
                for (int col = 1; col < width - 1; col++)
                {
                    char current = garden[row, col];
                    if (current == '#' || current == '@' || current == 'o')
                    {
                        // Add flow patterns around rocks
                        CreateFlowAroundObstacle(garden, row, col, context);
                    }
                }
            }
        }
        
        private void CreateFlowAroundObstacle(char[,] garden, int rockRow, int rockCol, GardenContext context)
        {
            // Create subtle flow patterns around rocks
            for (int dr = -2; dr <= 2; dr++)
            {
                for (int dc = -2; dc <= 2; dc++)
                {
                    int r = rockRow + dr;
                    int c = rockCol + dc;
                    
                    if (r >= 0 && r < garden.GetLength(0) && c >= 0 && c < garden.GetLength(1) &&
                        garden[r, c] == '.' && random.NextDouble() < 0.2)
                    {
                        // Determine flow direction based on position relative to rock
                        if (Math.Abs(dc) > Math.Abs(dr))
                        {
                            garden[r, c] = '-'; // Horizontal flow
                        }
                        else if (Math.Abs(dr) > Math.Abs(dc))
                        {
                            garden[r, c] = '|'; // Vertical flow
                        }
                        else if (random.NextDouble() < 0.5)
                        {
                            garden[r, c] = '~'; // Curved flow
                        }
                    }
                }
            }
        }
        
        private void CleanupIsolatedElements(char[,] garden, GardenContext context)
        {
            int height = garden.GetLength(0);
            int width = garden.GetLength(1);
            
            // Remove isolated spiritual elements that don't follow rules
            for (int row = 1; row < height - 1; row++)
            {
                for (int col = 1; col < width - 1; col++)
                {
                    char current = garden[row, col];
                    if (current == '^' || current == '*' || current == '=' || current == '+')
                    {
                        // Check if element is properly supported
                        bool hasProperSupport = CheckElementSupport(garden, row, col, current, context);
                        
                        if (!hasProperSupport && random.NextDouble() < 0.3)
                        {
                            garden[row, col] = '.'; // Replace with fine gravel
                        }
                    }
                }
            }
        }
        
        private bool CheckElementSupport(char[,] garden, int row, int col, char element, GardenContext context)
        {
            switch (element)
            {
                case '^': // Moss needs rocks or edges nearby
                    return HasNearbyRocks(garden, row, col, 3) || IsNearEdge(garden, row, col, 5);
                    
                case '*': // Lanterns need clear space
                    return HasClearSpace(garden, row, col, 4);
                    
                case '=': // Paths should connect or cross water
                    return IsPartOfPath(garden, row, col) || CrossesWater(garden, row, col);
                    
                case '+': // Water features should have proper containment
                    return true; // Water features are always valid once placed
                    
                default:
                    return true;
            }
        }
        
        private bool HasNearbyRocks(char[,] garden, int row, int col, int radius)
        {
            for (int r = Math.Max(0, row - radius); r <= Math.Min(garden.GetLength(0) - 1, row + radius); r++)
            {
                for (int c = Math.Max(0, col - radius); c <= Math.Min(garden.GetLength(1) - 1, col + radius); c++)
                {
                    char element = garden[r, c];
                    if (element == '#' || element == '@' || element == 'o')
                        return true;
                }
            }
            return false;
        }
        
        private bool IsNearEdge(char[,] garden, int row, int col, int distance)
        {
            return row < distance || row >= garden.GetLength(0) - distance ||
                   col < distance || col >= garden.GetLength(1) - distance;
        }
        
        private bool HasClearSpace(char[,] garden, int row, int col, int radius)
        {
            int clearCount = 0;
            int totalCount = 0;
            
            for (int r = Math.Max(0, row - radius); r <= Math.Min(garden.GetLength(0) - 1, row + radius); r++)
            {
                for (int c = Math.Max(0, col - radius); c <= Math.Min(garden.GetLength(1) - 1, col + radius); c++)
                {
                    if (r != row || c != col)
                    {
                        totalCount++;
                        if (garden[r, c] == '.' || garden[r, c] == '-' || garden[r, c] == '|' || garden[r, c] == '~')
                            clearCount++;
                    }
                }
            }
            
            return totalCount > 0 && (clearCount / (double)totalCount) > 0.7;
        }
        
        private bool IsPartOfPath(char[,] garden, int row, int col)
        {
            // Check if this path element connects to other path elements
            int pathConnections = 0;
            
            // Check 4 directions
            int[] dr = {-1, 1, 0, 0};
            int[] dc = {0, 0, -1, 1};
            
            for (int i = 0; i < 4; i++)
            {
                int r = row + dr[i];
                int c = col + dc[i];
                
                if (r >= 0 && r < garden.GetLength(0) && c >= 0 && c < garden.GetLength(1) &&
                    garden[r, c] == '=')
                {
                    pathConnections++;
                }
            }
            
            return pathConnections > 0;
        }
        
        private bool CrossesWater(char[,] garden, int row, int col)
        {
            // Check if this path element crosses water
            int[] dr = {-1, 1, 0, 0};
            int[] dc = {0, 0, -1, 1};
            
            for (int i = 0; i < 4; i++)
            {
                int r = row + dr[i];
                int c = col + dc[i];
                
                if (r >= 0 && r < garden.GetLength(0) && c >= 0 && c < garden.GetLength(1) &&
                    garden[r, c] == '+')
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private void OptimizeVisualDensity(char[,] garden, GardenContext context)
        {
            int height = garden.GetLength(0);
            int width = garden.GetLength(1);
            
            // Balance visual density for proper ASCII art hierarchy
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    char current = garden[row, col];
                    int currentDensity = GenerationRules.VisualDensity.GetValueOrDefault(current, 0);
                    
                    // Check surrounding density
                    int surroundingDensity = GetSurroundingDensity(garden, row, col);
                    
                    // Adjust for proper contrast and balance
                    if (currentDensity > 6 && surroundingDensity > 15) // Too dense area
                    {
                        // Reduce density slightly
                        if (random.NextDouble() < 0.1)
                        {
                            garden[row, col] = '.'; // Return to gravel
                        }
                    }
                }
            }
        }
        
        private int GetSurroundingDensity(char[,] garden, int row, int col)
        {
            int totalDensity = 0;
            
            for (int r = Math.Max(0, row - 1); r <= Math.Min(garden.GetLength(0) - 1, row + 1); r++)
            {
                for (int c = Math.Max(0, col - 1); c <= Math.Min(garden.GetLength(1) - 1, col + 1); c++)
                {
                    if (r != row || c != col)
                    {
                        char element = garden[r, c];
                        totalDensity += GenerationRules.VisualDensity.GetValueOrDefault(element, 0);
                    }
                }
            }
            
            return totalDensity;
        }
        
        public Dictionary<char, ZenElement> GetElementDictionary()
        {
            return elements.ToDictionary(e => e.Symbol);
        }
    }
}