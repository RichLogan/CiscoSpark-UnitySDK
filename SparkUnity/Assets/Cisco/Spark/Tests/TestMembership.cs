using UnityEngine;
using Cisco.Spark;

public class TestMembership : MonoBehaviour
{
    [TooltipAttribute("This must NOT be the person creating the room")]
    public string testPersonid = "";

    Person person;
    Membership membership;
    Room testRoom;

    bool start = false;

    void Update()
    {
        // We need to use Person.AuthenticatedUser so have to wait for it to be ready.
        if (Request.Instance.SetupComplete && !start)
        {
            TestStart();
            start = true;
        }
    }

    void TestStart()
    {
        // Check people.
        if (testPersonid == Person.AuthenticatedUser.Id)
        {
            Fail("Test person cannot be the authenticated user.");
        }

        // Need a person and a room first!
        person = new Person(testPersonid);
        testRoom = new Room("Cisco Spark Unity SDK Test", null);
        // Commit Test Room.
        StartCoroutine(testRoom.Commit(error =>
        {
            Fail(error.Message);
        }, result =>
        {
            CreateMembership();
        }));

    }

    void CreateMembership()
    {
        // Got new room, now create membership.
        var newMembership = new Membership(testRoom, person);

        // Commit to Spark.
        StartCoroutine(newMembership.Commit(error =>
        {
            // Failed!
            Fail(error.Message);
        }, commited =>
        {
            // Seems to have worked, but we don't know for sure yet.
            membership = newMembership;
            GetMembership();
        }));
    }

    void GetMembership()
    {
        var retrieve = new Membership(membership.Id);
        StartCoroutine(retrieve.Load(error =>
        {
            // Failed!
            Fail(error.Message);
        }, success =>
        {
            if (retrieve.Room.Id == membership.Room.Id)
            {
                // Create is now known to have passed here.
                Debug.Log("Create Membership Passed!");
                // Get Membership just passed.
                Debug.Log("Get Membership Passed!");
                // Move on.
                UpdateMembership();
            }
        }));
    }

    void UpdateMembership()
    {
        // Let's set moderator to true.
        if (membership.IsModerator == true)
        {
            Fail("Moderator should start false");
        }
        else
        {
            membership.IsModerator = true;
            StartCoroutine(membership.Commit(error =>
            {
                // Failed.
                Fail(error.Message);
            }, success =>
            {
                // Looks like it's worked, let's check.
                var checkingModerator = new Membership(membership.Id);
                StartCoroutine(checkingModerator.Load(error =>
                {
                    Fail(error.Message);
                }, checkSuccess =>
                {
                    if (checkingModerator.IsModerator)
                    {
                        // Now we know Update passed for sure.
                        Debug.Log("Update Membership Passed!");
                        ListMemberships();
                    }
                }));
            }));
        }
    }

    void ListMemberships()
    {
        // List all memberships and see if our new one is there.
        StartCoroutine(Membership.ListMemberships(error =>
        {
            // Failed.
            Fail(error.Message);
        }, results =>
        {
            foreach (var mem in results)
            {
                if (mem.Id == membership.Id)
                {
                    // Found it!
                    Debug.Log("List Memberships Passed");
                    DeleteMembership();
                }
            }
        }, testRoom));
    }

    void DeleteMembership()
    {
        var oldMembershipId = string.Copy(membership.Id);
        StartCoroutine(membership.Delete(error =>
        {
            Fail(error.Message);
        }, success =>
        {
            // Double check (Expect get to know fail).
            var checkDelete = new Membership(oldMembershipId);
            StartCoroutine(checkDelete.Load(error =>
            {
                Debug.Log(error.Message);
                TestEnd();
            }, getSuccess =>
            {
                Fail("Should fail to Get deleted Membership");
            }));
        }));
    }

    void TestEnd()
    {
        // Delete the room for cleanup.
        StartCoroutine(testRoom.Delete(error => Fail(error.Message), success =>
        {
            Debug.Log("Deleted the test Room");
            Debug.Log("***TestMembership Finished***");
        }));
    }

    void Fail(string error)
    {
        throw new System.Exception("Membership tests failed: " + error);
    }
}
