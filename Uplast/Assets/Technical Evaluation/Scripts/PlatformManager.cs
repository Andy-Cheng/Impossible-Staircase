using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class PlatformManager
{
    private static readonly string serverURL = "http://192.168.50.32/";

    public static IEnumerator SendRequestToServer(string functionName, string parameter){
        string url = serverURL + functionName + "?" + parameter;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError) {
                Debug.Log("Network error");
            } else {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
    
}
