using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// A Team is a group of <see cref="Person"/>s with a set of <see cref="Room"/>s that are visible to all members of the Team.
    /// </summary>
    public class Team : SparkObject
    {
        /// <summary>
        /// The SparkType for this SparkObject implementation.
        /// </summary>
        internal override SparkType SparkType
        {
            get { return SparkType.Team; }
        }

        /// <summary>
        /// A user-friendly name for the Team.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Creates a new Team object representing an existing Spark Team.
        /// </summary>
        /// <param name="id"></param>
        public Team(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Creates a new Team locally.
        /// <see cref="Name"/> MUST be set manually due to constructor conflict.
        /// </summary>
        public Team() { }

        /// <summary>
        /// Returns a dictionary representation of the object.
        /// </summary>
        /// <returns>The serialised object as a Dictionary.</returns>
        /// <param name="fields">A specific list of fields to serialise.</param>
        protected override Dictionary<string, object> ToDict(List<string> fields)
        {
            var data = base.ToDict();
            if (Name == null)
            {
                throw new Exception("Team Name must be set");
            }
            else
            {
                data["name"] = Name;
                return CleanDict(data, fields);
            }
        }

        /// <summary>
        /// Populates the object with data retrieved from Spark.
        /// </summary>
        /// <param name="data">De-serialised data dictionary from Spark.</param>
        protected override void LoadDict(Dictionary<string, object> data)
        {
            base.LoadDict(data);
            Name = data["name"] as string;
        }

        /// <summary>
        /// Lists teams to which the authenticated user belongs.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="results">List of teams.</param>
        /// <param name="max">Limit the maximum number of teams.</param>
        /// <returns></returns>
        public static IEnumerator ListTeams(Action<SparkMessage> error, Action<List<Team>> results, int max = 0)
        {
            var constraints = new Dictionary<string, string>();
            if (max > 0)
            {
                constraints.Add("max", max.ToString());
            }

            var listObjects = ListObjects<Team>(constraints, SparkType.Team, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }
    }
}