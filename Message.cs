using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// A single message in a group Room or directly to a Person.
    /// </summary>
    public class Message : SparkObject
    {
        /// <summary>
        /// SparkType the implementation represents.
        /// </summary>
        internal override SparkType SparkType
        {
            get { return SparkType.Message; }
        }

        /// <summary>
        /// The Room the message belongs to if it is a general message.
        /// </summary>
        public Room Room { get; set; }

        /// <summary>
        /// The recipient if the message is a 1:1 room / direct message.
        /// </summary>
        public Person Recipient { get; set; }

        /// <summary>
        /// The author of the message.
        /// </summary>
        public Person Author { get; set; }

        /// <summary>
        /// The plain text of the message.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The message in Markdown form.
        /// </summary>
        public string Markdown { get; set; }

        /// <summary>
        /// The message in HTML form.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// List of uploaded files attached with the message.
        /// </summary>
        public List<SparkFile> Files { get; set; }

        /// <summary>
        /// List of people mentioned in the message.
        /// </summary>
        public List<Person> Mentions { get; set; }

        /// <summary>
        /// Creates a representation of existing Spark-side Message.
        /// Use <see cref="Load"/> to populate rest of properties from Spark.
        /// </summary>
        /// <param name="id">Spark UID of the Message.</param>
        public Message(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Creates a new Message posted into a given Room.
        /// </summary>
        /// <param name="room">The Room to post the Message into.</param>
        public Message(Room room)
        {
            Room = room;
            Author = Person.AuthenticatedUser;
        }

        /// <summary>
        /// Creates a new Message sent to a given Person via a 1:1 / Direct Room.
        /// </summary>
        /// <param name="person">The Person to send the Message to.</param>
        public Message(Person person)
        {
            Recipient = person;
            Author = Person.AuthenticatedUser;
        }

        protected override Dictionary<string, object> ToDict(List<string> fields)
        {
            var data = base.ToDict();

            // Destination.
            if (Room != null)
            {
                data["roomId"] = Room.Id;
            }
            else if (Recipient != null)
            {
                data["toPersonId"] = Recipient.Id;
            }
            else
            {
                throw new Exception("Message must have a destination");
            }

            // Author.
            data["personId"] = Author.Id;

            // Message Content.

            // Content check.
            if ((Files == null || Files.Count == 0) && (Text == null || Text == ""))
            {
                throw new Exception("A message must have content (text and/or files)");
            }

            data["text"] = Text;
            data["markdown"] = Markdown;

            // Uploaded Files.
            if (Files != null && Files.Count > 0)
            {
                var fileUrls = new List<string>();
                foreach (var file in Files)
                {
                    fileUrls.Add(file.UploadUrl.AbsoluteUri);
                }
                data["files"] = fileUrls;
            }

            // Mentions.
            if (Mentions != null && Mentions.Count > 0)
            {
                var personIds = new List<string>();
                foreach (var person in Mentions)
                {
                    personIds.Add(person.Id);
                }
                data["mentionedPeople"] = personIds;
            }

            return CleanDict(data, fields);
        }

        protected override void LoadDict(Dictionary<string, object> data)
        {
            base.LoadDict(data);

            // Destination.
            if (data.ContainsKey("roomId"))
            {
                Room = new Room(data["roomId"] as string);
            }
            else if (data.ContainsKey("toPersonId"))
            {
                Recipient = new Person(data["toPersonId"] as string);
            }

            // Author.
            Author = new Person(data["personId"] as string);

            // Message Content (can be blank when uploading files).
            object text;
            if (data.TryGetValue("text", out text))
            {
                Text = text as string;
            }

            object markdown;
            if (data.TryGetValue("markdown", out markdown))
            {
                Markdown = markdown as string;
            }

            object html;
            if (data.TryGetValue("html", out html))
            {
                Html = html as string;
            }

            // Uploaded Files.
            object files;
            if (data.TryGetValue("files", out files))
            {
                var extractedFiles = files as List<object>;
                var tempFiles = new List<SparkFile>();
                foreach (var urlString in extractedFiles)
                {
                    var url = new Uri(urlString as string);
                    tempFiles.Add(new SparkFile(url));
                }
                Files = tempFiles;
            }

            // Mentions
            object mentions;
            if (data.TryGetValue("mentionedPeople", out mentions))
            {
                var extractedIds = mentions as List<object>;
                var tempPeople = new List<Person>();
                foreach (var id in extractedIds)
                {
                    var stringId = id as string;
                    tempPeople.Add(new Person(stringId));
                }
                Mentions = tempPeople;
            }
        }

        /// <summary>
        /// Lists all Message matching given criteria.
        /// </summary>
        /// <param name="room">The room to pull from.</param>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="results">Callback for the list of messages.</param>
        /// <param name="mentionedPeople">Will only return messages where these people are mentioned.</param>
        /// <param name="before">Will only return messages sent before this DateTime.</param>
        /// <param name="beforeMessage">Will only return messages from before this Message.</param>
        /// <param name="max">Maximum number of messages to return.</param>
        public static IEnumerator ListMessages(Room room, Action<SparkMessage> error, Action<List<Message>> results, List<Person> mentionedPeople = null, DateTime? before = null, Message beforeMessage = null, int max = 0)
        {
            // Room must exist on Spark.
            if (room.Id == null)
            {
                throw new Exception("Must pass a valid Room to ListMessages");
            }

            // Search constraints.
            var constraints = new Dictionary<string, string>();
            constraints["roomId"] = room.Id;

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

            // Make request.
            var listObjects = ListObjects<Message>(constraints, SparkType.Message, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }
    }
}