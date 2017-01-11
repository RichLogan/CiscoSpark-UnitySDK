using UnityEngine;
using Cisco.Spark;
using System;

public class TestWebhook : MonoBehaviour
{
    Webhook webhook;

    void Start()
    {
        TestStart();
    }

    void TestStart()
    {
        CreateWebhook();
    }

    void CreateWebhook()
    {
        webhook = new Webhook("Testing Webhook", new Uri("http://example.org"), SparkType.Room, "created");
        StartCoroutine(webhook.Commit(error =>
        {
            throw new Exception("Failed to create Webhook: " + error.Message);
        }, success =>
        {
            GetWebhook();
        }));
    }

    void GetWebhook()
    {
        var getWebhook = new Webhook(webhook.Id);
        StartCoroutine(getWebhook.Load(error =>
        {
            throw new Exception("Failed to get Webhook: " + error.Message);
        }, success =>
        {
            Debug.Log(getWebhook.Name);
            if (getWebhook.Name.Equals("Testing Webhook"))
            {
                Debug.Log("Create Webhook Passed!");
                Debug.Log("Get Webhook Passed");
                UpdateWebhook();
            } else {
                throw new Exception("Failed to Get Webhook Name but operation shows successful: Raise an Issue!");                
            }
        }));
    }

    void UpdateWebhook()
    {
        var newName = "Testing Webhook - Updated";
        var targetUri = new Uri("http://google.com");
        webhook.Name = newName;
        webhook.Target = targetUri;

        // Update.
        StartCoroutine(webhook.Commit(error => {
            throw new Exception("Couldn't update Webhook: " + error.Message);
        }, success => {
            var checkUpdate = new Webhook(webhook.Id);
            StartCoroutine(checkUpdate.Load(error => {
                throw new Exception("Failed to get Updated Webhook: " + error.Message);
            }, updateSuccess => {
                if (checkUpdate.Name == newName) {
                    Debug.Log("Update Webhook Passed!");
                    ListWebhooks();
                } else {
                    throw new Exception("Failed to Get Updated Name but operation shows successful: Raise an Issue!");
                }
            }));
        }));
    }

    void ListWebhooks() {
        StartCoroutine(Webhook.ListWebhooks(error => {
            throw new Exception("Failed to List Webhooks: " + error.Message);
        }, webhooks => {
            var found = false;
            foreach (var wh in webhooks) {
                if (wh.Id == webhook.Id) {
                    Debug.Log("List Memberships Passed");
                    DeleteWebhook();
                    found = true;
                }
            }

            if (!found) {
                throw new Exception("List Memberships Failed!");
            }
        }));
    }

    void DeleteWebhook() {
        StartCoroutine(webhook.Delete(error => {
            throw new Exception("Failed to Delete Webhook: " + error.Message);
        }, success => {
            var checkDelete = new Webhook(webhook.Id);
            StartCoroutine(checkDelete.Load(error => {
                // This should error with a not found.
                if (error.Message.Equals("webhook not found")) {
                    Debug.Log("Delete Webhook Passed!");
                    TestEnd();
                } else {
                    throw new Exception("Failed to get Delete Webhook: " + error.Message);
                }
            }, updateSuccess => {
                throw new Exception("Managed to Get Deleted Webhook: Raise an Issue!");
            }));
        }));
    }

    void TestEnd() {
        Debug.Log("***TestWebhooks Finished**");
    }
}