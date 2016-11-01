using UnityEngine;
using Cisco.Spark;

public class TestTeam : MonoBehaviour {
	void Start () {
		var errorCount = 0;
		// List Teams
		StartCoroutine (Team.ListTeams (listError => {
			errorCount++;
			Debug.LogError ("List Teams failed: " + listError.Message);
		}, teams => {
			if (teams.Count > 0) {
				Debug.Log ("ListTeams passed");
			} else {
				errorCount++;
				Debug.LogError ("ListTeams failed");
			}

			// Get Team Details
			StartCoroutine (Team.GetTeamDetails (teams[0].Id, detailsError => {
				errorCount++;
				Debug.LogError ("List Teams failed: " + detailsError.Message);
			}, retrievedTeam => {
				if (retrievedTeam.Name != teams[0].Name) {
					errorCount++;
					Debug.LogError("GetTeamDetails failed");
				} else {
					Debug.Log("GetTeamDetails passed");
				}

				// Create Team
				const string OriginalName = "Unity SDK Test Team";
				var t = new Team(OriginalName);
				StartCoroutine (t.Commit (commitError => {
					errorCount++;
					Debug.LogError("GetTeamDetails failed: " + commitError.Message);	
				}, createdTeam => {
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
					StartCoroutine (t.Commit (updateError => {
						errorCount++;
						Debug.LogError("Update failed: " + updateError.Message);
					}, updatedTeam => {
						t = updatedTeam;
						if (t.Name != NewName) {
							errorCount++;
							Debug.LogError ("Update Team failed");
						} else {
							Debug.Log("Update Team passed");
						}

						// Delete Team
						StartCoroutine (t.Delete (deleteError => {
							errorCount++;
							Debug.LogError("Delete team failed: " + deleteError.Message);
						}, deleteSuccess => {
							// Error Report
							if (errorCount > 0) {
								Debug.LogError(errorCount + " Team tests failed");
							} else {
								Debug.Log("All Team tests passed");
							}
						}));
					}));
				}));
			}));
		}));
	}
}
