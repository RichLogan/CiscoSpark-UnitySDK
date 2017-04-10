using UnityEngine;
using Cisco.Spark;

public class TestUpdateTeam : MonoBehaviour
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
        team.Name = "Unity SDK Team";
        StartCoroutine(team.Commit(error =>
        {
            IntegrationTest.Fail("Failed to commit test team");
        }, success =>
        {
            Test();
        }));
    }

    void Test()
    {
        team.Name = "Unity SDK Team - Updated";
        StartCoroutine(team.Commit(error =>
        {
            TearDown();
            IntegrationTest.Fail(error.Message);
        }, success =>
        {
            TearDown();
            IntegrationTest.Pass();
        }));
    }

    void TearDown()
    {
        StartCoroutine(team.Delete(error =>
        {
            IntegrationTest.Fail("Failed to clean up test team");
        }, success =>
        {
            return;
        }));
    }
}
