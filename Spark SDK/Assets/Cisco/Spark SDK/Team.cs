using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Team {
		private string id;
		private string name;
		private DateTime created;

		public string getName() {
			return name;
		}

		public void setName(string name) {
			this.name = name;
		}

		public string getId() {
			return id;
		}

		public void setId(string id) {
			this.id = id;
		}

		public DateTime getCreated() {
			return created;
		}

		public void setCreated(DateTime created) {
			this.created = created;
		}
	}
}