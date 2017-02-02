using UnityEngine;
using Cisco.Spark;

public class TestCreateRoom : MonoBehaviour
{
    void Start()
    {
        Test();
    }

    void Test()
    {
        var room = new Room("Unity SDK Test Room", null);
        StartCoroutine(room.Commit(error =>
        {
            // Error on Commit.
            IntegrationTest.Fail(error.Message);
        }, success =>
        {
            TearDown(room);
        }));
    }

    void TearDown(Room room)
    {
        StartCoroutine(room.Delete(error =>
        {
            IntegrationTest.Fail("Failed to cleanup test room: " + error.Message);
        }, success =>
        {
            IntegrationTest.Pass();
        }));
    }
}