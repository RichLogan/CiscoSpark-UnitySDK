using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark {
	public class SparkMessage {
		public string Message;
		public IEnumerable Errors;
		public string TrackingId;
	
		public SparkMessage (Dictionary<string, object> data) {
			Message = (string) data ["message"];
			Errors = new List<SparkError> ();

			object errors;
			if (data.TryGetValue ("errors", out errors)) {
				var listOfErrors = errors as List<object>;
				var errorList = new List<SparkError> ();
				foreach (var error in listOfErrors) {
					errorList.Add (new SparkError(error as Dictionary<string, object>));
				}
				Errors = errorList;
			}
			TrackingId = (string) data ["trackingId"];
		}
	}
}