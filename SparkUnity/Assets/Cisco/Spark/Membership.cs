using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark {
	public class Membership : SparkObject {
		public Room Room { get; set;}
		public Person Person {get; set;}
		public bool IsModerator { get; set;}
		public bool IsMonitor { get; set;}

		public Membership(string id) {
			Id = id;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Membership"/> class.
		/// </summary>
		/// <param name="room">The Room of the membership.</param>
		/// <param name="person">The person belonging to the membership.</param>
		/// <param name="isModerator">True if this member is a moderator.</param>
		/// /// <param name="isMonitor">True if this member is a monitor.</param>
		public Membership(Room room, Person person, bool isModerator=false, bool isMonitor=false) {
			Room = room;
			Person = person;
			IsModerator = isModerator;
			IsMonitor = isMonitor;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Membership"/> class.
		/// </summary>
		/// <param name="membershipData">Membership data.</param>
		Membership(Dictionary<string, object> membershipData) {
			try {
				Id = membershipData ["id"] as string;
				Room = new Room();
				Room.Id = membershipData ["roomId"] as string;
				Person = new Person();
				Person.Id = membershipData ["personId"] as string;
//				Person.Emails.Add (membershipData ["personEmail"] as string);
//				Person.DisplayName = membershipData ["personDisplayName"] as string;
				IsModerator = (bool) membershipData ["isModerator"];
				IsMonitor = (bool) membershipData ["isMonitor"];
				Created = DateTime.Parse ((string) membershipData ["created"]);
			} catch (KeyNotFoundException) {
				Debug.Log ("Couldn't parse Membership");
			}
		}

		public override IEnumerator Load (Action<SparkMessage> error, Action<bool> success)
		{
			if (Id != null) {
				var getRecordRoutine = Request.Instance.GetRecord (Id, SparkType.Membership, error, result => {
					LoadDict (result);
					success (true);
				});
				yield return Request.Instance.StartCoroutine (getRecordRoutine);
			} else {
				Debug.LogError ("An ID must be set in order to populate the object.");
			}
		}

		/// <summary>
		/// Commits the object to the Spark service.
		/// This will create the object if it doesn't already exist.
		/// </summary>
		/// <param name="error">Error from Spark, if any.</param>
		/// <param name="success">Callback for completion.</param>
		public override IEnumerator Commit (Action<SparkMessage> error, Action<bool> success)
		{
			if (Id == null) {
				// Create a new record.
				var keys = new List<string>{"roomId", "personId", "isModerator"};
				var createRoutine = Request.Instance.CreateRecord (ToDict (keys), SparkType.Membership, error, result => LoadDict (result));
				yield return Request.Instance.StartCoroutine (createRoutine);
				success (true);
			} else {
				// Update an existing record.
				var keys = new List<string>{ "isModerator" };
			}
		}

		/// <summary>
		/// Deletes the object from the Spark service.
		/// </summary>
		/// <param name="error">Error from Spark, if any.</param>
		/// <param name="success">Callback for completion.</param>
		public override IEnumerator Delete (Action<SparkMessage> error, Action<bool> success)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Returns a dictionary representation of the object.
		/// </summary>
		/// <returns>The Dictionary.</returns>
		/// <param name="fields">A specific list of fields to serialise.</param>
		protected override Dictionary<string, object> ToDict (List<string> fields = null)
		{
			// Serialise to dictionary.
			var data = new Dictionary<string, object> ();
			data ["id"] = Id;
			data ["roomId"] = Room.Id;
			data ["personId"] = Person.Id;
			data ["isModerator"] = IsModerator;
			data ["created"] = Created;

			// Only return asked for keys.
			if (fields != null) {
				foreach (var key in data.Keys) {
					if (!fields.Contains (key)) {
						data.Remove (key);
					}
				}
			}
			return data;
		}

		protected override void LoadDict (Dictionary<string, object> data)
		{
			Id = data ["id"] as string;
			Room = new Room();
			Room.Id = data ["roomId"] as string;
			Person = new Person();
			Person.Id = data ["personId"] as string;
			// Person.Emails = result ["personEmail"] as List<string>;
			// Person.DisplayName = result ["personDisplayName"] as string;
			IsModerator = (bool) data ["isModerator"];
			IsMonitor = (bool) data ["isMonitor"];
			Created = DateTime.Parse ((string) data ["created"]);
		}
	}
}
