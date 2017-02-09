using UnityEngine;
using Cisco.Spark;

public class TestListTeams : MonoBehaviour
{

    Team team;

    // Use this for initialization
    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        team = new Team();
        team.Name = "Unity SDK Test Team";
        StartCoroutine(team.Commit(error =>
        {
            IntegrationTest.Fail("Failed to create test team: " + error.Message);
        }, success =>
        {
            Test();
        }));
    }

    void Test()
    {
		var found = false;
		StartCoroutine(Team.ListTeams(error => {
			TearDown();
			IntegrationTest.Fail(error.Message);
		}, results => {
			foreach (var t in results) {
				if (t.Name == team.Name) {
					found = true;
					break;
				}
			}

			if (found) {
				TearDown();
				IntegrationTest.Pass();
			} else {
				TearDown();
				IntegrationTest.Fail("Failed to find created room in list");
			}
		}));
    }

    void TearDown()
    {
		StartCoroutine(team.Delete(error => {
			IntegrationTest.Fail("Failed to delete test room: " + error.Message);
		}, success => {
			return;
		}));
    }
}
