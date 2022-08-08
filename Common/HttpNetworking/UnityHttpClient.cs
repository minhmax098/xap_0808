using System.IO;
using System.Net.Mime;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using System.Collections;
using EasyUI.Toast;
using UnityEngine.SceneManagement;

public static class UnityHttpClient
{
    public static string LOCAL_FILE_PATH = "file://";
    private static int defaultDurationForTokenTimeout = 4000;
    // public static async Task<TResponseType> CallAPI<TResponseType>(string url, string method, object requestData = null)
    // {
    //     try
    //     {
    //         // convert request data to byte array
    //         UnityWebRequest www = new UnityWebRequest(APIUrlConfig.DOMAIN_SERVER + url, method);
    //         if (requestData != null)
    //         {
    //             string requestDataString = JsonUtility.ToJson(requestData);
    //             byte[] requestDataBytes = Encoding.UTF8.GetBytes(requestDataString);
    //             www.uploadHandler = (UploadHandler)new UploadHandlerRaw(requestDataBytes);
    //         }
    //         www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
    //         www.SetRequestHeader(APIUrlConfig.CONTENT_TYPE_KEY, APIUrlConfig.JSON_CONTENT_TYPE_VALUE);
    //         www.SetRequestHeader(APIUrlConfig.AUTHORIZATION_KEY, PlayerPrefs.GetString(PlayerPrefConfig.userToken));

    //         var operation = www.SendWebRequest();

    //         while (!operation.isDone)
    //         {
    //             await Task.Yield();
    //         }

    //         if (www.result == UnityWebRequest.Result.ConnectionError)
    //         {
    //             // Only for error connection. Bad request will be handled (show message)
    //             throw new Exception(www.error);
    //         }
    //         TResponseType responseData = JsonUtility.FromJson<TResponseType>(www.downloadHandler.text);
    //         return responseData;
    //     }
    //     catch (Exception exception)
    //     {
    //         throw exception;
    //     }
    // }

    public static async Task<APIResponse<TypeData>> CallAPI<TypeData>(string url, string method, object requestData = null)
    {
        try
        {
            // convert request data to byte array
            UnityWebRequest www = new UnityWebRequest(APIUrlConfig.DOMAIN_SERVER + url, method);  
            if (requestData != null)
            {
                string requestDataString = JsonUtility.ToJson(requestData);
                byte[] requestDataBytes = Encoding.UTF8.GetBytes(requestDataString);
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(requestDataBytes);
            }
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader(APIUrlConfig.CONTENT_TYPE_KEY, APIUrlConfig.JSON_CONTENT_TYPE_VALUE);
            www.SetRequestHeader(APIUrlConfig.AUTHORIZATION_KEY, PlayerPrefs.GetString(PlayerPrefConfig.userToken));

            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                throw new Exception(www.error);
            }
            APIResponse<TypeData> responseData = JsonUtility.FromJson<APIResponse<TypeData>>(www.downloadHandler.text);
            if (responseData.code == APIUrlConfig.TOKEN_TIME_OUT_CODE)
            {
                Toast.ShowCommonToast(responseData.message, responseData.code); 
                await Task.Delay(defaultDurationForTokenTimeout);
                Helper.Logout();
            }
            else if (method != UnityWebRequest.kHttpVerbGET)
            {
                Toast.ShowCommonToast(responseData.message, responseData.code);
            }
            return responseData;
        }
        catch (Exception exception)
        {
            Toast.ShowCommonToast(exception.Message, APIUrlConfig.SERVER_ERROR_RESPONSE_CODE);
            throw exception;
        }
    }

    public static async Task<AudioClip> GetAudioClip(string audioURL)
    {
        try
        {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(APIUrlConfig.DOMAIN_SERVER + audioURL, AudioType.UNKNOWN);
            www.SetRequestHeader("Authorization", PlayerPrefs.GetString("user_token"));

            var operation = www.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new Exception(www.error);
            }
            Debug.Log($"{www}");
            Debug.Log($"{DownloadHandlerAudioClip.GetContent(www)}");
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            Debug.Log($"{audioClip == null}");
            return audioClip;
        }
        catch (Exception exception)
        {
            throw exception;
        }

    }

    public static IEnumerator LoadRawImageAsync(string imageURL, RawImage targetImage, Action<bool> callback)
    {
        bool isSuccess = false;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture) www.downloadHandler).texture;
            if (targetImage != null)
            {
                targetImage.texture = texture;
                isSuccess = true;
            }
        }
        callback?.Invoke(isSuccess);
    }
}