using System.Collections.Generic;
using UnityEngine;

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
		/// Mapping of SparkType for single operation URL enpoints.
		/// </summary>
		public Dictionary<SparkType, string> Single;

		/// <summary>
		/// Mapping of SparkType for single operation URL enpoints.
		/// </summary>
		public Dictionary<SparkType, string> Multiple;
	
		void Awake() {
			// Singleton.
			Instance = this;

			Multiple = new Dictionary<SparkType, string> {
				{ SparkType.Membership, "memberships" },
				{ SparkType.Room, "rooms" },
				{ SparkType.Person, "people" },
				{ SparkType.Team, "teams" },
				{ SparkType.TeamMembership, "team/memberships" },
				{ SparkType.Message, "messages" },
				{ SparkType.Webhook, "webhooks" },
			};
		}
	}
}