using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Logging
{
    /// <summary>
    /// Logging class with flags to enable or disable different types of logging.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Enumerates the flags available to log different kinds of messages.
        /// </summary>
        public enum Flag
        {
            WorldModelTrace,
            RandomBoxSpawnerModelTrace,
            CollectorNpcPathFinderTrace,
        };

        /// <summary>
        /// Determines which messages will be logged.
        /// </summary>
        private static List<Flag> flags = new List<Flag>();

        /// <summary>
        /// Logs a message to the console, if the associated flag is enabled.
        /// </summary>
        public static void Write(Flag flag, string message)
        {
            if (flags.Contains(flag))
            {
                // Currently this writes messages to the Unity debug log. It could potentially be extended to
                // route the messages to other locations if desired (log file, print to screen, etc).
                // Preprocessor directives could also be used to set the type of logging based on the platorm.
                Debug.Log($"{flag}: {message}");
            }
        }

        /// <summary>
        /// Enable logging for messages with the given flag.
        /// </summary>
        public static void EnableFlag(Flag flag)
        {
            if (!flags.Contains(flag))
            {
                flags.Add(flag);
            }
        }

        /// <summary>
        /// Disable logging for messages with the given flag.
        /// </summary>
        public static void DisableFlag(Flag flag)
        {
            if (flags.Contains(flag))
            {
                flags.Remove(flag);
            }
        }

        /// <summary>
        /// Disable logging for all messages.
        /// </summary>
        /// <remarks>
        /// The Unity Editor does not reinitialize static classes when entering play mode or running unit tests.
        /// To ensure consistant logging functionality between runs, it is recommended to clear all flags and
        /// enable the ones which are to be used at the beginning of each run.
        /// </remarks>
        public static void ClearAllFlags()
        {
            flags.Clear();
        }
    }
}
