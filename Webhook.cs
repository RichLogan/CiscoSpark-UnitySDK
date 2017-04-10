using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// A Webhook allows notification (via HTTP) when a specific event occurs on Spark.
    /// </summary>
    public class Webhook : SparkObject
    {
        /// <summary>
        /// The SparkType this SparkObject implementation represents.
        /// </summary>
        internal override SparkType SparkType
        {
            get { return SparkType.Webhook; }
        }

        /// <summary>
        /// A user-friendly name for this Webhook.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL that receives POST requests for each event.
        /// </summary>
        public Uri Target { get; set; }

        /// <summary>
        /// The resource type for the Webhook. Creating a webhook requires 'read' scope on the resource the webhook is for.
        /// </summary>
        public SparkType Resource { get; set; }

        /// <summary>
        /// The event type for the Webhook.
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// The filter that defines the webhook scope.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Secret used to generate payload signature.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Creates a Webhook from an existing Spark side Webhook.
        /// </summary>
        /// <param name="id">Spark UID of the Webhook.</param>
        public Webhook(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Create a new Webhook locally.
        /// </summary>
        /// <param name="name">A user-friendly name for this Webhook.</param>
        /// <param name="target">The URL that receives POST requests for each event.</param>
        /// <param name="resource">The resource type for the Webhook. Creating a webhook requires 'read' scope on the resource the webhook is for.</param>
        /// <param name="webhookEvent">The event type for the Webhook.</param>
        /// <param name="filter">The filter that defines the webhook scope.</param>
        /// <param name="secret">Secret used to generate payload signature.</param>
        public Webhook(string name, Uri target, SparkType resource, string webhookEvent, string filter = null, string secret = null)
        {
            Name = name;
            Target = target;
            Resource = resource;
            Event = webhookEvent;
            Filter = filter;
            Secret = secret;
        }

        /// <summary>
        /// Returns a dictionary representation of the object.
        /// </summary>
        /// <param name="fields">A specific list of fields to serialise.</param>
        /// <returns>The serialised object as a Dictionary.</returns>
        protected override Dictionary<string, object> ToDict(List<string> fields)
        {
            var data = base.ToDict();
            data["name"] = Name;
            data["targetUrl"] = Target.AbsoluteUri;
            data["targetUrl"] = Target.AbsoluteUri;
            data["resource"] = Resource.GetEndpoint();
            data["event"] = Event.ToString();
            data["filter"] = Filter;
            data["secret"] = Secret;
            return CleanDict(data, fields);
        }

        /// <summary>
        /// Populates the object with the given data.
        /// </summary>
        /// <param name="data">Dictionary of data to load.</param>
        protected override void LoadDict(Dictionary<string, object> data)
        {
            base.LoadDict(data);
            Name = data["name"] as string;
            Target = new Uri(data["targetUrl"] as string);
            Resource = SparkTypeExtensions.FromEndpoint(data["resource"] as string);
            Event = data["event"] as string;

            object filter;
            if (data.TryGetValue("filter", out filter))
            {
                Filter = filter as string;
            }

            object secret;
            if (data.TryGetValue("secret", out secret))
            {
                Secret = secret as string;
            }
        }

        /// <summary>
        /// List's Webhooks to which the authenticated user owns.
        /// </summary>
        /// <param name="error">Error from Spark, if any.</param>
        /// <param name="results">List of Webhooks.</param>
        /// <param name="max">Maximum number of Webhooks to retrieve.</param>
        public static IEnumerator ListWebhooks(Action<SparkMessage> error, Action<List<Webhook>> results, int max = 0)
        {
            var constraints = new Dictionary<string, string>();
            if (max > 0)
            {
                constraints.Add("max", max.ToString());
            }

            var listObjects = ListObjects<Webhook>(constraints, SparkType.Webhook, error, results);
            yield return Request.Instance.StartCoroutine(listObjects);
        }
    }
}