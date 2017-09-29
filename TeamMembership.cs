using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// TeamMembership represents a <see cref="Person"/>'s relationship to a <see cref="Team"/>.
    /// </summary>
    public class TeamMembership : SparkObject
    {
        /// <summary>
        /// The <see cref="Team"/> the Membership belongs to.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// The <see cref="Person"/> the Membership belongs to.
        /// </summary>
        public Person Person { get; set; }

        /// <summary>
        /// True if the <see cref="Person"/> is a moderator of the Team.
        /// </summary>
        public bool IsModerator { get; set; }

        /// <summary>
        /// <see cref="SparkType"/> this <see cref="SparkObject"/> implementation represents.
        /// </summary>
        internal override SparkType SparkType
        {
            get { return SparkType.TeamMembership; }
        }

        /// <summary>
        /// Constructor to build representation of existing Spark-side TeamMembership.
        /// Use Load() to populate rest of properties from Spark.
        /// </summary>
        /// <param name="id">Spark UID of the TeamMembership.</param>
        public static TeamMembership FromId(string id)
        {
            return (TeamMembership)SparkObjectFactory.Make(id, SparkType.TeamMembership);
        }

        public TeamMembership() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cisco.Spark.TeamMembership"/> class.
        /// </summary>
        /// <param name="team">The Team of the TeamMembership.</param>
        /// <param name="person">The person belonging to the TeamMembership.</param>
        /// <param name="isModerator">True if this member will be a moderator.</param>
        public TeamMembership(Team team, Person person, bool isModerator = false)
        {
            Team = team;
            Person = person;
            IsModerator = isModerator;
        }

        /// <summary>
        /// Returns a dictionary representation of the object.
        /// </summary>
        /// <returns>The Dictionary.</returns>
        /// <param name="fields">A specific list of fields to serialise.</param>
        protected override Dictionary<string, object> ToDict(List<string> fields = null)
        {
            // Serialise to dictionary.
            var data = base.ToDict();
            data["teamId"] = Team.Id;
            data["personId"] = Person.Id;
            data["isModerator"] = IsModerator;
            return CleanDict(data, fields);
        }

        /// <summary>
        /// Populates the TeamMembership with data from Spark.
        /// </summary>
        /// <param name="data">Dictionary of TeamMembership data.</param>
        protected override void LoadDict(Dictionary<string, object> data)
        {
            base.LoadDict(data);
            var teamId = data["teamId"] as string;
            Team = Team.FromId(teamId);
            var personId = data["personId"] as string;
            Person = Person.FromId(personId);
            IsModerator = (bool)data["isModerator"];
        }

        /// <summary>
        /// Lists all TeamMemberships matching the given criteria.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="results">List of TeamMemberships.</param>
        /// <param name="team">The Team to show TeamMemberships for.</param>
        /// <param name="person">The Person to show TeamMemberships for.</param>
        /// <param name="max">The maximum number of TeamMemberships to return.</param>
        /// <returns></returns>
		public static IEnumerator ListTeamMemberships(Action<SparkMessage> error, Action<List<TeamMembership>> results, Team team = null, Person person = null, int max = 0)
        {
            var constraints = new Dictionary<string, string>();
            if (team != null)
            {
                constraints.Add("teamId", team.Id);
            }
            else if (person != null)
            {
                constraints.Add("personId", person.Id);
            }

            if (max > 0)
            {
                constraints.Add("max", max.ToString());
            }

            var listObjects = ListObjects(constraints, SparkType.TeamMembership, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }
    }
}
