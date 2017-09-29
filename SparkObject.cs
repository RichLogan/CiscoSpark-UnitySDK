using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// SparkObject represents an object on the Spark Service, and defines
    /// functionality needed for CRUD operations, as well as loading from and
    /// sending to the Spark web service.
    /// Each SparkObject MUST map to a <see cref="SparkType"/>.
    /// </summary>
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

        /// <summary>
        /// True if the object has already been loaded with Spark data.
        /// </summary>
        public bool Loaded { get; internal set; }

        /// <summary>
        /// The SparkType the implementation represents.
        /// </summary>
        internal abstract SparkType SparkType { get; }

        // Caches and queues.
        internal static Dictionary<string, SparkObject> _LocalCache = new Dictionary<string, SparkObject>();
        static List<string> LockedIds = new List<string>();
        static Dictionary<string, List<Action<bool>>> LoadCallbacks = new Dictionary<string, List<Action<bool>>>();

        /// <summary>
        /// Commits the object to the Spark service.
        /// This will create the object if it doesn't already exist.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="success">Callback for completion.</param>
        public IEnumerator Commit(Action<SparkMessage> error, Action<bool> success)
        {
            IEnumerator routine;
            if (Id == null)
            {
                // Create new record.
                var keys = RetrieveConstraints("create");
                routine = Request.Instance.CreateRecord(ToDict(keys), SparkType, error, LoadDict);
            }
            else
            {
                // Update existing record.
                var keys = RetrieveConstraints("update");
                routine = Request.Instance.UpdateRecord(Id, ToDict(keys), SparkType, error, LoadDict);
            }
            yield return Request.Instance.StartCoroutine(routine);
            success(true);
            _LocalCache.Add(Id, this);
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
                if (!Loaded)
                {
                    // Is this already being worked on?
                    if (LockedIds.Contains(Id))
                    {
                        // Add callback for notification.
                        if (!LoadCallbacks.ContainsKey(Id)) LoadCallbacks.Add(Id, new List<Action<bool>>());
                        LoadCallbacks[Id].Add(success);
                    }
                    else
                    {
                        // We're the first one to load this object.
                        // Lock, load, callback.
                        LockedIds.Add(Id);

                        // Load details from Spark.
                        var getRecordRoutine = Request.Instance.GetRecord(Id, SparkType, error, result =>
                        {
                            try
                            {
                                LoadDict(result);
                                Loaded = true;
                            }
                            catch (KeyNotFoundException)
                            {
                                error(new SparkMessage("Record was malformed"));
                            }
                        });
                        yield return Request.Instance.StartCoroutine(getRecordRoutine);

                        // Invoke all waiting callbacks.
                        if (LoadCallbacks.ContainsKey(Id))
                        {
                            foreach (var callback in LoadCallbacks[Id]) callback(true);
                            LoadCallbacks.Remove(Id);
                        }
                        LockedIds.Remove(Id);
                        success(true);
                    }
                }
                else
                {
                    success(true);
                }
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
        /// <param name="data">Deserialised data dictionary from Spark.</param>
        protected virtual void LoadDict(Dictionary<string, object> data)
        {
            Id = data["id"] as string;
            Created = DateTime.Parse(data["created"] as string);
        }

        /// <summary>
        /// Removes any fields in a dictionary that don't match the list of keys given in paramref name="fields".
        /// Primarily used to clean the searilised objects ready for a Create/Update request to Spark.
        /// </summary>
        /// <param name="data">The dictionary to clean.</param>
        /// <param name="fields">The list of fields to keep.</param>
        /// <returns></returns>
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
            try
            {
                var lookupKey = SparkType.GetEndpoint();
                try
                {
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
                catch (KeyNotFoundException)
                {
                    throw new Exception("This SparkType has no supported fields in ApiConstraints.");
                }
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("This SparkType has no registered URL Endpoint in SparkResources.");
            }


        }

        /// <summary>
        /// Returns a list of a given SparkObject type matching the API constraints offered.
        /// </summary>
        /// <param name="constraints">Dictionary of URL constraints the request takes.</param>
        /// <param name="type">The SparkType being searched.</param>
        /// <param name="error">The error from Spark, if any.</param>
        /// <param name="result">The resultant list of the given SparkObject typeparamref name="T".</param>
        /// <typeparam name="T">The SparkObject child being searched for.</param>
        protected static IEnumerator ListObjects<T>(Dictionary<string, string> constraints, SparkType type, Action<SparkMessage> error, Action<List<T>> result) where T : SparkObject
        {
            var listRoutine = Request.Instance.ListRecords(constraints, type, error, success =>
            {
                List<T> retrivedObjects = new List<T>();
                foreach (var sparkObject in success)
                {
                    var details = sparkObject as Dictionary<string, object>;
                    var newSparkObject = (T)SparkObjectFactory.Make(details["id"] as string, type);
                    try
                    {
                        newSparkObject.LoadDict(details);
                        retrivedObjects.Add(newSparkObject);
                    }
                    catch (Exception)
                    {
                        UnityEngine.Debug.LogError("Error while parsing record. Failed record was: " + details["id"]);
                        continue;
                    }
                }
                result(retrivedObjects);
            });
            yield return Request.Instance.StartCoroutine(listRoutine);
        }
    }
}