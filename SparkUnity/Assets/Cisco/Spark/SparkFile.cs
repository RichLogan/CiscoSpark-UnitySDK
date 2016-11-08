using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark {

	public class SparkFile {
		public string Id { get; set;}
		public string Filename { get; set;}
		public string Extension { get; set;}
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
		/// Download the File data.
		/// </summary>
		/// <param name="callback">File data as Unity specific object or bytes.</param>
		/// <param name="forceBytes">Stops intelligent conversion of file array (e.g png -> Texture) and forces returning of a raw byte array.</param>
		public IEnumerator Download(Action<object> callback, bool forceBytes = false) {
			var manager = Request.Instance;
			using (UnityWebRequest www = manager.Generate ("contents/" + Id, UnityWebRequest.kHttpVerbGET)) {
				yield return www.Send ();
				if (www.isError) {
					Debug.LogError ("Failed to Download File: " + www.error);
				} else {
					// Get Headers.
					var headers = www.GetResponseHeaders ();
					// Get data as bytes.
					Data = www.downloadHandler.data;
					// Get filename from Content-Disposition header.
					Filename = headers ["Content-Disposition"].Split ('"')[1];
					// Parse Extension.
					Extension = Filename.Substring(Filename.LastIndexOf ('.') + 1).ToLower ();
					// Figure Unity-native ReturnType.
					SetReturnType ();

					// Convert data to ReturnType.
					if (forceBytes) {
						// Specifically asked for bytes.
						callback (Data);
					} else if (ReturnType == typeof(Texture)) {
						// Downloaded File is a supported image.
						var texture = new Texture2D (2, 2);
						texture.LoadImage (Data);
						callback (texture);
					} else if (ReturnType == typeof(GameObject)) {
						// File should return a GameObject.
						if (Extension.Equals ("obj")) {
							// File is an Obj.
							var downloadedObject = new GameObject ();
							var objImport = downloadedObject.AddComponent<ObjLoad> ();
							var dataAsString = System.Text.Encoding.UTF8.GetString (Data);
							objImport.SetGeometryData (dataAsString);
							callback (downloadedObject);
						}
					} else {
						// TODO: Support more file types.
						// Unsupported file type, returning bytes.
						Debug.LogWarning(Extension + " is unsupported, returning bytes.");
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
					Extension = Filename.Substring(Filename.LastIndexOf ('.') + 1).ToLower ();
					SetReturnType ();
					callback (headers);
				}
			}
		}

		void SetReturnType() {
			// Supported Return Types
			var models = new List<string> {"obj"};
			var images = new List<string> {"psd", "tiff", "jpg", "tga", "png", "gif", "bmp", "iff", "pict", "exr", "hdr"};

			if (images.Contains (Extension)) {
				// Image -> Texture
				ReturnType = typeof(Texture);
			} else if (models.Contains (Extension)) {
				// 3D Model -> Mesh
				ReturnType = typeof(GameObject);
			}
		}
	}	
}
