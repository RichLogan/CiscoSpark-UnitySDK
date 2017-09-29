using System;
using System.Collections.Generic;
using System.Collections;

namespace Cisco.Spark
{
    /// <summary>
    /// Membership represents a <see cref="Person"/>'s relationship to a <see cref="Room"/>.
    /// </summary>
    public class Membership : SparkObject
    {
        /// <summary>
        /// The Room the Membership belongs to.
        /// </summary>
        public Room Room { get; set; }

        /// <summary>
        /// The Person the Membership belongs to.
        /// </summary>
        public Person Person { get; set; }

        /// <summary>
        /// True if the Person is a moderator of the Room.
        /// </summary>
        public bool IsModerator { get; set; }

        /// <summary>
        /// True if the Person is a monitor of the Room.
        /// </summary>
        public bool IsMonitor { get; set; }

        /// <summary>
        /// The SparkType this SparkObject implementation represents.
        /// </summary>
        /// <returns></returns>
        internal override SparkType SparkType
        {
            get
            {
                return SparkType.Membership;
            }
        }

        /// <summary>
        /// Create a Membership from an existing Spark membership ID.
        /// </summary>
        /// <param name="id">Spark UID of a Membership.</param>
        /// <returns>The Membership.</returns>
        public static Membership FromId(string id)
        {
            return (Membership)SparkObjectFactory.Make(id, SparkType.Membership);
        }

        public Membership() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cisco.Spark.Membership"/> class.
        /// </summary>
        /// <param name="room">The Room of the membership.</param>
        /// <param name="person">The person belonging to the membership.</param>
        /// <param name="isModerator">True if this member is a moderator.</param>
        /// /// <param name="isMonitor">True if this member is a monitor.</param>
        public Membership(Room room, Person person, bool isModerator = false, bool isMonitor = false)
        {
            Room = room;
            Person = person;
            IsModerator = isModerator;
            IsMonitor = isMonitor;
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
            data["roomId"] = Room.Id;
            data["personId"] = Person.Id;
            data["isModerator"] = IsModerator;
            return CleanDict(data, fields);
        }

        /// <summary>
        /// Populates the Membership with data from Spark.
        /// </summary>
        /// <param name="data">Dictionary of Membership data.</param>
		protected override void LoadDict(Dictionary<string, object> data)
        {
            base.LoadDict(data);
            var roomId = data["roomId"] as string;
            Room = Room.FromId(roomId);
            var personId = data["personId"] as string;
            Person = Person.FromId(personId);
            IsModerator = (bool)data["isModerator"];
            IsMonitor = (bool)data["isMonitor"];
        }

        /// <summary>
        /// Lists all Memberships matching the given criteria.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="results">List of Memberships.</param>
        /// <param name="room">The Room to show Memberships for.</param>
        /// <param name="person">The person to show Memberships for.</param>
        /// <param name="max">The maximum number of Memberships to return.</param>
        /// <returns></returns>
        public static IEnumerator ListMemberships(Action<SparkMessage> error, Action<List<Membership>> results, Room room = null, Person person = null, int max = 0)
        {
            var constraints = new Dictionary<string, string>();
            if (room != null)
            {
                constraints.Add("roomId", room.Id);
            }
            else if (person != null)
            {
                constraints.Add("personId", person.Id);
            }

            if (max > 0)
            {
                constraints.Add("max", max.ToString());
            }

            var listObjects = ListObjects(constraints, SparkType.Membership, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }
    }
}
