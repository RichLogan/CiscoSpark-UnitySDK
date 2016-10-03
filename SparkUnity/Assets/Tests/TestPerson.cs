using UnityEngine;
using Cisco.Spark;

public class TestPerson : MonoBehaviour {

	void Start() {
		// Error Count
		var errorCount = 0;
		StartCoroutine (Person.ListPeople (people => {
			if (people.Count != 1) {
				errorCount++;
				Debug.LogError("rilogan@cisco.com should return 1 record");
			}
				
			if (people[0].DisplayName != "Rich Logan") {
				errorCount++;
				Debug.Log("List People not returning details correctly");
			}

			StartCoroutine (Person.GetPersonDetails (people[0].Id, person => {
				if (person.DisplayName != "Rich Logan") {
					errorCount++;
					Debug.Log("Display name not retrieved correctly");
				}

				StartCoroutine (Person.GetPersonDetails (callback => {
					Debug.Log("Authenticated user is: " + callback.DisplayName);

					StartCoroutine (person.DownloadAvatar (texture => {
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
