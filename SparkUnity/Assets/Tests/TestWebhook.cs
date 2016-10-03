using UnityEngine;
using System.Collections;
using Cisco.Spark;

public class TestWebhook : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine (Webhook.ListWebhooks (webhooks => {
			
		}));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
