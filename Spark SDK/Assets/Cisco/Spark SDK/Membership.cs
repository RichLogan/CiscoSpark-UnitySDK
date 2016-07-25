using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Membership : ScriptableObject {
		private string id;
		private string roomId;
		private string personId;
		private string personDisplayName;
		private string personEmail;
		private bool isModerator;
		private bool isMonitor;
		private DateTime created;

		public string getId() {
			return id;
		}

		public void setId(string id) {
			this.id = id;
		}

		public string getRoomId() {
			return roomId;
		}

		public void setRoomId(string roomId) {
			this.roomId = roomId;
		}

		public string getPersonId() {
			return personId;
		}

		public void setPersonId(string personId) {
			this.personId = personId;
		}

		public string getPersonDisplayName() {
			return personDisplayName;
		}

		public void setPersonDisplayName(string personDisplayName) {
			this.personDisplayName = personDisplayName;
		}

		public String getPersonEmail() {
			return personEmail;
		}

		public void setPersonEmail(String personEmail) {
			this.personEmail = personEmail;
		}

		public bool getIsModerator() {
			return isModerator;
		}

		public void setIsModerator(bool isModerator) {
			this.isModerator = isModerator;
		}

		public bool getIsMonitor() {
			return isMonitor;
		}

		public void setIsMonitor(bool isMonitor) {
			this.isMonitor = isMonitor;
		}

		public DateTime getCreated() {
			return created;
		}

		public void setCreated(DateTime created) {
			this.created = created;
		}
	}
}