using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class Webhook {
		public string Id { get; private set; }
		public string Name { get; set; }
		public string TargetUrl { get; set;}
		public string Resource { get; set; }
		public string Event { get; set; }
		public string Filter { get; set; }
		public string OrgId { get; set; }
		public string CreatedBy { get; set; }
		public string AppId { get; set; }
		public string Secret { get; set; }
		public DateTime Created { get; private set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="Cisco.Spark.Webhook"/> class from a API JSON representation.*/
		/// </summary>
		/// <param name="data">The API returned Webhook data</param>
		public Webhook(Dictionary<string, object> data) {
			Id = (string)data ["id"];
			Name = (string)data ["name"];
			TargetUrl = (string)data ["targetUrl"];
			Resource = (string)data ["resource"];
			Event = (string)data ["event"];
			OrgId = (string)data ["orgId"];
			CreatedBy = (string)data ["createdBy"];
			AppId = (string)data ["appId"];
			Created = DateTime.Parse ((string)data ["created"]);

			// Filter may not always be retrieved
			object filter;
			if (data.TryGetValue ("filter", out filter)) {
				Filter = (string)data ["filter"];
			}

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
		/// <param name="error">The error or message from Spark (if any)</param>
		/// <param name="result">The created/updated Webhook from Spark (if any)</param>
		public IEnumerator Commit(Action<SparkMessage> error, Action<Webhook> result) {
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
					Debug.LogError("Failed to Create Webhook: " + www.error);
				} else {
					// Parse Response
					var webhookData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (webhookData.ContainsKey ("message")) {
						// Spark Error
						error (new SparkMessage (webhookData));
						result (null);
					} else {
						// Create local Membership object
						error(null);
						result(new Webhook (webhookData));
					}
				}
			}
		}

		/// <summary>
		/// Delete this Webhook on Spark.
		/// </summary>
		public IEnumerator Delete(Action<SparkMessage> error, Action<bool> result) {
			if (Id != null) {
				var manager = GameObject.FindObjectOfType<Request> ();
				using (UnityWebRequest www = manager.Generate ("webhooks/" + Id, UnityWebRequest.kHttpVerbDELETE)) {
					yield return www.Send ();
					if (www.isError) {
						// Network Error
						Debug.LogError ("Failed to Delete Webhook: " + www.error);
					} else {
						// Delete returns 204 on success
						if (www.responseCode == 204) {
							error (null);
							result (true);
						} else {
							// Delete Failed
							var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
							error (new SparkMessage (json));
							result (false);
						}
					}
				}
			}
		}
			
		public static IEnumerator ListWebhooks(Action<SparkMessage> error, Action<List<Webhook>> result, string teamId = null, int max = 0, string type = null) {
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

			// Optional Parameters to URL query
			string queryString = System.Text.Encoding.UTF8.GetString (UnityWebRequest.SerializeSimpleForm (data));

			// Make Request
			using (UnityWebRequest www = manager.Generate ("webhooks?" + queryString, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError("Failed to List Webhooks: " + www.error);
				} else {
					// Request succeeded, parse response
					var json = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;

					// Check for Spark side errors
					if (json.ContainsKey ("message")) {
						error (new SparkMessage (json));
						result (null);
					} else {
						// Convert to Membership objects
						var webhooks = new List<Webhook> ();
						var items = json ["items"] as List<object>;
						foreach (var webhook in items) {
							webhooks.Add (new Webhook (webhook as Dictionary<string, object>));
						}
						result (webhooks);
						error (null);
					}
				}
			}
		}

		/// <summary>
		/// Gets the webhook details.
		/// </summary>
		/// <returns>The webhook details.</returns>
		/// <param name="webhookId">Webhook identifier.</param>
		/// <param name="error">Error from Spark if any.</param>
		/// <param name="result">Result Callback.</param>
		public static IEnumerator GetWebhookDetails(string webhookId, Action<SparkMessage> error, Action<Webhook> result) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("webhooks/" + webhookId, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();

				if (www.isError) {
					// Network error
					Debug.LogError (www.error);
				} else {
					// Parse Response
					var webhookData = Json.Deserialize (www.downloadHandler.text) as Dictionary<string, object>;
					if (webhookData.ContainsKey ("message")) {
						// Error Callback
						error (new SparkMessage (webhookData));
						result(null);
					} else {
						// Result callback
						error (null);
						result(new Webhook (webhookData));
					}
				}
			}
		}
	}
}
