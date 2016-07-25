using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Webhook : ScriptableObject {
		string id { get; set;}
		string webhookName { get; set;}
		string resource { get; set;}
		string eventType { get; set;}
		string filter { get; set;}
		Uri targetUrl { get; set;}
		DateTime created { get; set;}
	}
}