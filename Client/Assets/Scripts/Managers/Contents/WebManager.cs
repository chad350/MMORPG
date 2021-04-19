using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager
{
    public string BaseUrl { get; set; } = "https://localhost:5001/api";

    public void SendPostRequest<T>(string url, object obj, Action<T> callback)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbPOST, obj, callback));
    }

    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> callback)
    {
        string sendUrl = $"{BaseUrl}/{url}";
        
        byte[] jsonBytes = null;
        if (obj != null)
        {
            string jsonStr = JsonUtility.ToJson(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            uwr.certificateHandler = new ForceAcceptAll();
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                T recv = JsonUtility.FromJson<T>(uwr.downloadHandler.text);
                callback.Invoke(recv);
            }
        }
    }
    
    public class ForceAcceptAll : CertificateHandler 
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        } 
    }
}
