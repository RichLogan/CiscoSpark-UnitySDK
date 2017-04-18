using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// Handles building and making web requests to the Spark service.
    /// </summary>
    public class Request : MonoBehaviour
    {
        /// <summary>
        /// Singleton for a Request instance.
        /// </summary>
        public static Request Instance;

        /// <summary>
        /// Base URL for the Spark API.
        /// </summary>
        public const string BaseUrl = "https://api.ciscospark.com/v1";

        /// <summary>
        /// The authentication token to make requests with.
        /// The Person (or bot) associated with this token is stored in <see cref="Person.AuthenticatedUser"/>.
        /// </summary>
        public string AuthenticationToken = "";

        /// <summary>
        /// True if initial Setup has completed.
        /// Calls using <see cref="Person.AuthenticatedUser"/> must wait for this to be true.
        /// </summary>
        public bool SetupComplete { get; private set; }

        /// <summary>
        /// Request setup should run as early as possible, in case requests are made on Start() elsewhere.
        /// </summary>
        void Awake()
        {
            if (AuthenticationToken == null || AuthenticationToken == "")
            {
                throw new Exception("AuthenticationToken MUST be set for Request setup");
            }

            // Assign singleton.
            Instance = this;

            // Reference to Authenticated User.
            StartCoroutine(Person.GetMyself(error =>
            {
                throw new Exception("Couldn't set the Authenticated User");
            }, success =>
            {
                SetupComplete = true;
                Debug.Log("Cisco Spark SDK Ready! Authenticated as: " + Person.AuthenticatedUser.DisplayName);
            }));
        }

        /// <summary>
        /// Generate a Web Request to Spark.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="requestType">Request type.</param>
        /// <param name="data">Data to upload.</param>
        public UnityWebRequest Generate(string resource, string requestType, byte[] data = null)
        {
            // Setup Headers.
            var www = new UnityWebRequest(BaseUrl + "/" + resource);
            www.SetRequestHeader("Authorization", "Bearer " + AuthenticationToken);
            www.SetRequestHeader("Content-type", "application/json; charset=utf-8");
            www.method = requestType;
            www.downloadHandler = new DownloadHandlerBuffer();

            // Is there data to upload?
            if (data != null)
            {
                www.uploadHandler = new UploadHandlerRaw(data);
            }

            return www;
        }

        /// <summary>
        /// Retrieves an existing record from the Spark service by ID.
        /// </summary>
        /// <returns>The record's data.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="type">SparkType being retrieved.</param>
        /// <param name="error">Error.</param>
        /// <param name="result">Result.</param>
        public IEnumerator GetRecord(string id, SparkType type, Action<SparkMessage> error, Action<Dictionary<string, object>> result)
        {
            var url = string.Format("{0}/{1}", type.GetEndpoint(), id);
            var send = SendRequest(url, UnityWebRequest.kHttpVerbGET, null, error, result);
            yield return StartCoroutine(send);
        }

        /// <summary>
        /// Creates the record.
        /// </summary>
        /// <returns>The record.</returns>
        /// <param name="data">Data.</param>
        /// <param name="type">Type.</param>
        /// <param name="error">Error.</param>
        /// <param name="result">Result.</param>
        public IEnumerator CreateRecord(Dictionary<string, object> data, SparkType type, Action<SparkMessage> error, Action<Dictionary<string, object>> result)
        {
            // Create request.
            var recordDetails = System.Text.Encoding.UTF8.GetBytes(Json.Serialize(data));
            var url = type.GetEndpoint();
            var send = SendRequest(url, UnityWebRequest.kHttpVerbPOST, recordDetails, error, result);
            yield return StartCoroutine(send);
        }

        /// <summary>
        /// Updates the record.
        /// </summary>
        /// <returns>The record.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="data">Data.</param>
        /// <param name="type">Type.</param>
        /// <param name="error">Error.</param>
        /// <param name="result">Result.</param>
        public IEnumerator UpdateRecord(string id, Dictionary<string, object> data, SparkType type, Action<SparkMessage> error, Action<Dictionary<string, object>> result)
        {
            // Update Record.
            var recordDetails = System.Text.Encoding.UTF8.GetBytes(Json.Serialize(data));
            var url = type.GetEndpoint() + "/" + id;
            var send = SendRequest(url, UnityWebRequest.kHttpVerbPUT, recordDetails, error, result);
            yield return StartCoroutine(send);
        }

        /// <summary>
        /// Deletes the record from Spark.
        /// </summary>
        /// <returns>The record.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="type">Type.</param>
        /// <param name="error">Error.</param>
        /// <param name="success">Success.</param>
        public IEnumerator DeleteRecord(string id, SparkType type, Action<SparkMessage> error, Action<bool> success)
        {
            // Create Request.
            var url = type.GetEndpoint() + "/" + id;
            var send = SendRequest(url, UnityWebRequest.kHttpVerbDELETE, null, error, result =>
            {
                success(true);
            });
            yield return StartCoroutine(send);
        }

        /// <summary>
        /// Retrieves multiple records from the Spark service.
        /// </summary>
        /// <param name="constraints">Any constraints on the results returned. See ApiConstrains.json for SparkType specific options.</param>
        /// <param name="type">The SparkType to retrieve.</param>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="result">List of de-serialised results as dictionaries.</param>
        public IEnumerator ListRecords(Dictionary<string, string> constraints, SparkType type, Action<SparkMessage> error, Action<List<object>> result)
        {
            string queryString = System.Text.Encoding.UTF8.GetString(UnityWebRequest.SerializeSimpleForm(constraints));
            string url = string.Format("{0}?{1}", type.GetEndpoint(), queryString);
            var operation = SendRequest(url, UnityWebRequest.kHttpVerbGET, null, error, data =>
            {
                var items = data["items"] as List<object>;
                result(items);
            });
            yield return StartCoroutine(operation);
        }

        /// <summary>
        /// Makes a request so Spark and parses the response.
        /// </summary>
        /// <param name="url">URL to send request.</param>
        /// <param name="requestType">Request Type.</param>
        /// <param name="data">Data to upload, if any.</param>
        /// <param name="error">SparkMessage to return.</param>
        /// <param name="result">Result dictionary to return.</param>
        IEnumerator SendRequest(string url, string requestType, byte[] data, Action<SparkMessage> error, Action<Dictionary<string, object>> result)
        {
            using (var www = Generate(url, requestType, data))
            {
                yield return www.Send();

                if (www.isError)
                {
                    // Unity couldn't make the Web Request.
                    throw new Exception(www.error);
                }
                else
                {
                    // Handle an empty response.
                    try
                    {
                        var returnedData = Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                        if (www.responseCode == 200)
                        {
                            result(returnedData);
                        }
                        else
                        {
                            error(new SparkMessage(returnedData, www));
                        }
                    }
                    catch (OverflowException)
                    {
                        // Response Body was empty.
                        if (www.responseCode == 204)
                        {
                            // This is actually a deletion success (it returns no body).
                            result(null);
                        }
                        else
                        {
                            // TODO: Find out if this can actually happen.
                            // Unknown error occurred.
                            Debug.LogError("Create an issue!: www.error");
                            error(new SparkMessage(www));
                        }
                    }
                }
            }
        }
    }
}