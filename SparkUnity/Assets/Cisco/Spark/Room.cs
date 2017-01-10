using System.Collections;
using System.Collections.Generic;
using System;

namespace Cisco.Spark
{
    /// <summary>
    /// Virtual meeting place where Persons post Messages and collaborate.
    /// </summary>
    public class Room : SparkObject
    {
        /// <summary>
        /// Title of the Room.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Type of the Room (currently Direct or Group).
        /// </summary>
        public RoomType Type { get; set; }

        /// <summary>
        /// True if only moderators/creator can add people to the Room.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// The Team the Room is part of, if any.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// The DateTime of the last activity in the room.
        /// </summary>
        public DateTime LastActivity { get; private set; }

        /// <summary>
        /// The Person who created the Room.
        /// </summary>
        public Person Creator { get; private set; }

        internal override SparkType SparkType
        {
            get { return SparkType.Room; }
        }

        /// <summary>
        /// Constructor to build representation of existing Spark-side Room.
        /// Use Load() to populate rest of properties from Spark.
        /// </summary>
        /// <param name="id">Spark UID of the Room.</param>
        public Room(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Constructor to build a new Room locally.
        /// </summary>
        /// <param name="title">The title of the Room.</param>
        /// <param name="team">The team the room should be assigned to, if any.</param>
        public Room(string title, Team team)
        {
            Title = title;
            Team = team;
        }


        /// <summary>
        /// Lists all Messages in the Room matching the given constraints.
        /// </summary>
        /// <param name="error">Error callback from Spark, if any.</param>
        /// <param name="results">Results callback.</param>
        /// <param name="mentionedPeople">Will only return messages where these people are mentioned.</param>
        /// <param name="before">Will only return messages before this DateTime.</param>
        /// <param name="beforeMessage">Will only return messages before this Message was posted.</param>
        /// <param name="max">Maximum number of messages to return.</param>
        public IEnumerator ListMessages(Action<SparkMessage> error, Action<List<Message>> results, List<Person> mentionedPeople = null, DateTime? before = null, Message beforeMessage = null, int max = 0)
        {
            // Room must exist on Spark.
            if (Id == null)
            {
                throw new Exception("Room ID must be set before listing messages.");
            }

            // Search constraints.
            var constraints = new Dictionary<string, string>();
            constraints["roomId"] = Id;

            if (mentionedPeople != null)
            {
                var serialisedString = new List<string>();
                foreach (var person in mentionedPeople)
                {
                    serialisedString.Add(person.Id);
                }
                var queryString = MiniJSON.Json.Serialize(serialisedString);
                constraints["mentionedPeople"] = queryString;
            }

            if (before != null)
            {
                constraints["before"] = before.ToString();
            }

            if (beforeMessage != null)
            {
                constraints["beforeMessage"] = beforeMessage.Id;
            }

            if (max > 0)
            {
                constraints["max"] = max.ToString();
            }

            var listObjects = ListObjects<Message>(constraints, SparkType.Message, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }

        /// <summary>
        /// Returns a dictionary representation of the object.
        /// </summary>
        /// <returns>The Dictionary.</returns>
        /// <param name="fields">A specific list of fields to serialise.</param>
        protected override Dictionary<string, object> ToDict(List<string> fields = null)
        {
            // Constraint checks.
            if (Title == null)
            {
                throw new Exception("Room title can never be null.");
            }
            var data = base.ToDict();
            data["title"] = Title;
            return CleanDict(data, fields);
        }

        /// <summary>
        /// Populates an object with data retrieved from Spark.
        /// </summary>
        /// <param name="data">Data.</param>
        protected override void LoadDict(Dictionary<string, object> data)
        {
            base.LoadDict(data);
            Title = data["title"] as string;
            Type = RoomTypeExtensions.FromApi(data["type"] as string);
            IsLocked = (bool)data["isLocked"];
            LastActivity = DateTime.Parse(data["lastActivity"] as string);
            Creator = new Person(data["creatorId"] as string);
        }

        /// <summary>
        /// Lists all Rooms the registered user is a member of, matching the given filters.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="results">The restulting list of Rooms.</param>
        /// <param name="team">The Team to filter on.</param>
        /// <param name="max">The maximum number of rooms to return.</param>
        /// <param name="type">The RoomType of Room to look for.</param>
        public static IEnumerator ListRooms(Action<SparkMessage> error, Action<List<Room>> results, Team team = null, int max = 0, RoomType type = RoomType.Unsupported)
        {
            var constraints = new Dictionary<string, string>();
            if (team != null)
            {
                constraints.Add("teamId", team.Id);
            }

            if (max > 0)
            {
                constraints.Add("max", max.ToString());
            }

            if (type != RoomType.Unsupported)
            {
                constraints.Add("type", type.ToApi());
            }

            var listObjects = ListObjects<Room>(constraints, SparkType.Room, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }
    }
}