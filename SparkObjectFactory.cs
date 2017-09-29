using System;
namespace Cisco.Spark
{
    class SparkObjectFactory
    {
        /// <summary>
        /// Make a new SparkObject from an ID.
        /// </summary>
        /// <param name="id">The ID of the SparkObject</param>
        /// <param name="type">The SparkType of the SparkObject.</param>
        /// <returns>The created/cached SparkObject instance.</returns>
        internal static SparkObject Make(string id, SparkType type)
        {
            SparkObject output;

            // If the object hasn't been cached, create it.
            if (!SparkObject._LocalCache.TryGetValue(id, out output))
            {
                switch (type)
                {
                    case SparkType.Membership:
                        output = new Membership();
                        break;
                    case SparkType.Message:
                        output = new Message();
                        break;
                    case SparkType.Person:
                        output = new Person();
                        break;
                    case SparkType.Room:
                        output = new Room();
                        break;
                    case SparkType.Team:
                        output = new Team();
                        break;
                    case SparkType.TeamMembership:
                        output = new TeamMembership();
                        break;
                    case SparkType.Webhook:
                        output = new Webhook();
                        break;
                    default:
                        throw new ArgumentException("Unsupported SparkType");
                }
                // Set Id.
                output.Id = id;
                // Cache.
                SparkObject._LocalCache.Add(id, output);
            }
            return output;
        }
    }
}
