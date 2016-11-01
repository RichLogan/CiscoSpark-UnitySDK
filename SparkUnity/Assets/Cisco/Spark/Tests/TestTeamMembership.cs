using UnityEngine;
using Cisco.Spark;

public class TestTeamMembership : MonoBehaviour {
	// Use this for initialization
	void Start ()
	{
		// Error Tracking
		var errorCount = 0;

		var testTeam = new Team ("SparkUnityTestTeam");
		StartCoroutine (testTeam.Commit (error => {
			Debug.LogError("Could not create target Team: " + error.Message);
			errorCount++;
		}, team => {
			Debug.Log("Created target Team!");
			testTeam = team;
			if (testTeam.Id == null) {
				Debug.LogError ("Could not create target Team");
				errorCount++;
			}

			// Create the membership
			var teamMembership = new TeamMembership ();
			teamMembership.TeamId = testTeam.Id;
			teamMembership.PersonId = "Y2lzY29zcGFyazovL3VzL1BFT1BMRS9iYzJkMjY2YS1jYjI4LTRkZDItOTBkOC1kMzllYjc0OWZlYTU";

			// Commit the membership to Spark
			StartCoroutine (teamMembership.Commit (commitError => {
				Debug.LogError ("Couldn't commit team membership: " + commitError.Message);
				errorCount++;
			}, commitResponse => {
				Debug.Log("Created team membership on Spark!");
				teamMembership = commitResponse;
				if (teamMembership.Id == null) {
					Debug.LogError ("Couldn't commit membership");
					errorCount++;
				}

				// Try and commit empty membership
				StartCoroutine (new TeamMembership ().Commit (emptyCommitError => {
					// This should error
					if (!emptyCommitError.Message.Equals ("teamId cannot be null")) {
						Debug.LogError ("Empty TeamId test failed: " + emptyCommitError.Message);
					} else {
						Debug.Log("Empty TeamId test passed!");
					}

					// Get Membership Details
					StartCoroutine (TeamMembership.GetTeamMembershipDetails (teamMembership.Id, membershipDetailsError => {
						Debug.LogError ("Get Team Membership Details failed: " + membershipDetailsError.Message);
						errorCount++;
					}, membershipDetails => {
						teamMembership = membershipDetails;
						if (membershipDetails.Id != teamMembership.Id) {
							Debug.LogError ("Couldn't retrieve membership details");
							errorCount++;
						} else {
							Debug.Log ("Get Team Membership Details passed!");
						}

						// List Memberships
						StartCoroutine (TeamMembership.ListTeamMemberships (listMembershipsError => {
							Debug.LogError("Couldn't list team memberships: " + listMembershipsError.Message);
							errorCount++;
						}, memberships => {
							Debug.Log("List Team Memberships passed!");

							// Convert to moderator
							teamMembership.IsModerator = true;
							StartCoroutine (teamMembership.Commit(moderatedError => {
								Debug.LogError("Couldn't set moderator flag: " + moderatedError.Message);
								errorCount++;
							}, moderatedMembership => {
								teamMembership = moderatedMembership;
								if (!teamMembership.IsModerator) {
									Debug.LogError("Couldn't set moderator flag");
									errorCount++;
								} else {
									Debug.Log("Edit Team Membership passed!");
								}

								// Clean up membership
								StartCoroutine (teamMembership.Delete (deleteError => {
									Debug.Log("Couldn't delete membership: " + deleteError.Message);
									errorCount++;
								}, deleted => StartCoroutine (testTeam.Delete (deleteTeamError => {
									Debug.LogError ("Couldn't delete target Team: " + deleteTeamError.Message);
									errorCount++;
								}, deleteTeam => {
									if (errorCount > 0) {
										Debug.LogError (errorCount + " tests failed");
									} else {
										Debug.Log ("All Team Membership tests passed");
									}
								}))));
							}));
						}, testTeam.Id));
					}));
				}, res => {}));
			}));
		}));
	}
}