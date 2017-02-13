using UnityEngine;
using Cisco.Spark;

public class TestLoadTeam : MonoBehaviour
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
            IntegrationTest.Fail("Couldn't create test team: " + error.Message);
        }, success =>
        {
            Test();
        }));
    }

    void Test()
    {
        var loadedTeam = new Team(team.Id);
        StartCoroutine(loadedTeam.Load(error =>
        {
            IntegrationTest.Fail(error.Message);
        }, success =>
        {
            if (loadedTeam.Name == team.Name)
            {
                TearDown();
                IntegrationTest.Pass();

            }
            else
            {
                TearDown();
                IntegrationTest.Fail("Loaded team name doesn't match created team");
            }
        }));
    }

    void TearDown()
    {
        StartCoroutine(team.Delete(error =>
        {
            IntegrationTest.Fail("Failed to delete test team: " + error.Message);
        }, success =>
        {
            return;
        }));
    }
}