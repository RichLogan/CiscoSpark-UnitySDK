﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// A registered user of the Spark platform.
    /// </summary>
    public class Person : SparkObject
    {
        /// <summary>
        /// The SparkType the implementation represents.
        /// </summary>
        internal override SparkType SparkType
        {
            get { return SparkType.Person; }
        }

        /// <summary>
        /// Full name of the Person.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Friendly name of the Person.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// First name of the Person.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the Person.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Display picture of the Person.
        /// </summary>
        public Avatar Avatar { get; set; }

        /// <summary>
        /// List of emails associated with this person.
        /// </summary>
        public List<string> Emails { get; set; }

        /// <summary>
        /// Holds a reference to the currently authenticated <see cref="Person"/> given
        /// by the authentication token in Request.
        /// </summary>
        /// <returns>Currently authenticated <see cref="Person"/>.</returns>
        public static Person AuthenticatedUser { get; internal set; }

        // TODO: New Objects for these.
        // public Organization Organization {get; set;}
        // public List<Role> Roles {get; set;}
        // public List<License> Licenses {get; set;}
        // public TimeZone Timezone {get; set;}
        // public string Status {get; set;}

        /// <summary>
        /// Builds representation of existing Spark-side
        /// Person. Use <see cref="Load"/> to populate rest of properties from Spark.
        /// </summary>
        /// <param name="id">Spark UID of the Person.</param>
        public Person(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Returns a dictionary representation of the object.
        /// </summary>
        /// <returns>The Dictionary.</returns>
        /// <param name="fields">A specific list of fields to serialise.</param>
        protected override Dictionary<string, object> ToDict(List<string> fields = null)
        {
            var data = base.ToDict();
            data["displayName"] = DisplayName;
            data["nickName"] = NickName;
            data["firstName"] = FirstName;
            data["lastName"] = LastName;
            data["avatar"] = Avatar.Uri.ToString();
            data["emails"] = Emails;
            return CleanDict(data, fields);
        }

        /// <summary>
        /// Populates an object with data received from Spark.
        /// </summary>
        /// <param name="data">Data.</param>
        protected override void LoadDict(Dictionary<string, object> data)
        {
            base.LoadDict(data);

            object displayName;
            if (data.TryGetValue("displayName", out displayName))
            {
                DisplayName = displayName as string;
            }

            object nickName;
            if (data.TryGetValue("nickName", out nickName))
            {
                NickName = nickName as string;
            }

            object firstName;
            if (data.TryGetValue("firstName", out firstName))
            {
                FirstName = firstName as string;
            }

            object lastName;
            if (data.TryGetValue("lastName", out lastName))
            {
                lastName = lastName as string;
            }

            // Avatar.
            var avatarUri = new Uri(data["avatar"] as string);
            Avatar = new Avatar(avatarUri);

            // Emails.
            Emails = new List<string>();
            foreach (var obj in data["emails"] as List<object>)
            {
                Emails.Add(obj as string);
            }
        }

        /// <summary>
        /// Sets <see cref="AuthenticatedUser"/> to the currently authenticated user.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="success">True if the operation completed successfully.</param>
        public static IEnumerator GetMyself(Action<SparkMessage> error, Action<bool> success)
        {
            var getRecordRoutine = Request.Instance.GetRecord("me", SparkType.Person, error, dict =>
            {
                var id = dict["id"] as string;
                AuthenticatedUser = new Person(id);
                AuthenticatedUser.LoadDict(dict);
                success(true);
            });
            yield return getRecordRoutine;
        }

        /// <summary>
        /// Lists all Person objects found on Spark matching the given criteria.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="results">List of People found.</param>
        /// <param name="email">An email address to filter on.</param>
        /// <param name="displayName">A display name to filter on.</param>
        /// <param name="max">Maximum number of results to return.</param>
        public static IEnumerator ListPeople(Action<SparkMessage> error, Action<List<Person>> results, string email = null, string displayName = null, int max = 0)
        {
            // TODO: Admins are not bound by this rule.
            if (email == null && displayName == null)
            {
                throw new ArgumentException("One of Email or Display Name must be provided when listing People.");
            }

            var constraints = new Dictionary<string, string>();
            if (email != null)
            {
                constraints.Add("email", email);
            }
            else if (displayName != null)
            {
                constraints.Add("displayName", displayName);
            }

            if (max > 0)
            {
                constraints.Add("max", max.ToString());
            }

            var listObjects = ListObjects<Person>(constraints, SparkType.Person, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }
    }
}