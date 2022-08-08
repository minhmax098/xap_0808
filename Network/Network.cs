using System.Collections;
using System.Collections.Generic;
using EasyUI.Toast;
using UnityEngine.SceneManagement;
using UnityEngine;

public static class Network
{
    static bool IsDisconnecting = false;
    public static void CheckNetWorkDisplayToast()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            Toast.ShowCommonToast(NetworkString.errorNetwork, APIUrlConfig.SERVER_ERROR_RESPONSE_CODE);
            IsDisconnecting = true;
        }
        else
        {
            if (IsDisconnecting)
            {
                Toast.ShowCommonToast(NetworkString.successReconect, APIUrlConfig.SUCCESS_RESPONSE_CODE);
                IsDisconnecting = false;
            }
        }
    }

    public static void CheckNetWorkMoveScence()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (SceneManager.GetActiveScene().name != SceneConfig.network)
                SceneManager.LoadScene(SceneConfig.network);
            else
                Toast.ShowCommonToast(NetworkString.reconnectFailed, APIUrlConfig.SERVER_ERROR_RESPONSE_CODE);
        }
        else
        {
            if (SceneManager.GetActiveScene().name == SceneConfig.network)
                SceneManager.LoadScene(SceneNameManager.prevScene);
        }
    }

    public static IEnumerator CronjobCheckNetWork()
    {
        SceneNameManager.setPrevScene(SceneManager.GetActiveScene().name); 
        Network.CheckNetWorkMoveScence();
        yield return new WaitForSeconds(5f);
    }

    public static IEnumerator CronjobCheckNetWorkDisPlayToast()
    {
        SceneNameManager.setPrevScene(SceneManager.GetActiveScene().name); 
        Network.CheckNetWorkDisplayToast();
        yield return new WaitForSeconds(10f);
    }
}
