using UnityEngine;
using Cisco.Spark;

public class TestMembership : MonoBehaviour {
	// Use this for initialization
	void Start ()
	{
		// Error Tracking
		var errorCount = 0;

		// Create a new room for testing
		var testRoom = new Room ("Test Room (CiscoSpark-UnitySDK", null);
		StartCoroutine (testRoom.Commit (room => {
			testRoom = room;
			if (testRoom.Id == null) {
				Debug.LogError ("Could not create target Room");
				errorCount++;
			}

			// Create the membership
			var membership = new Membership ();
			membership.RoomId = testRoom.Id;
			membership.PersonId = "Y2lzY29zcGFyazovL3VzL1BFT1BMRS9iYzJkMjY2YS1jYjI4LTRkZDItOTBkOC1kMzllYjc0OWZlYTU";

			// Commit the membership to Spark
			StartCoroutine (membership.Commit (commitError => {
				if (commitError != null) {
					Debug.LogError (commitError.Message);
					errorCount++;
				}
			}, commitResponse => {
				membership = commitResponse;
				if (membership.Id == null) {
					Debug.LogError ("Couldn't commit membership");
					errorCount++;
				}

				// Try and commit empty membership
				StartCoroutine (new Membership ().Commit (emptyCommitError => {
					// This should error
					if (emptyCommitError != null) {
						if (!emptyCommitError.Message.Equals ("roomId cannot be null")) {
							Debug.LogError ("Empty RoomId test failed");							
						}
					}

					// Get Membership Details
					StartCoroutine (Membership.GetMembershipDetails (membership.Id, membershipDetailsError => {
						if (membershipDetailsError != null) {
							Debug.LogError(membershipDetailsError.Message);
							Debug.LogError ("GetMembership Details failed");
							errorCount++;
						}
					}, membershipDetails => {
						membership = membershipDetails;
						if (membershipDetails.Id != membership.Id) {
							Debug.LogError ("Couldn't retrieve membership details");
							errorCount++;
						}

						// List Memberships
						StartCoroutine (Membership.ListMemberships (listMembershipsError => {
							if (listMembershipsError != null) {
								Debug.LogError("Couldn't list memberships: " + listMembershipsError.Message);
								errorCount++;
							}
						}, memberships => {
							// Convert to moderator
							membership.IsModerator = true;
							StartCoroutine (membership.Commit(moderatedError => {
								if (moderatedError != null) {
									Debug.LogError("Couldn't set moderator flag: " + moderatedError.Message);
									errorCount++;
								}
							}, moderatedMembership => {
								membership = moderatedMembership;
								if (!membership.IsModerator) {
									Debug.LogError("Couldn't set moderator flag");
									errorCount++;
								}

								// Clean up membership
								StartCoroutine (membership.Delete (deleteError => {
									if (deleteError != null) {
										Debug.Log("Couldn't delete membership: " + deleteError.Message);
										errorCount++;
									}
								}, deleted => {
									StartCoroutine (testRoom.Delete());
									if (errorCount > 0) {
										Debug.LogError(errorCount + " tests failed");
									} else {
										Debug.Log("All Membership tests passed");
									}
								}));
							}));
						}, testRoom.Id));
					}));
				}, res => {}));
			}));
		}));
	}
}