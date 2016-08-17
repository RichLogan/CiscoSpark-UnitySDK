using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace Cisco.Spark {
	public class SparkFile  {
		public string Id { get; set;}
		public string Filename { get; set;}
		public byte[] Data { get; set;}

		// Return helper
		public Type returnType;

		/// <summary>
		/// Initializes a new <see cref="Cisco.Spark.SparkFile"/> instance from a given Spark File ID.
		/// </summary>
		/// <param name="id">File ID</param>
		public SparkFile(string id) {
			Id = id;
		}

		public SparkFile(byte[] data) {
			
		}

		/// <summary>
		/// Download the File itself
		/// </summary>
		/// <param name="callback">File data as byte array</param>
		/// <param name="forceBytes">Stops intelligent conversion of file array (e.g png -> Texture) and forces returning of a raw byte array. </param>
		public IEnumerator Download(Action<object> callback, bool forceBytes = false) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("contents/" + Id, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Download File: " + www.error);
				} else {
					Dictionary<string, string> headers = www.GetResponseHeaders ();
					Data = www.downloadHandler.data;
					Filename = headers ["Content-Disposition"].Split ('"')[1];
					string fileType = Filename.Split ('.')[1];
					if (fileType.Equals ("png") || fileType.Equals ("jpg")) {
						// Downloaded File is a supported image
						Texture2D texture = new Texture2D (2,2);
						texture.LoadImage (Data);
						returnType = typeof(UnityEngine.Texture2D);
						callback (texture);
					}
					else if (forceBytes) {
						// Asked for bytes
						returnType = typeof(System.Byte);
						callback (Data);
					} else {
						// TODO: Support more file types
						// No supported file type, returning bytes
						returnType = typeof(System.Byte);
						callback (Data);
					}
				}
			}
		}

		public IEnumerator GetHeaders(Action<Dictionary<string, string>> callback) {
			Request manager = GameObject.FindObjectOfType<Request> ();
			using (UnityWebRequest www = manager.Generate ("contents/" + Id, UnityWebRequest.kHttpVerbHEAD)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Download File Headers: " + www.error);
				} else {
					Dictionary<string, string> headers = www.GetResponseHeaders ();
					Filename = headers ["Content-Disposition"].Split ('"')[1];
					callback (headers);
				}
			}
		}
	}	
}
