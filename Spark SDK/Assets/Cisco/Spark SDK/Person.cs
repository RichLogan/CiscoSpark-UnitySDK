using UnityEngine;

namespace Cisco.Spark {
	public class Person : ScriptableObject {
		private string id;
		private string displayName;
		private string[] emails;

		public string getId() {
			return id;
		}

		public void setId(string id) {
			this.id = id;
		}

		public string getDisplayName() {
			return displayName;
		}

		public void setDisplayName(string displayName) {
			this.displayName = displayName;
		}

		public string[] getEmails() {
			return emails;
		}

		public void setEmails(string[] emails) {
			this.emails = emails;
		}
	}
}