using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
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
        Webhook
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
                    return null;
            }
        }
    }

    public class SparkResources : MonoBehaviour
    {
        /// <summary>
        /// Singleton Instance.
        /// </summary>
        public static SparkResources Instance;

        /// <summary>
        /// Defines what fields each API endpoints can support for Create and Update operations.
        /// </summary>
        public Dictionary<string, object> ApiConstraints;

        void Awake()
        {
            // Singleton.
            Instance = this;

            // Load API Constraints from resource file.
            string contents;
            using (var streamReader = new StreamReader("Assets/Cisco/Spark/ApiConstraints.json"))
            {
                contents = streamReader.ReadToEnd();
            }
            ApiConstraints = Json.Deserialize(contents) as Dictionary<string, object>;
            ApiConstraints = ApiConstraints["apiConstraints"] as Dictionary<string, object>;
        }
    }
}