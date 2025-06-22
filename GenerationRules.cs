using System;
using System.Collections.Generic;

namespace ZenGardenGenerator
{
    /// <summary>
    /// Comprehensive rules system for authentic Japanese garden generation
    /// </summary>
    public static class GenerationRules
    {
        // ASCII Visual Density Hierarchy (darkest to lightest)
        public static readonly Dictionary<char, int> VisualDensity = new()
        {
            ['#'] = 9,  // Darkest - Large rocks (heaviest visual weight)
            ['@'] = 8,  // Very dark - Medium rocks
            ['*'] = 7,  // Dark - Stone lanterns (bright but dense)
            ['o'] = 6,  // Medium-dark - Small stones
            ['+'] = 5,  // Medium - Water features (intersections)
            ['='] = 4,  // Medium-light - Bridge/path (structured)
            ['^'] = 3,  // Light-medium - Moss (organic texture)
            ['~'] = 2,  // Light - Curved raked (flowing)
            ['|'] = 1,  // Very light - Vertical raked (linear)
            ['-'] = 1,  // Very light - Horizontal raked (linear)
            ['.'] = 0   // Lightest - Fine gravel (background)
        };

        // Generation order phases
        public enum GenerationPhase
        {
            Terrain = 1,        // Large rocks, medium rocks, small stones
            Water = 2,          // Rivers and ponds (water features)
            Infrastructure = 3, // Bridge and paths
            GravelGarden = 4,   // Fine gravel base
            FlowPatterns = 5,   // Raked patterns
            Decoration = 6      // Moss and lanterns
        }

        // Element limits per garden
        public static readonly Dictionary<Type, ElementLimits> Limits = new()
        {
            [typeof(GardenElements.LargeRocks)] = new(1, 3, 8),      // min, max, minDistance
            [typeof(GardenElements.MediumRocks)] = new(2, 8, 4),     // min, max, minDistance
            [typeof(GardenElements.SmallStones)] = new(5, 20, 2),    // min, max, minDistance
            [typeof(GardenElements.WaterFeature)] = new(0, 3, 12),   // min, max, minDistance
            [typeof(GardenElements.BridgePath)] = new(0, 1, 0),      // Only one bridge per garden
            [typeof(GardenElements.StoneLantern)] = new(0, 2, 20),   // min, max, minDistance
            [typeof(GardenElements.Moss)] = new(3, 15, 1),          // min, max, minDistance
        };

        // Zone restrictions
        public static readonly Dictionary<Type, ZoneRestrictions> ZoneRules = new()
        {
            [typeof(GardenElements.LargeRocks)] = new(
                [ZoneType.GravelGarden],
                [ZoneType.FocalPoint, ZoneType.Corner],
                5
            ),
            [typeof(GardenElements.MediumRocks)] = new(
                [ZoneType.GravelGarden],
                [ZoneType.FocalPoint, ZoneType.Edge],
                3
            ),
            [typeof(GardenElements.SmallStones)] = new(
                [ZoneType.GravelGarden],
                [ZoneType.Edge, ZoneType.Corner],
                2
            ),
            [typeof(GardenElements.WaterFeature)] = new(
                [ZoneType.GravelGarden, ZoneType.Corner],
                [ZoneType.Center, ZoneType.FocalPoint],
                8
            ),
            [typeof(GardenElements.BridgePath)] = new(
                [ZoneType.GravelGarden],
                [ZoneType.Flow, ZoneType.Center],
                3
            ),
            [typeof(GardenElements.StoneLantern)] = new(
                [ZoneType.GravelGarden, ZoneType.Flow],
                [ZoneType.FocalPoint],
                10
            ),
            [typeof(GardenElements.Moss)] = new(
                [ZoneType.GravelGarden],
                [ZoneType.Edge, ZoneType.Corner],
                1
            )
        };
    }

    public record ElementLimits(int MinCount, int MaxCount, int MinDistance);
    public record ZoneRestrictions(ZoneType[] Forbidden, ZoneType[] Preferred, int EdgeBuffer);
}