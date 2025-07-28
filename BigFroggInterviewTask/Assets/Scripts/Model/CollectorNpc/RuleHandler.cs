using BigFroggInterviewTask.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.CollectorNpc
{
    /// <summary>
    /// Helper function for the CollectorNpcModel interpret the rules governing its behavior.
    /// </summary>
    public static class RuleHandler
    {
        /// <summary>
        /// Enumerates the sides of the world that boxes can be sorted into.
        /// </summary>
        public enum WorldSide
        {
            Left,
            Right,
        };

        /// <summary>
        /// Defines the rules used by the NPC to determine its behavior.
        /// </summary>
        public struct RuleSet
        {
            /// <summary>
            /// Defines which side of the world the NPC will sort boxes into.
            /// </summary>
            public Dictionary<BoxModel.BoxColor, WorldSide> SortingRules;
        };

        /// <summary>
        /// Gets a list of all boxes in the world not already sorted according to the NPC's rules.
        /// </summary>
        /// <param name="rules">The box sorting rules.</param>
        /// <param name="world">The world to search for boxes in.</param>
        /// <returns>A list of the locations of all boxes in the world not sorted according to the rules.</returns>
        public static List<Vector2Int> FindAllUnsortedBoxes(RuleSet rules, WorldModel world)
        {
            Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"Attempting to find all unsorted boxes");

            // Boxes are sorted to the right or leftmost side of the world.
            // Once the right/leftmost column is full, boxes will be sorted into the next column on that side.
            // Therefore, the acceptable sorting area for any box is the first partially empty column on its side.
            Dictionary<BoxModel.BoxColor, int> firstPartiallyEmptyColumn = new Dictionary<BoxModel.BoxColor, int>();
            foreach (BoxModel.BoxColor color in rules.SortingRules.Keys)
            {
                firstPartiallyEmptyColumn[color] = FindFirstPartiallyEmptyColumn(world, rules.SortingRules[color]);
                Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"{color} boxes outside column {firstPartiallyEmptyColumn[color]} are sorted");
            }

            Dictionary<Vector2Int, BoxModel> allBoxes = world.GetAllEntities<BoxModel>();
            List<Vector2Int> unsortedBoxes = new List<Vector2Int>();

            // Seach all boxes in the world. If the box column is outside of the first partially empty column, the box is sorted.
            foreach (KeyValuePair<Vector2Int, BoxModel> box in allBoxes)
            {
                BoxModel.BoxColor color = box.Value.Color;
                int boxColumn = box.Key.x;

                bool isBoxSorted;
                if (rules.SortingRules[color] == WorldSide.Left)
                {
                    isBoxSorted = boxColumn <= firstPartiallyEmptyColumn[color];
                }
                else if (rules.SortingRules[color] == WorldSide.Right)
                {
                    isBoxSorted = boxColumn >= firstPartiallyEmptyColumn[color];
                }
                else
                {
                    throw new Exception($"FindAllUnsortedBoxes: Unhandled sorting rule [{rules.SortingRules[color]}] for color {color}");
                }

                Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"{color} box in column {boxColumn} is sorted? {isBoxSorted}");
                if (!isBoxSorted)
                {
                    unsortedBoxes.Add(box.Key);
                }
            }

            Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"{unsortedBoxes.Count} unsorted boxes found");
            return unsortedBoxes;
        }

        /// <summary>
        /// Gets a list of possible locations for a box of the given color to be sorted into.
        /// </summary>
        /// <param name="rules">The box sorting rules.</param>
        /// <param name="world">The world to search for sorting locations in.</param>
        /// <param name="color">The color of box to search for sorting locations for.</param>
        /// <returns>A list of the locations of all boxes in the world not sorted according to the rules.</returns>
        public static List<Vector2Int> GetDropoffLocationsForBox(RuleSet rules, WorldModel world, BoxModel.BoxColor color)
        {
            Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"Attempting to find dropoff locations for {color} boxes");

            // Boxes are sorted to the right or leftmost side of the world.
            // Once the right/leftmost column is full, boxes will be sorted into the next column on that side.
            // Therefore, the acceptable sorting area for any box is the first partially empty column on its side.
            int firstPartiallyEmptyColumn = FindFirstPartiallyEmptyColumn(world, rules.SortingRules[color]);
            Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"{color} box dropoff locations are in column {firstPartiallyEmptyColumn}");

            List<Vector2Int> emptyTilesInDropoffColumn = new List<Vector2Int>();

            // All tiles in the dropoff column not already occupied by a box are valid locations to sort a box into
            if (firstPartiallyEmptyColumn >= 0 && firstPartiallyEmptyColumn < world.Size.x)
            {
                for (int y = 0; y < world.Size.y; y++)
                {
                    Vector2Int location = new Vector2Int(firstPartiallyEmptyColumn, y);
                    if (!(world.GetEntityAt(location) is BoxModel))
                    {
                        Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"{color} box dropoff location found: [{location}]");
                        emptyTilesInDropoffColumn.Add(location);
                    }
                }
            }

            Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"{emptyTilesInDropoffColumn.Count} dropoff locations found");
            return emptyTilesInDropoffColumn;
        }

        /// <summary>
        /// Gets the first column in the world on the given side that is not completely full of boxes.
        /// </summary>
        /// <param name="world">The world to search for partially empty columns in.</param>
        /// <param name="side">The side of the world to start searching on.</param>
        /// <returns>The column index of the first column not completely full of boxes.</returns>
        private static int FindFirstPartiallyEmptyColumn(WorldModel world, WorldSide side)
        {
            Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"Attempting to find first partially empty column on the world {side}");
            int column;
            int nextColumnDirection;

            if (side == WorldSide.Left)
            {
                // Start searching in the leftmost column and move to the right.
                column = 0;
                nextColumnDirection = 1;
            }
            else if (side == WorldSide.Right)
            {
                // Start searching in the rightmost column and move to the left.
                column = world.Size.x - 1;
                nextColumnDirection = -1;
            }
            else
            {
                throw new Exception($"FindFirstPartiallyEmptyColumn: Unhandled sorting rule [{side}]");
            }

            // Search each column in order until the first one that is not completely full of boxes is found.
            bool partiallyEmptyColumnFound = false;
            while (!partiallyEmptyColumnFound && column >= 0 && column < world.Size.x)
            {
                Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"Searching column {column}");
                for (int y = 0; y < world.Size.y; y++)
                {
                    if (!(world.GetEntityAt(new Vector2Int(column, y)) is BoxModel))
                    {
                        Log.Write(Log.Flag.CollectorNpcRuleHandlerTrace, $"Empty tile found at row {y}");
                        partiallyEmptyColumnFound = true;
                        break;
                    }
                }

                if (!partiallyEmptyColumnFound)
                {
                    column += nextColumnDirection;
                }
            }

            return column;
        }
    }
}