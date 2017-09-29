using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Cisco.Spark
{
    /// <summary>
    /// Represents a <see cref="Person"/>s display picture on Spark.
    /// </summary>
    public class Avatar
    {
        /// <summary>
        /// URL of the Person's avatar.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The UnityEngine Texture of the person's avatar.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// True if the texture has already been downloaded.
        /// </summary>
        public bool Downloaded = false;

        /// <summary>
        /// True if already being downloaded.
        /// </summary>
        bool Locked = false;

        /// <summary>
        /// Callbacks that are queued to fire.
        /// </summary>
        List<Action<bool>> WaitingCallbacks;

        /// <summary>
        /// Builds an Avatar object from an image url.
        /// </summary>
        /// <param name="uri">Display picture URL.</param>
        internal Avatar(Uri uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// Downloads the Person's Avatar as a Texture from the Url.
        /// </summary>
        /// <param name="success">True if the download succeeded.</param>
        /// <param name="force">Optional: Force the texture to be redownloaded.</param>
        public IEnumerator Download(Action<SparkMessage> error, Action<bool> success, bool force = false)
        {
            if (Downloaded && !force)
            {
                success(true);
            }
            else
            {
                if (Locked)
                {
                    if (WaitingCallbacks == null) WaitingCallbacks = new List<Action<bool>>();
                    WaitingCallbacks.Add(success);
                }
                else
                {
                    Locked = true;
#if UNITY_5_4 || UNITY_5_5
                    var www = UnityWebRequest.GetTexture(Uri.AbsoluteUri);
#else
                    var www = UnityWebRequestTexture.GetTexture(Uri.AbsoluteUri);
#endif
                    yield return www.Send();

#if UNITY_5_4 || UNITY_5_5
                    if (www.isError)
#else
                    if (www.isNetworkError)
#endif
                    {
                        Debug.LogError("Failed to Download Avatar: " + www.error);
                        error(new SparkMessage(www));
                    }
                    else
                    {
                        var tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                        if (tex)
                        {
                            // Complete.
                            Texture = tex;
                            Downloaded = true;
                            success(true);
                            Locked = false;

                            // Notify waiting callbacks, if any.
                            if (WaitingCallbacks != null)
                            {
                                foreach (var callback in WaitingCallbacks) callback(true);
                                WaitingCallbacks = null;
                            }
                        }
                        else
                        {
                            Debug.LogError(www.downloadHandler.text + " (" + www.responseCode + ")");
                            error(new SparkMessage(www));
                        }
                    }
                }
            }
        }
    }
}