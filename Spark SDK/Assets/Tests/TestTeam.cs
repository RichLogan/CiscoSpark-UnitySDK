using UnityEngine;
using Cisco.Spark;

public class TestTeam : MonoBehaviour {
	void Start () {
		var errorCount = 0;
		// List Teams
		StartCoroutine (Team.ListTeams (teams => {
			if (teams.Count > 0) {
				Debug.Log ("ListTeams passed");
			} else {
				errorCount++;
				Debug.LogError ("ListTeams failed");
			}

			// Get Team Details
			StartCoroutine (Team.GetTeamDetails (teams[0].Id, retrievedTeam => {
				if (retrievedTeam.Name != teams[0].Name) {
					errorCount++;
					Debug.LogError("GetTeamDetails failed");
				} else {
					Debug.Log("GetTeamDetails passed");
				}

				// Create Team
				const string OriginalName = "Unity SDK Test Team";
				Team t = new Team(OriginalName);
				StartCoroutine (t.Commit (createdTeam => {
					t = createdTeam;
					if (t.Id == null) {
						errorCount++;
						Debug.LogError ("Create Team failed");
					} else {
						Debug.Log("Create Team passed");
					}

					// Update Team
					const string NewName = OriginalName + "RENAMED";
					t.Name = NewName;
					StartCoroutine (t.Commit (updatedTeam => {
						t = updatedTeam;
						if (t.Name != NewName) {
							errorCount++;
							Debug.LogError ("Update Team failed");
						} else {
							Debug.Log("Update Team passed");
						}

						// Delete Team
						StartCoroutine (t.Delete ());

						// Error Report
						if (errorCount > 0) {
							Debug.Log(errorCount + " Team tests failed");
						} else {
							Debug.Log("All Team tests passed");
						}
					}));
				}));
			}));
		}));
	}
}
