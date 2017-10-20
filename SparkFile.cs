using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cisco.Spark
{
    /// <summary>
    /// An uploaded file on Spark.
    /// </summary>
    public class SparkFile
    {
        /// <summary>
        /// UID of the uploaded file.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Public URL the file can be found at (for uploading).
        /// </summary>
        public Uri UploadUrl { get; set; }

        /// <summary>
        /// The filename of the SparkFile.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// The extension of the SparkFile.
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// The raw data of the file as a byte array.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The C# type this file can be best represented as.
        /// </summary>
        public Type ReturnType { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="Cisco.Spark.SparkFile"/> instance from a given Spark File ID.
        /// </summary>
        /// <param name="id">File ID</param>
        public SparkFile(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Generate a new SparkFile for upload from a public URL.
        /// </summary>
        /// <param name="url">Public URL the file can be found at.</param>
        public SparkFile(Uri url)
        {
            UploadUrl = url;
        }

        /// <summary>
        /// Generate a new SparkFile from local data.
        /// </summary>
        /// <param name="filename">Filename of the file.</param>
        /// <param name="data">File data as bytes.</param>
        public SparkFile(string filename, byte[] data)
        {
            Filename = filename;
            Data = data;
        }

        /// <summary>
        /// Download the File data.
        /// </summary>
        /// <param name="callback">File data as Unity specific object or bytes.</param>
        /// <param name="forceBytes">Stops intelligent conversion of file array (e.g png -> Texture) and forces returning of a raw byte array.</param>
        public IEnumerator Download(Action<object> callback, bool forceBytes = false)
        {
            var manager = Request.Instance;
            using (UnityWebRequest www = manager.Generate("contents/" + Id, UnityWebRequest.kHttpVerbGET))
            {
                yield return www.Send();
#if UNITY_5_4 || UNITY_5_5
                if (www.isError)
#else
                if (www.isNetworkError)
#endif
                {
                    Debug.LogError("Failed to Download File: " + www.error);
                }
                else
                {
                    // Get Headers.
                    var headers = www.GetResponseHeaders();
                    // Get data as bytes.
                    Data = www.downloadHandler.data;
                    // Get filename from Content-Disposition header.
                    Filename = headers["Content-Disposition"].Split('"')[1];
                    // Parse Extension.
                    Extension = Filename.Substring(Filename.LastIndexOf('.') + 1).ToLower();
                    // Figure Unity-native ReturnType.
                    SetReturnType();

                    // Convert data to ReturnType.
                    if (forceBytes)
                    {
                        // Specifically asked for bytes.
                        callback(Data);
                    }
                    else if (ReturnType == typeof(Texture))
                    {
                        // Downloaded File is a supported image.
                        var texture = new Texture2D(2, 2);
                        texture.LoadImage(Data);
                        callback(texture);
                    }
                    else
                    {
                        // TODO: Support more file types.
                        // Unsupported file type, returning bytes.
                        Debug.LogWarning(Extension + " is unsupported, returning bytes.");
                        ReturnType = typeof(Byte);
                        callback(Data);
                    }
                }
            }
        }

        /// <summary>
        /// Only retrieve the headers of the file.
        /// </summary>
        /// <param name="callback">Dictionary of headers.</param>
        public IEnumerator GetHeaders(Action<Dictionary<string, string>> callback)
        {
            var manager = Request.Instance;
            using (UnityWebRequest www = manager.Generate("contents/" + Id, UnityWebRequest.kHttpVerbHEAD))
            {
                yield return www.Send();
#if UNITY_5_4 || UNITY_5_5
                if (www.isError)
#else
                if (www.isNetworkError)
#endif
                {
                    Debug.LogError("Failed to Download File Headers: " + www.error);
                }
                else
                {
                    var headers = www.GetResponseHeaders();
                    Filename = headers["Content-Disposition"].Split('"')[1];
                    Extension = Filename.Substring(Filename.LastIndexOf('.') + 1).ToLower();
                    SetReturnType();
                    callback(headers);
                }
            }
        }

        /// <summary>
        /// Sets <see cref="ReturnType"/> from the <see cref="extension"/> of the file.
        /// </summary>
        void SetReturnType()
        {
            // Supported Return Types
            var models = new List<string> { "obj" };
            var images = new List<string> { "psd", "tiff", "jpg", "tga", "png", "gif", "bmp", "iff", "pict", "exr", "hdr" };

            if (images.Contains(Extension))
            {
                // Image -> Texture
                ReturnType = typeof(Texture);
            }
            else if (models.Contains(Extension))
            {
                // 3D Model -> Mesh
                ReturnType = typeof(GameObject);
            }
        }
    }
}
