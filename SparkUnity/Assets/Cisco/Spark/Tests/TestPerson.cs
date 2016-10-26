using UnityEngine;
using Cisco.Spark;

public class TestPerson : MonoBehaviour {

	void Start() {
		// Error Count
		var errorCount = 0;
		StartCoroutine (Person.ListPeople (listPeopleError => {
			if (listPeopleError != null) {
				errorCount++;
				Debug.LogError("List people failed: " + listPeopleError.Message);
			}
		}, people => {
			if (people.Count != 1) {
				errorCount++;
				Debug.LogError("rilogan@cisco.com should return 1 record");
			}
				
			if (people[0].DisplayName != "Rich Logan") {
				errorCount++;
				Debug.Log("List People not returning details correctly");
			}

			StartCoroutine (Person.GetPersonDetails (people[0].Id, getPersonError => {
				if (getPersonError != null) {
					errorCount++;
					Debug.LogError("Get Person Details failed: " + getPersonError.Message);
				}
			},person => {
				if (person.DisplayName != "Rich Logan") {
					errorCount++;
					Debug.Log("Display name not retrieved correctly");
				}

				StartCoroutine (Person.GetPersonDetails (getPersonAgain => {
					if (getPersonAgain != null) {
						errorCount++;
						Debug.LogError("Get Person Details failed: " + getPersonAgain.Message);
					}
				}, callback => {
					Debug.Log("Authenticated user is: " + callback.DisplayName);

					StartCoroutine (person.DownloadAvatar (avatarError => {
						if (avatarError != null) {
							errorCount++;
							Debug.LogError("List people failed: " + avatarError.Message);
						}
					}, texture => {
						if (texture.width <= 0) {
							Debug.Log("Couldn't download user's avatar");
							errorCount++;
						}

						// Finish
						if (errorCount > 0) {
							Debug.LogError("Some tests failed");
						} else {
							Debug.Log ("All tests passed");
						}
					}));
				}));
			}));
		}, "rilogan@cisco.com"));
	}
}
