using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Membership : ScriptableObject {
		string Id { get; set;}
		string RoomId { get; set;}
		string PersonId { get; set;}
		string PersonDisplayName { get; set;}
		string PersonEmail { get; set;}
		bool IsModerator { get; set;}
		bool IsMonitor { get; set;}
		DateTime Created { get; set;}
	}
}