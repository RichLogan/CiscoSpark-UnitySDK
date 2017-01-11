using UnityEngine;
using Cisco.Spark;

public class TestTeam : MonoBehaviour {

    string teamId;
    string teamName = "Cisco Spark Unity Test Team";

    void Start() {
        CreateTeam();
    }

    void CreateTeam() {
        var team = new Team();
        team.Name = teamName;
        StartCoroutine(team.Commit(error => {
            Debug.LogError("Create Team Failed: " + error.Message);
        }, success => {
            teamId = team.Id;
            LoadTeam();
        }));
    }

    void LoadTeam() {
        var team = new Team(teamId);
        StartCoroutine(team.Load(error => {
            Debug.LogError("Failed to load Team: " + error.Message);
        }, success => {
            if (team.Name.Equals(teamName)) {
                // Create is known to have passed here.
                Debug.Log("Create Team Passed!");

                // Load is also known to have passed here.
                Debug.Log("Get Team Passed!");

                // List Teams.
                UpdateTeam();
            }
        }));
    }

    void UpdateTeam() {
        var team = new Team(teamId);
        teamName = "Cisco Spark Unity Test Team - Updated";
        team.Name = teamName;
        StartCoroutine(team.Commit(error => {
            Debug.LogError("Update Team Failed: " + error.Message);
        }, success => {
            ListTeams();
        }));
    }

    void ListTeams() {
        StartCoroutine(Team.ListTeams(error => {
            Debug.LogError("List Teams Failed: " + error.Message);
        }, teams => {
            bool success = false;
            foreach (var team in teams) {
                if (team.Name.Equals(teamName)) {
                    success = true;
                }
            }
            if (success) {
                // Update is known to have passed here.
                Debug.Log("Update Team Passed!");

                // List Teams Passed!
                Debug.Log("List Teams Passed!");
                DeleteTeam();
            }
        }));
    }

    void DeleteTeam() {
        var team = new Team(teamId);
        StartCoroutine(team.Delete(error => {
            Debug.LogError("Delete Team Failed: " + error.Message);
        }, success => {
            // Check Deleted.
            var checkDelete = new Team(teamId);
            StartCoroutine(checkDelete.Load(error => {
                Debug.Log(error.Message);
                if (error.Message.Equals("Could not find teams.")) {
                    // Passed!
                    Debug.Log("Delete Team Passed!");
                } else {
                    // Actual error.
                    Debug.LogError("Delete Team Failed: " + error.Message);
                }
            }, deleteTestSuccess => {
                // This shouldn't happen!
                Debug.LogError("Delete Team Failed!");
            }));
        }));
    }
}