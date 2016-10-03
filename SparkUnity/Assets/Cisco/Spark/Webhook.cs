using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Webhook {
		public string Id { get; set; }
		public string Name { get; set; }
		public string TargetUrl { get; set;}
		public string Resource { get; set; }
		public string Event { get; set; }
		public string Filter { get; set; }
		public string Secret { get; set; }
		public DateTime Created { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Webhook"/> class from a API JSON representation.*/
		/// </summary>
		/// <param name="json">The API returned JSON string</param>
		public Webhook(string json) {
			var data = Json.Deserialize (json) as Dictionary<string, object>;
			Id = (string)data ["id"];
			Name = (string)data ["name"];
			TargetUrl = (string)data ["targetUrl"];
			Resource = (string)data ["resource"];
			Event = (string)data ["event"];
			Filter = (string)data ["filter"];
			Created = DateTime.Parse ((string)data ["created"]);

			// Secret may not always be retrieved
			object secret;
			if (data.TryGetValue ("secret", out secret)) {
				Secret = (string)data ["secret"];	
			}
		}
			
		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Webhook"/> class locally.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="targetUrl">Target URL.</param>
		/// <param name="resource">Resource.</param>
		/// <param name="event">Event.</param>
		/// <param name="secret">Secret.</param>
		/// <param name="filter">Filter.</param>
		public Webhook(string name, string targetUrl, string resource, string @event, string secret, string filter = null) {
			Name = name;
			TargetUrl = targetUrl;
			Resource = resource;
			Event = @event;
			Secret = secret;
			Filter = filter;
		}

		/// <summary>
		/// Commits the current state of the local Webhook object to Spark.
		/// This will create a new webhook if it doesn't exist. 
		/// </summary>
		/// <param name="callback">The created/updated Webhook from Spark</param>
		public IEnumerator Commit(Action<Webhook> callback) {
			// Setup request from current state of Webhook object
			var manager = GameObject.FindObjectOfType<Request> ();

			// Webhook Data
			var data = new Dictionary<string, string> ();
			data ["name"] = Name;
			data ["targetUrl"] = TargetUrl;
			data ["resource"] = Resource;
			data ["event"] = Event;
			data ["secret"] = Secret;
			// Filter is optional
			if (Filter != null) {
				data ["filter"] = Filter;	
			}

			// Create or Update?
			string resource;
			string httpVerb;
			if (Id == null) {
				// Creating a new webhook
				resource = "webhooks";
				httpVerb = UnityWebRequest.kHttpVerbPOST;
			} else {
				// Updating a previous webhook
				// Only changing name and targetUrl is currently
				// supported
				data.Remove ("resource");
				data.Remove ("event");
				data.Remove ("secret");
				data.Remove ("filter");
				resource = "webhooks/" + Id;
				httpVerb = UnityWebRequest.kHttpVerbPUT;
			}

			// Make request
			using (UnityWebRequest www = manager.Generate(resource, httpVerb)) {
				byte[] raw_data = System.Text.Encoding.UTF8.GetBytes (Json.Serialize (data));
				www.uploadHandler = new UploadHandlerRaw (raw_data);
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError("Failed to Create/Update Webhook: " + www.error);
				} else {
					var webhook = new Webhook (www.downloadHandler.text);
					callback (webhook);
				}
			}
		}

		/// <summary>
		/// Delete this Webhook on Spark.
		/// </summary>
		public IEnumerator Delete() {
			if (Id != null) {
				var manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("webhooks/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						Debug.LogError ("Failed to Delete Webhook: " + www.error);
					}
				}
			}
		}
			
		public static IEnumerator ListWebhooks(Action<List<Webhook>> callback, string teamId = null, int max = 0, string type = null) {
			// Build Request
			var manager = GameObject.FindObjectOfType<Request> ();
			var data = new Dictionary<string, string> ();

			// Handle optional arguments
			if (teamId != null) {
				data ["teamId"] = teamId;
			}
			if (max != 0) {
				data ["max"] = max.ToString ();	
			}
			if (type != null) {
				data ["type"] = type;	
			}

			// Make Request
			string queryString = System.Text.Encoding.UTF8.GetString (UnityWebRequest.SerializeSimpleForm (data));
			using (UnityWebRequest www = manager.Generate ("webhooks?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to List Webhooks: " + www.error);
				} else {
					// Convert to Webhook objects
					var webhooks = new List<Webhook> ();
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					var items = json ["items"] as List<object>;
					foreach (Dictionary<string, object> webhook_json in items) {
						// TODO: Do I need to reconvert this?
						string reJsoned = Json.Serialize (webhook_json);
						webhooks.Add(new Webhook (reJsoned));
					}
					callback (webhooks);
				}
			}
		}

		/// <summary>
		/// Gets the webhook details.
		/// </summary>
		/// <returns>The webhook details.</returns>
		/// <param name="webhookId">Webhook identifier.</param>
		/// <param name="callback">Callback.</param>
		public static IEnumerator GetWebhookDetails(string webhookId, Action<Webhook> callback) {
			var manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("webhooks/" + webhookId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Retrieve Webhook: " + www.error);
				} else {
					callback(new Webhook(www.downloadHandler.text));
				}
			}
		}
	}
}
