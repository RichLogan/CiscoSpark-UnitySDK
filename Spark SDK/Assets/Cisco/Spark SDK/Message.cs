using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Message {
		private string id;
		private string roomId;
		private string personId;
		private string personEmail;
		private string text;
		private string file;
		private Uri[] files;

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

		public string getPersonEmail() {
			return personEmail;
		}

		public void setPersonEmail(string personEmail) {
			this.personEmail = personEmail;
		}

		public string getText() {
			return text;
		}

		public void setText(string text) {
			this.text = text;
		}

		public string getFile() {
			return file;
		}

		public void setFile(string file) {
			this.file = file;
		}

		public Uri[] getFiles() {
			return files;
		}

		public void setFiles(Uri[] files) {
			this.files = files;
		}
	}
}