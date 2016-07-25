using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Room : ScriptableObject {
		string Id { get; set; }
		string Title { get; set; }
		string TeamId { get; set; }
		bool IsLocked { get; set; }
		DateTime Created { get; set; }
		DateTime LastActivity { get; set; }
	}
}
