using UnityEngine;
using System;

namespace Cisco.Spark {
	public class Webhook : ScriptableObject {
		private string id;
		private string webhookName;
		private string resource;
		private string eventType;
		private string filter;
		private Uri targetUrl;
		private DateTime created;

		public string getId() {
			return id;
		}

		public void setId(string id) {
			this.id = id;
		}

		public string getName() {
			return webhookName;
		}

		public void setName(string webhookName) {
			this.webhookName = webhookName;
		}

		public string getResource() {
			return resource;
		}

		public void setResource(string resource) {
			this.resource = resource;
		}

		public string getEvent() {
			return eventType;
		}

		public void setEvent(string eventType) {
			this.eventType = eventType;
		}

		public String getFilter() {
			return filter;
		}

		public void setFilter(String filter) {
			this.filter = filter;
		}

		public Uri getTargetUrl() {
			return targetUrl;
		}

		public void setTargetUrl(Uri targetUrl) {
			this.targetUrl = targetUrl;
		}

		public DateTime getCreated() {
			return created;
		}

		public void setCreated(DateTime created) {
			this.created = created;
		}
	}
}