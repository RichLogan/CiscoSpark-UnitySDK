using UnityEngine;

namespace Cisco.Spark {
	public class Person : ScriptableObject {
		string Id { get; set;}
		string DisplayName { get; set;}
		string[] Emails { get; set;}
	}
}