using System;
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

            // Author.
            data["personId"] = Author.Id;

            // Message Content.
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
            if (Mentions != null && Mentions.Count > 0) {
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

            // Message Content.
            Text = data["text"] as string;

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
    }
}