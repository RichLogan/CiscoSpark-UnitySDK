using System.Collections.Generic;
using UnityEngine.Networking;

namespace Cisco.Spark
{
    /// <summary>
    /// A message Spark returns in response to an operation.
    /// This is usually a wrapping of a SparkError.
    /// </summary>
    public class SparkMessage
    {
        /// <summary>
        /// The specific message from Spark.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// A collection of one or more <see cref="SparkError"/>s.
        /// </summary>
        public IEnumerable<SparkError> Errors { get; private set; }

        /// <summary>
        /// Spark's internal tracking ID for this message.
        /// </summary>
        public string TrackingId { get; private set; }

        /// <summary>
        /// The associated web request.
        /// </summary>
        public UnityWebRequest WebRequest {get; private set; }

        /// <summary>
        /// Creates a SparkMessage from a JSON Spark response.
        /// </summary>
        /// <param name="data">Dictionary of data about the SparkMessage.</param>
        public SparkMessage(Dictionary<string, object> data, UnityWebRequest request)
        {
            WebRequest = request;
            Message = (string)data["message"];
            Errors = new List<SparkError>();

            object errors;
            if (data.TryGetValue("errors", out errors))
            {
                var listOfErrors = errors as List<object>;
                var errorList = new List<SparkError>();
                foreach (var error in listOfErrors)
                {
                    errorList.Add(new SparkError(error as Dictionary<string, object>));
                }
                Errors = errorList;
            }
            TrackingId = (string)data["trackingId"];
        }

        /// <summary>
        /// Creates a SparkMessage object from a UnityWebRequest.
        /// </summary>
        /// <param name="statusCode">UnityWebRequest.</param>
        public SparkMessage(UnityWebRequest request) {
            WebRequest = request;
        }
    }
}