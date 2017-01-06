using UnityEngine;
using Cisco.Spark;

public class TestTeamMembership : MonoBehaviour
{
    [TooltipAttribute("This must NOT be the person creating the Team")]
    public string testPersonid = "";

    Person person;
    TeamMembership membership;
    Team testTeam;

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

        // Need a Person and a Team first!
        person = new Person(testPersonid);
        testTeam = new Team();
        testTeam.Name = "Cisco Spark Unity Team Test";
        // Commit Test Team.
        StartCoroutine(testTeam.Commit(error =>
        {
            Fail(error.Message);
        }, result =>
        {
            CreateMembership();
        }));

    }

    void CreateMembership()
    {
        // Got new Team, now create membership.
        var newMembership = new TeamMembership(testTeam, person);

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
        var retrieve = new TeamMembership(membership.Id);
        StartCoroutine(retrieve.Load(error =>
        {
            // Failed!
            Fail(error.Message);
        }, success =>
        {
            if (retrieve.Team.Id == membership.Team.Id)
            {
                // Create is now known to have passed here.
                Debug.Log("Create Team Membership Passed!");
                // Get Membership just passed.
                Debug.Log("Get Team Membership Passed!");
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
                var checkingModerator = new TeamMembership(membership.Id);
                StartCoroutine(checkingModerator.Load(error =>
                {
                    Fail(error.Message);
                }, checkSuccess =>
                {
                    if (checkingModerator.IsModerator)
                    {
                        // Now we know Update passed for sure.
                        Debug.Log("Update Team Membership Passed!");
                        ListMemberships();
                    }
                }));
            }));
        }
    }

    void ListMemberships()
    {
        // List all memberships and see if our new one is there.
        StartCoroutine(TeamMembership.ListTeamMemberships(error =>
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
                    Debug.Log("List Team Membership Memberships Passed");
                    DeleteMembership();
                }
            }
        }, testTeam));
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
            var checkDelete = new TeamMembership(oldMembershipId);
            StartCoroutine(checkDelete.Load(error =>
            {
                if (error.Message.Equals("Failed to get membership."))
                {
                    Debug.Log("Delete TeamMembership Passed!");
                    TestEnd();
                }
            }, getSuccess =>
            {
                Fail("Should fail to Get deleted Membership");
            }));
        }));
    }

    void TestEnd()
    {
        // Delete the Team for cleanup.
        StartCoroutine(testTeam.Delete(error => Fail(error.Message), success =>
        {
            Debug.Log("Deleted the test Team");
            Debug.Log("***TestTeamMembership Finished***");
        }));
    }

    void Fail(string error)
    {
        throw new System.Exception("TeamMembership tests failed: " + error);
    }
}
