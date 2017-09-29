using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// Represents a single Spark side error returned from an operation.
    /// Usually a SparkMessage contains one or more <see cref="SparkError"/>s.
    /// </summary>
    public class SparkError
    {
        /// <summary>
        /// Specific description of the error.
        /// </summary>
        public string Description;

        /// <summary>
        /// Builds an error from an API result.
        /// </summary>
        /// <param name="data">The serialised SparkError.</param>
        public SparkError(Dictionary<string, object> data)
        {
            Description = data["description"] as string;
        }
    }
}