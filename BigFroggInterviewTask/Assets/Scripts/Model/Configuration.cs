using System.IO;
using UnityEngine;

namespace BigFroggInterviewTask.Model
{
    /// <summary>
    /// Helper class used to read a configuration file.
    /// </summary>
    public abstract class Configuration
    {
        /// <summary>
        /// Load a configuration file from a given file path.
        /// </summary>
        public static T Load<T>(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found", path);
            }

            // The purpose of this method is to control the manner in which configuration files in the
            // project are loaded. Currently config files are expected to be in JSON format. If that
            // changes in the future, a single update to this method will change how all models load
            // their configurations.
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
    }
}
