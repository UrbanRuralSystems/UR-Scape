﻿// Copyright (C) 2019 Singapore ETH Centre, Future Cities Laboratory
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Michael Joos  (joos@arch.ethz.ch)

using System.Collections;
using System.IO;
using UnityEngine;

public class TileProvider : ResourceProvider<TileRequest>
{
    private MonoBehaviour behaviour;
    private TextureCache cache;

    public TileProvider(MonoBehaviour behaviour, TextureCache cache)
    {
        this.behaviour = behaviour;
        this.cache = cache;
    }

    public void Run(TileRequest request, ProviderCallback<TileRequest> callback)
    {
		// Try finding the texture in the cache
		if (cache.TryRemove(request.id, out Texture texture))
		{
			request.texture = texture;
			request.State = RequestState.Succeeded;
			callback(request);
			return;
		}

#if UNITY_STANDALONE || UNITY_IOS
		// If not, try loading it on disk
		if (File.Exists(request.file))
        {
            ReadFromDisk(request);
            callback(request);
            return;
        }
#endif

		// Otherwise, get it from the url (if there's access to the internet)
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			request.State = RequestState.Canceled;
			callback(request);
		}
		else
		{
			behaviour.StartCoroutine(GetFromURL(request, callback));
		}
	}

    private void ReadFromDisk(TileRequest request)
    {
        var data = File.ReadAllBytes(request.file);

        if (!request.IsCanceled)
        {
            request.SetData(data);
            request.State = RequestState.Succeeded;
        }
    }

    private void AddToDisk(TileRequest request, byte[] data)
    {
#if UNITY_STANDALONE || UNITY_IOS
        if (!string.IsNullOrEmpty(request.file))
        {
			var dirName = Path.GetDirectoryName(request.file);
#if UNITY_STANDALONE_OSX
			dirName = dirName.Replace('\\', Path.DirectorySeparatorChar);
#endif
			Directory.CreateDirectory(dirName);
            File.WriteAllBytes(request.file, data);
        }
#endif
    }

    private IEnumerator GetFromURL(TileRequest request, ProviderCallback<TileRequest> callback)
    {
		using (var webRequest = UnityEngine.Networking.UnityWebRequest.Get(request.url))
		{
			// Request and wait for the desired page.
			yield return webRequest.SendWebRequest();

			if (!webRequest.isNetworkError && !webRequest.isHttpError && webRequest.responseCode == 200)    // 200 = HttpStatusCode.OK
			{
				request.SetData(webRequest.downloadHandler.data);
				request.State = RequestState.Succeeded;

				// Add it to cache and disk
				AddToDisk(request, webRequest.downloadHandler.data);
			}
			else
			{
				request.Error = "Response (code " + webRequest.responseCode + "): " + webRequest.error;
				request.State = RequestState.Failed;
			}

			callback(request);
		}
	}

}
