using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Message {
		string Id { get; set;}
		string RoomId { get; set;}
		string PersonId { get; set;}
		string PersonEmail { get; set;}
		string Text { get; set;}
		string File { get; set;}
		Uri[] Files { get; set;}
	}
}