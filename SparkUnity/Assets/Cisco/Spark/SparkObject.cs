using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cisco.Spark {
	public abstract class SparkObject {
		/// <summary>
		/// The Spark service's UID for this object.
		/// </summary>
		public string Id { get; internal set; }

		/// <summary>
		/// The DateTime at which this record was created on Spark.
		/// </summary>
		public DateTime Created { get; internal set; }

		/// <summary>
		/// Commits the object to the Spark service.
		/// This will create the object if it doesn't already exist.
		/// </summary>
		/// <param name="error">Error from Spark, if any.</param>
		/// <param name="success">Callback for completion.</param>
		public abstract IEnumerator Commit(Action<SparkMessage> error, Action<bool> success);

		/// <summary>
		/// Deletes the object from the Spark service.
		/// </summary>
		/// <param name="error">Error from Spark, if any.</param>
		/// <param name="success">Callback for completion.</param>
		public abstract IEnumerator Delete(Action<SparkMessage> error, Action<bool> success);

		/// <summary>
		/// Populates the current object with it's details from Spark, if it exists.
		/// </summary>
		/// <param name="error">Error from Spark, if any.</param>
		/// <param name="success">Callback for completion.</param>
		public abstract IEnumerator Load(Action<SparkMessage> error, Action<bool> success);

		/// <summary>
		/// Returns a dictionary representation of the object.
		/// </summary>
		/// <returns>The Dictionary.</returns>
		/// <param name="fields">A specific list of fields to serialise.</param>
		protected abstract Dictionary<string, object> ToDict (List<string> fields = null);

		/// <summary>
		/// Populates an object with data retrieved from Spark.
		/// </summary>
		/// <param name="data">Data.</param>
		protected abstract void LoadDict(Dictionary<string, object> data);
	}
}