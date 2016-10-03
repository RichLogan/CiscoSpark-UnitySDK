using UnityEngine;
using Cisco.Spark;
using System.Collections.Generic;
	
public class Spark : MonoBehaviour {

	// EVAR Bot Token: ODNkYzNiOTItMThiMC00OWVjLTgwYTEtYTk2N2E3MDQzNzZjZTdiY2ZiM2QtYjk4
	// EVAR Team ID: Y2lzY29zcGFyazovL3VzL1RFQU0vYjY5YmJiOTAtMWNkNS0xMWU2LTlhMjEtOWRhMDVmNWZjNGNk

	void Start() {
//		StartCoroutine (Membership.ListMemberships (memberships => {
//			foreach (Membership membership in memberships) {
//			}
//		}));

//		StartCoroutine (Membership.GetMembershipDetails (membership => {
//			Debug.Log(membership.Id);
//		}, "Y2lzY29zcGFyazovL3VzL01FTUJFUlNISVAvMjY1YTA3OWQtMTg5MC00Y2VmLTg2MzQtNzY4YTllOGNmZjA5OjI4MjkyMmEwLTUzZmItMTFlNi04ZmZjLWFiZjAzOTA5MmQzOQ"));

//		StartCoroutine (Person.GetPersonDetails ("Y2lzY29zcGFyazovL3VzL1BFT1BMRS8wOTcyMmZhNS1lMmI2LTRhM2YtYjc3Ni1hZDZmMzcyZDY4Mjc", person => {
//			foreach (string email in person.Emails) {
//				Debug.Log(email);
//			}
//		}));

//		StartCoroutine (Message.ListMessages ("Y2lzY29zcGFyazovL3VzL1JPT00vMzFhOTVkYTAtZjgwYi0xMWU1LWIyMjgtNTk1Mjc3YjMwNDli", messages => {
//			foreach (Message message in messages) {
//				if (message.Files != null) {
//					foreach (SparkFile file in message.Files) {
//						StartCoroutine(file.Download (callback => {
//							if (file.returnType == typeof(UnityEngine.Texture2D)) {
//								GameObject test = GameObject.CreatePrimitive (PrimitiveType.Cube);
//								test.GetComponent<Renderer>().material.mainTexture = callback as Texture2D;
//							}
//						}));
//					}
//				}
//			}
//		}));
	}
}