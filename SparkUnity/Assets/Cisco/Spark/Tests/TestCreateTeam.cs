using UnityEngine;
using Cisco.Spark;

public class TestCreateTeam : MonoBehaviour
{
    Team team;

    void Start()
    {
        Test();
    }

    void Test()
    {
        // Try and create team.
        team = new Team();
        team.Name = "Unity SDK Test Team";
        StartCoroutine(team.Commit(error => {
            IntegrationTest.Fail(error.Message);
        }, success => {
            TearDown();
            IntegrationTest.Pass();
        }));
    }

    void TearDown()
    {
        StartCoroutine(team.Delete(error => {
            IntegrationTest.Fail("Failed to delete test Team: " + error.Message);
        }, success => {
            return;
        }));
    }
}