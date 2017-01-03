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

        internal override SparkType SparkType
        {
            get
            {
                return SparkType.Membership;
            }
        }

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
		/// Returns a dictionary representation of the object.
		/// </summary>
		/// <returns>The Dictionary.</returns>
		/// <param name="fields">A specific list of fields to serialise.</param>
		protected override Dictionary<string, object> ToDict (List<string> fields = null)
		{
			// Serialise to dictionary.
			var data = base.ToDict();
			data ["roomId"] = Room.Id;
			data ["personId"] = Person.Id;
			data ["isModerator"] = IsModerator;
			return CleanDict(data, fields);
		}

		protected override void LoadDict (Dictionary<string, object> data)
		{
			base.LoadDict(data);
			var roomId = data ["roomId"] as string;
			Room = new Room(roomId);
			var personId = data ["personId"] as string;
			Person = new Person(personId);
			// Person.Emails = result ["personEmail"] as List<string>;
			// Person.DisplayName = result ["personDisplayName"] as string;
			IsModerator = (bool) data ["isModerator"];
			IsMonitor = (bool) data ["isMonitor"];
		}
	}
}
