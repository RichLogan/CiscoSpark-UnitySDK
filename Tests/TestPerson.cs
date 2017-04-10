using UnityEngine;
using Cisco.Spark;

public class TestPerson : MonoBehaviour
{

    public string TestPersonId = "";

    // Create.
    // GetPersonDetails.
    // Update.
    // Delete.
    // Get Own Details.

    bool started = false;

    void Update()
    {
        // Wait for Request to initialise.
        if (Request.Instance.SetupComplete && !started)
        {
            started = true;
            CreatePerson();
        }
    }

    void CreatePerson()
    {
        // Create can only be used by an admin.
        // TODO: CreatePerson Test.
        GetPersonDetails();
    }

    void GetPersonDetails()
    {
        var newPerson = new Person(TestPersonId);
        var getPersonDetails = newPerson.Load(error =>
        {
            Debug.LogError(error.Message);
        }, success =>
        {
            if (newPerson.DisplayName != null)
            {
                Debug.Log("Get Person Details Passed!");
                UpdatePerson();
            }
        });
        StartCoroutine(getPersonDetails);
    }

    void UpdatePerson()
    {
        // TODO: UpdatePerson test.
        DeletePerson();
    }

    void DeletePerson()
    {
        // TODO: DeletePerson test.
        GetOwnDetails();
    }

    void GetOwnDetails()
    {
        if (Person.AuthenticatedUser.DisplayName != null)
        {
            Debug.Log("Get Own Details Passed!");
        }
        else
        {
            Debug.LogError("Get Own Details Failed");
        }
        DownloadAvatar();
    }

    void DownloadAvatar()
    {
        var avatar = new Person(TestPersonId);
        var loadRoutine = avatar.Load(error =>
        {
            Debug.LogError("Failed to Load person details during Avatar test");
        }, success =>
        {
            var downloadRoutine = avatar.Avatar.Download(textureSuccess =>
            {
                var testObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                testObject.GetComponent<Renderer>().material.mainTexture = avatar.Avatar.Texture;
                if (avatar.Avatar.Texture.width > 0)
                {
                    Debug.Log("Avatar download Passed");
                }
                else
                {
                    Debug.LogError("Avatar download got an empty texture");
                }
            });
            StartCoroutine(downloadRoutine);
        });
        StartCoroutine(loadRoutine);
    }
}