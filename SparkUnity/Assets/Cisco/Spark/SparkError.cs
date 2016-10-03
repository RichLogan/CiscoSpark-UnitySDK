using UnityEngine;
using System.Collections.Generic;

namespace Cisco.Spark {
	public class SparkError {

		public string Description;

		public SparkError(Dictionary<string, object> data) {
			Description = data ["description"] as string;
		}
	}
}