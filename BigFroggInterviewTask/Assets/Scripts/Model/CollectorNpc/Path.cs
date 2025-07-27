using BigFroggInterviewTask.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.CollectorNpc
{
    using StepList = List<Vector2Int>;

    /// <summary>
    /// Helper function for the CollectorNpcModel to find paths to objectives in the world.
    /// </summary>
    public class Path
    {
        /// <summary>
        /// The destination location this path leads to.
        /// </summary>
        public Vector2Int Destination { get; private set; }

        /// <summary>
        /// The number of steps on the path remaining until the destination.
        /// </summary>
        public int StepsRemaining { get { return stepList.Count; } }

        /// <summary>
        /// The list of steps leading to the destination.
        /// </summary>
        private StepList stepList;

        /// <summary>
        /// Get the next step toward the destination and remove it from the path.
        /// </summary>
        public Vector2Int PopNextStep()
        {
            if (StepsRemaining == 0)
            {
                throw new InvalidOperationException($"PopNextStep: No steps remaining");
            }

            Vector2Int nextStep = stepList[0];
            stepList.Remove(nextStep);

            return nextStep;
        }

        /// <summary>
        /// Generates the shortest path between the origin locaiton and the nearest of any locations in the destination list.
        /// </summary>
        /// <param name="world">The world model to determine the path in.</param>
        /// <param name="origin">The start location.</param>
        /// <param name="destinations">The list of valid ending locations.</param>
        /// <returns>A list of locations the NPC can move through to reach the nearest destination from the origin, not including either the origin or the destination.</returns>
        public static Path FindShortestPath(WorldModel world, Vector2Int origin, List<Vector2Int> destinations)
        {
            Log.Write(Log.Flag.CollectorNpcPathFinderTrace, $"Attempting to find shortest path between [{origin}] and any of {LocationListToString(destinations)}");

            if (!world.IsLocationInRange(origin))
            {
                throw new ArgumentOutOfRangeException("origin");
            }

            if (destinations.Count == 0)
            {
                throw new ArgumentException("FindShortestPath: no destinations provided", "destinations");
            }

            // Maintain a list of visited tiles so that they are not searched multiple times
            List<Vector2Int> visitedTiles = new List<Vector2Int>() { origin };

            // Initialize the search list with a single path starting at the origin.
            List<StepList> pathsToSearch = new List<StepList> { new StepList { origin } };

            // Perform a breadth-first search to find the shortest path to any of the valid destinations.
            // Each iteration through the loop will generate a list of paths to all reachable tiles in the number of
            // steps equal to the iteration number. (i.e, the first loop will find all tiles reachable in 1 step, the
            // second loop will find all tiles reachable in 2 steps, etc.) When any destination tile is found, the
            // path is guarenteed to be the shortest available.
            while (pathsToSearch.Count > 0)
            {
                Log.Write(Log.Flag.CollectorNpcPathFinderTrace, $"Checking all paths of length {pathsToSearch[0].Count + 1}");
                List<StepList> pathesFoundThisIteration = new List<StepList>();

                foreach (StepList path in pathsToSearch)
                {
                    Log.Write(Log.Flag.CollectorNpcPathFinderTrace, $"Searching down path {LocationListToString(path)}");

                    // From the last step in the path, the NPC could potentially move to any of the tiles
                    // in the four cardinal directions.
                    Vector2Int lastStepInPath = path[path.Count - 1];
                    List<Vector2Int> adjacentTilesToLastStep = new List<Vector2Int>()
                    {
                        lastStepInPath + Vector2Int.up,
                        lastStepInPath + Vector2Int.down,
                        lastStepInPath + Vector2Int.left,
                        lastStepInPath + Vector2Int.right,
                    };

                    foreach (Vector2Int nextStep in adjacentTilesToLastStep)
                    {
                        if (destinations.Contains(nextStep))
                        {
                            // The path returned to the caller should be the one between the origin and destination,
                            // exclusive of both points. Remove the origin from the path.
                            path.Remove(origin);
                            Log.Write(Log.Flag.CollectorNpcPathFinderTrace, $"Shortest path to [{nextStep}] found: {LocationListToString(path)}");

                            return new Path(path, nextStep);
                        }

                        if (IsStepPassable(world, nextStep) && !visitedTiles.Contains(nextStep))
                        {
                            // The current path plus this adjacent tile is a valid path that has not yet been searched.
                            // Add it to the list of pathes to check during the next iteration.
                            StepList pathFound = new StepList(path);
                            pathFound.Add(nextStep);
                            pathesFoundThisIteration.Add(pathFound);

                            // Mark this tile as visited.
                            visitedTiles.Add(nextStep);

                            Log.Write(Log.Flag.CollectorNpcPathFinderTrace, $"Found path of length {pathFound.Count}: {LocationListToString(pathFound)}");
                        }
                    }
                }

                // All the paths with a length equal to the iteration number were found.
                // On the next iteration, use this list to find all paths with length one greater.
                Log.Write(Log.Flag.CollectorNpcPathFinderTrace, $"Total paths of length {pathesFoundThisIteration[0].Count} found: {pathesFoundThisIteration.Count}");
                pathsToSearch = pathesFoundThisIteration;
            }

            return null;
        }

        /// <summary>
        /// Create a path from a list of steps and a destination.
        /// </summary>
        private Path(StepList path, Vector2Int destination)
        {
            Destination = destination;
            stepList = new StepList(path);
        }

        /// <summary>
        /// Indicates whether every step in the path is passable in the world.
        /// </summary>
        /// <param name="world">The world model to validate the path against.</param>
        /// <returns>False if any step is blocked by an entity or outside the bounds of the world, true otherwise.</returns>
        public bool IsPathPassable(WorldModel world)
        {
            foreach (Vector2Int step in stepList)
            {
                if (!IsStepPassable(world, step))
                {
                    Log.Write(Log.Flag.CollectorNpcPathFinderTrace, $"Path not passable on step [{step}]: {LocationListToString(stepList)}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the step location contains a passable tile.
        /// </summary>
        /// <param name="world">The world model to validate the path against.</param>
        /// <param name="location">The step location.</param>
        /// <returns>False if the location is blocked by an entity or outside the bounds of the world, true otherwise.</returns>
        private static bool IsStepPassable(WorldModel world, Vector2Int location)
        {
            return world.IsLocationInRange(location) && world.GetEntityAt(location) == null;
        }

        /// <summary>
        /// Converts a list of locations to a string.
        /// </summary>
        private static string LocationListToString(List<Vector2Int> locations)
        {
            string pathString = "{";
            if (locations.Count > 0)
            {
                foreach (Vector2Int location in locations)
                {
                    pathString += $"[{location}], ";
                }
                pathString = pathString.Remove(pathString.Length - 2);
            }
            pathString += "}";
            return pathString;
        }
    }
}