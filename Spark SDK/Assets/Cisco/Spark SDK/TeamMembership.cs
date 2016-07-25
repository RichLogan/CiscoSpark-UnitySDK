using UnityEngine;
using System;

namespace Cisco.Spark {
	public class TeamMembership : ScriptableObject {
		private string id;
		private string teamId;
		private string personId;
		private string personDisplayName;
		private string personEmail;
		private bool isModerator;
		private DateTime created;

		public string getId() {
			return id;
		}

		public void setId(string id) {
			this.id = id;
		}

		public string getTeamId() {
			return teamId;
		}

		public void setTeamId(string teamId) {
			this.teamId = teamId;
		}

		public string getPersonId() {
			return personId;
		}

		public void setPersonId(string personId) {
			this.personId = personId;
		}

		public string getPersonEmail() {
			return personEmail;
		}

		public void setPersonEmail(string personEmail) {
			this.personEmail = personEmail;
		}

		public bool getIsModerator() {
			return isModerator;
		}

		public void setIsModerator(bool isModerator) {
			this.isModerator = isModerator;
		}

		public string getPersonDisplayName() {
			return personDisplayName;
		}

		public void setPersonDisplayName(string personDisplayName) {
			this.personDisplayName = personDisplayName;
		}

		public DateTime getCreated() {
			return created;
		}

		public void setCreated(DateTime created) {
			this.created = created;
		}
	}
}