using UnityEngine;
using Cisco.Spark;

public class TestWebhook : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var errorCount = 0;

		// List Webhooks
		StartCoroutine (Webhook.ListWebhooks (listWebhookError => {
			errorCount++;
			Debug.LogError ("List Webhooks failed: " + listWebhookError.Message);
		}, webhooks => {
			// List Webhooks Passed
			var newWebhook = new Webhook (
				"testingWebhook",
				"http://example.org",
				"messages",
				"created",
				"86dacc007724d8ea666f88fc77d918dad9537a15"
			);
			StartCoroutine (newWebhook.Commit (commitError => {
				errorCount++;
				Debug.LogError("Create Webhook failed: " + commitError.Message);
			}, commitedWebhook => {
				newWebhook = commitedWebhook;
				if (newWebhook.Id == null) {
					Debug.LogError("ID wasn't set. This shouldn't happen.");
				} else {
					// Create Passed
					Debug.Log("Create Webhook Passed");
					newWebhook.Name = "testingWebhookUpdated";
					StartCoroutine (newWebhook.Commit (updateError => {
						errorCount++;
						Debug.Log("Update Webhook failed: " + updateError.Message);
					}, updatedWebhook => {
						newWebhook = updatedWebhook;
						if (newWebhook.Name != "testingWebhookUpdated") {
							errorCount++;
							Debug.LogError("Couldn't update Webhook Name");
						} else {
							// Update Passed
							Debug.Log("Update Webhook Passed");

							StartCoroutine (Webhook.GetWebhookDetails (newWebhook.Id, detailsError => {
								errorCount++;
								Debug.LogError("Couldn't get Webhook details: " + detailsError.Message);
							}, webhook => {
								if (newWebhook.Id != webhook.Id) {
									errorCount++;
									Debug.LogError ("Retrieve webhook ID doesn't match");
								} else {
									// Details Passed
									StartCoroutine (newWebhook.Delete (deleteError => {
										errorCount++;
										Debug.LogError ("Delete Webhook failed: " + deleteError.Message);
									}, delete => {
										// Finish and Report
										if (errorCount == 0) {
											Debug.Log("All tests passed");
										} else {
											Debug.LogError(errorCount + " tests failed");
										}
									}));
								}
							}));
						}
					}));
				}
			}));
		}));
	}
}
