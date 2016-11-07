using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark {
	public class SparkFile  {
		public string Id { get; set;}
		public string Filename { get; set;}
		public byte[] Data { get; set;}

		// Return helper
		public Type ReturnType;

		/// <summary>
		/// Initializes a new <see cref="Cisco.Spark.SparkFile"/> instance from a given Spark File ID.
		/// </summary>
		/// <param name="id">File ID</param>
		public SparkFile(string id) {
			Id = id;
		}

		public SparkFile(string filename, byte[] data) {
			Filename = filename;
			Data = data;
		}

		/// <summary>
		/// Download the File itself
		/// </summary>
		/// <param name="callback">File data as byte array</param>
		/// <param name="forceBytes">Stops intelligent conversion of file array (e.g png -> Texture) and forces returning of a raw byte array. </param>
		public IEnumerator Download(Action<object> callback, bool forceBytes = false) {
			var manager = Request.Instance;
			using (UnityWebRequest www = manager.Generate ("contents/" + Id, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Download File: " + www.error);
				} else {
					var headers = www.GetResponseHeaders ();
					Data = www.downloadHandler.data;
					Filename = headers ["Content-Disposition"].Split ('"')[1];
					string fileType = Filename.Split ('.')[1];
					if (fileType.Equals ("png") || fileType.Equals ("jpg")) {
						// Downloaded File is a supported image
						var texture = new Texture2D (2,2);
						texture.LoadImage (Data);
						ReturnType = typeof(UnityEngine.Texture2D);
						callback (texture);
					}
					else if (forceBytes) {
						// Asked for bytes
						ReturnType = typeof(Byte);
						callback (Data);
					} else {
						// TODO: Support more file types
						// No supported file type, returning bytes
						ReturnType = typeof(Byte);
						callback (Data);
					}
				}
			}
		}

		public IEnumerator GetHeaders(Action<Dictionary<string, string>> callback) {
			var manager = Request.Instance;
			using (UnityWebRequest www = manager.Generate ("contents/" + Id, UnityWebRequest.kHttpVerbHEAD)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Download File Headers: " + www.error);
				} else {
					var headers = www.GetResponseHeaders ();
					Filename = headers ["Content-Disposition"].Split ('"')[1];
					callback (headers);
				}
			}
		}
	}	
}
