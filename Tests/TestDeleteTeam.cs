using UnityEngine;
using Cisco.Spark;

public class TestDeleteTeam : MonoBehaviour
{
    Team team;

    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        team = new Team();
        team.Name = "Unity SDK Test Room";
        StartCoroutine(team.Commit(error =>
        {
            IntegrationTest.Fail("Failed to create example team: " + error.Message);
        }, success =>
        {
            Test();
        }));
    }

    void Test()
    {
        StartCoroutine(team.Delete(error =>
        {
            IntegrationTest.Fail(error.Message);
        }, success =>
        {
            IntegrationTest.Pass();
        }));
    }
}
