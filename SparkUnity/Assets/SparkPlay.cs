using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cisco.Spark;

public class SparkPlay : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var mem = new Membership ("Y2lzY29zcGFyazovL3VzL01FTUJFUlNISVAvMDk3MjJmYTUtZTJiNi00YTNmLWI3NzYtYWQ2ZjM3MmQ2ODI3OjAwZjcyNmYwLTY4ZGMtMTFlNi1iMzBlLWQ3OWQ0MjhlODA4Nw");
		StartCoroutine (mem.Load (error => {
			Debug.LogError("Failed");
		}, success => {
			Debug.Log(mem.Person.Id);
		}));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
