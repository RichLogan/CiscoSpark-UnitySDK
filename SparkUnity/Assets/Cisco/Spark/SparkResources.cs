using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.IO;

namespace Cisco.Spark {

	public enum SparkType {
		Membership,
		Room,
		Person,
		Team,
		TeamMembership,
		Message,
		Webhook
	}

	public class SparkResources : MonoBehaviour {

		/// <summary>
		/// Singleton Instance.
		/// </summary>
		public static SparkResources Instance;

		/// <summary>
		/// Mapping of SparkType to URL endpoints.
		/// </summary>
		public Dictionary<SparkType, string> UrlEndpoints;

		public Dictionary<string, object> ApiConstraints;
	
		void Awake() {
			// Singleton.
			Instance = this;

			// Spark Type URL Endpoints.
			UrlEndpoints = new Dictionary<SparkType, string> {
				{ SparkType.Membership, "memberships" },
				{ SparkType.Room, "rooms" },
				{ SparkType.Person, "people" },
				{ SparkType.Team, "teams" },
				{ SparkType.TeamMembership, "team/memberships" },
				{ SparkType.Message, "messages" },
				{ SparkType.Webhook, "webhooks" },
			};

			// Load API Constraints from resource file.
			string contents;
			using (var streamReader = new StreamReader("Assets/Cisco/Spark/ApiConstraints.json")) {
				contents = streamReader.ReadToEnd();
			}
			ApiConstraints = Json.Deserialize(contents) as Dictionary<string, object>;
			ApiConstraints = ApiConstraints["apiConstraints"] as Dictionary<string, object>;
		}
	}
}