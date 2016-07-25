using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Room : ScriptableObject {
		private string id;
		private string title;
		private string teamId;
		private bool isLocked;
		private DateTime created;
		private DateTime lastActivity;

		public string getTitle() {
			return title;
		}

		public void setTitle(string title) {
			this.title = title;
		}

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

		public bool getIsLocked() {
			return isLocked;
		}

		public void setIsLocked(bool isLocked) {
			this.isLocked = isLocked;
		}

		public DateTime getCreated() {
			return created;
		}

		public void setCreated(DateTime created) {
			this.created = created;
		}

		public DateTime getLastActivity() {
			return lastActivity;
		}

		public void setLastActivity(DateTime lastActivity) {
			this.lastActivity = lastActivity;
		}
	}
}
