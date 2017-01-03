using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    public abstract class SparkObject
    {
        /// <summary>
        /// The Spark service's UID for this object.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// The DateTime at which this record was created on Spark.
        /// </summary>
        public DateTime Created { get; internal set; }

        internal abstract SparkType SparkType { get; }

        /// <summary>
        /// Commits the object to the Spark service.
        /// This will create the object if it doesn't already exist.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="success">Callback for completion.</param>
        public IEnumerator Commit(Action<SparkMessage> error, Action<bool> success)
        {
            if (Id == null)
            {
                // Create new record.
                var keys = RetrieveConstraints("create");
                var createRoutine = Request.Instance.CreateRecord(ToDict(keys), SparkType.Room, error, LoadDict);
                yield return Request.Instance.StartCoroutine(createRoutine);
                success(true);
            }
            else
            {
                // Update existing record.
                var keys = RetrieveConstraints("update");
                var updateRoutine = Request.Instance.UpdateRecord(Id, ToDict(keys), SparkType.Room, error, LoadDict);
                yield return Request.Instance.StartCoroutine(updateRoutine);
                success(true);
            }
        }

        /// <summary>
        /// Deletes the object from the Spark service.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="success">Callback for completion.</param>
        public IEnumerator Delete(Action<SparkMessage> error, Action<bool> success)
        {
            var deleteRoutine = Request.Instance.DeleteRecord(Id, SparkType, error, success);
            yield return Request.Instance.StartCoroutine(deleteRoutine);
        }

        /// <summary>
        /// Populates the current object with it's details from Spark, if it exists.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="success">Callback for completion.</param>
        public IEnumerator Load(Action<SparkMessage> error, Action<bool> success)
        {
            if (Id != null)
            {
                var getRecordRoutine = Request.Instance.GetRecord(Id, SparkType, error, result =>
                {
                    LoadDict(result);
                    success(true);
                });
                yield return Request.Instance.StartCoroutine(getRecordRoutine);
            }
            else
            {
                UnityEngine.Debug.LogError("An ID must be set in order to populate the object.");
            }
        }

        /// <summary>
        /// Returns a dictionary representation of the object.
        /// </summary>
        /// <returns>The Dictionary.</returns>
        /// <param name="fields">A specific list of fields to serialise.</param>
        protected virtual Dictionary<string, object> ToDict(List<string> fields = null)
        {
            var data = new Dictionary<string, object>();
            data["id"] = Id;
            data["created"] = Created;
            return data;
        }

        /// <summary>
        /// Populates an object with data retrieved from Spark.
        /// </summary>
        /// <param name="data">Data.</param>
        protected virtual void LoadDict(Dictionary<string, object> data)
        {
            if (data.ContainsKey("message"))
            {
                var message = new SparkMessage(data);
                throw new Exception(message.Message);
            }

            Id = data["id"] as string;
            Created = DateTime.Parse(data["created"] as string);
        }

        protected Dictionary<string, object> CleanDict(Dictionary<string, object> data, List<string> fields)
        {
            if (fields != null)
            {
                var buffer = new List<string>(data.Keys);
                foreach (var key in buffer)
                {
                    if (!fields.Contains(key))
                    {
                        data.Remove(key);
                    }
                }
                return data;
            }
            return data;
        }

        /// <summary>
        /// Returns the list of fields that the API accepts for a given operation.
        /// </summary>
        /// <param name="updateCreate">'update' or 'create' operation.</param>
        /// <returns>The list of fields.</returns>
        protected List<string> RetrieveConstraints(string updateCreate)
        {
            // Retrieve constraints.
            var lookupKey = SparkResources.Instance.UrlEndpoints[SparkType];
            var resourceConstraints = SparkResources.Instance.ApiConstraints[lookupKey] as Dictionary<string, object>;
            var requestedConstraints = resourceConstraints[updateCreate] as List<object>;

            // Build up / return results.
            var results = new List<string>();
            foreach (var constraint in requestedConstraints)
            {
                var value = (string)constraint;
                results.Add(value);
            }
            return results;
        }

        public static IEnumerator ListObjects<T>(Dictionary<string, string> constraints, SparkType type, Action<SparkMessage> error, Action<List<T>> result) where T : SparkObject, new()
        {
            var listRoutine = Request.Instance.ListRecords(constraints, type, error, success =>
            {
                List<T> retrivedObjects = new List<T>();
                foreach (var sparkObject in success)
                {
                    var details = sparkObject as Dictionary<string, object>;
                    var newSparkObject = new T();
                    newSparkObject.Id = details["id"] as string;
                    newSparkObject.LoadDict(details);
                    retrivedObjects.Add(newSparkObject);
                }
                result(retrivedObjects);
            });
            yield return Request.Instance.StartCoroutine(listRoutine);
        }
    }
}