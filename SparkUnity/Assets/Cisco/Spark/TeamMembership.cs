using UnityEngine;
using System;

namespace Cisco.Spark {
	public class TeamMembership : ScriptableObject {
		string Id { get; set;}
		string TeamId { get; set;}
		string PersonId { get; set;}
		string PersonDisplayName { get; set;}
		string PersonEmail { get; set;}
		bool IsModerator { get; set;}
		DateTime Created { get; set;}
	}
}