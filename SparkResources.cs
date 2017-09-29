using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Cisco.Spark
{
    /// <summary>
    /// The different Spark objects supported by the SDK.
    /// </summary>
    public enum SparkType
    {
        Membership,
        Room,
        Person,
        Team,
        TeamMembership,
        Message,
        Webhook,
        Unsupported
    }

    /// <summary>
    /// Extensions for SparkType.
    /// </summary>
    public static class SparkTypeExtensions
    {
        /// <summary>
        /// Returns the API Resource Name for this SparkType.
        /// </summary>
        /// <param name="type">The SparkType.</param>
        /// <returns>URL Endpoint string.</returns>
        public static string GetEndpoint(this SparkType type)
        {
            switch (type)
            {
                case SparkType.Membership:
                    return "memberships";
                case SparkType.Room:
                    return "rooms";
                case SparkType.Person:
                    return "people";
                case SparkType.Team:
                    return "teams";
                case SparkType.TeamMembership:
                    return "team/memberships";
                case SparkType.Message:
                    return "messages";
                case SparkType.Webhook:
                    return "webhooks";
                default:
                    throw new System.Exception("SparkType must have a registered endpoint");
            }
        }

        /// <summary>
        /// Returns the SparkType a URL endpoint represents.
        /// </summary>
        /// <param name="endpoint">The given URL endpoint resource.</param>
        /// <returns>The SparkType.</returns>
        public static SparkType FromEndpoint(this string endpoint)
        {
            switch (endpoint)
            {
                case "memberships":
                    return SparkType.Membership;
                case "rooms":
                    return SparkType.Room;
                case "people":
                    return SparkType.Person;
                case "teams":
                    return SparkType.Team;
                case "team/memberships":
                    return SparkType.TeamMembership;
                case "messages":
                    return SparkType.Message;
                case "webhooks":
                    return SparkType.Webhook;
                default:
                    throw new System.NotSupportedException("URL Endpoint " + endpoint + " does not have a supported SparkType.");
            }
        }
    }

    /// <summary>
    /// Holds specific data pertaining to the operations and URL endpoints
    /// supported by the Spark web service.
    /// </summary>
    public class SparkResources : MonoBehaviour
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static SparkResources Instance;

        /// <summary>
        /// Defines which fields each API endpoints can support for Create and Update operations.
        /// </summary>
        public Dictionary<string, object> ApiConstraints;

        /// <summary>
        /// Awake runs as soon as possible and before any Start call.
        /// </summary>
        void Awake()
        {
            // Singleton.
            Instance = this;

            // Load API Constraints from resource file.
            var contents = Resources.Load("ApiConstraints") as TextAsset;
            ApiConstraints = Json.Deserialize(contents.text) as Dictionary<string, object>;
            ApiConstraints = ApiConstraints["apiConstraints"] as Dictionary<string, object>;
        }
    }
}
