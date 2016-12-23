using Cisco.Spark;

namespace Cisco.Spark {
	public class Room : SparkObject {

		public override System.Collections.IEnumerator Commit (System.Action<SparkMessage> error, System.Action<bool> success)
		{
			throw new System.NotImplementedException ();
		}

		public override System.Collections.IEnumerator Delete (System.Action<SparkMessage> error, System.Action<bool> success)
		{
			throw new System.NotImplementedException ();
		}

		public override System.Collections.IEnumerator Load (System.Action<SparkMessage> error, System.Action<bool> success)
		{
			throw new System.NotImplementedException ();
		}

		protected override void LoadDict (System.Collections.Generic.Dictionary<string, object> data)
		{
			throw new System.NotImplementedException ();
		}

		protected override System.Collections.Generic.Dictionary<string, object> ToDict (System.Collections.Generic.List<string> fields)
		{
			throw new System.NotImplementedException ();
		}
	}
}

