using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Cisco.Spark
{
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

        public Avatar(Uri uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// Downloads the Person's Avatar as a Texture from the Url.
        /// </summary>
        /// <param name="success">True if the download succeeded.</param>
        /// <param name="force">Optional: Force the texture to be redownloaded.</param>
        public IEnumerator Download(Action<bool> success, bool force = false)
        {
            if (Downloaded && !force)
            {
                success(Texture);
            }
            else
            {
                var www = UnityWebRequest.GetTexture(Uri.AbsoluteUri);
                yield return www.Send();
                if (www.isError)
                {
                    Debug.LogError("Failed to Download Avatar: " + www.error);
                    success(false);
                }
                else
                {
                    var tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    if (tex)
                    {
                        Texture = tex;
                        Downloaded = true;
                        success(true);
                    }
                    else
                    {
                        // TODO: Find out if this can happen.
                        Debug.LogError("Failed. Check Implementation");
                        throw new NotSupportedException(www.downloadHandler.text);
                    }
                }
            }
        }
    }
}