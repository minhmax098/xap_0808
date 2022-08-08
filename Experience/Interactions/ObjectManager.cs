using System.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Net;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TriLibCore;
using System.Linq;
using System.IO;
using Photon.Pun;
using System.Threading.Tasks;
using EasyUI.Toast;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager instance;
    public static ObjectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectManager>();
            }
            return instance;
        }
    }

    public const float X_SIZE_BOUND = 2.5f;
    public const float Y_SIZE_BOUND = 2.5f;
    public const float Z_SIZE_BOUND = 2.5f;
    public float FactorScaleInitial { get; set; }
    Bounds boundOriginObject;

    private const float TIME_SCALE_FOR_APPEARANCE = 0.04f;
    public static event Action onReadyModel;
    public static event Action<string> onChangeCurrentObject;
    public Material OriginOrganMaterial { get; set; }
    public GameObject OriginObject { get; set; }
    public List<Vector3> ListchildrenOfOriginPosition = new List<Vector3>();
    public GameObject CurrentObject { get; set; }
    public Vector3 OriginPosition { get; set; }
    public Quaternion OriginRotation { get; set; }
    public Vector3 OriginScale { get; set; }

    public static event Action<GameObject> onLoadedObjectAtRuntime;
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Instantiate object at specified position/rotation in AR mode
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void InstantiateARObject(Vector3 position, Quaternion rotation, bool isHost)
    {
        OriginObject.transform.position = position;
        OriginObject.transform.rotation = rotation;
        if (isHost && (!ARUIManager.Instance.IsStartAR))
        {
            OriginObject.transform.localScale *= ModelConfig.scaleFactorInARMode;
        }
        OriginObject.SetActive(true);
        AddARAnchorToObject();
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Show zoom up affect for object
    /// </summary>
    public void ShowZoomUpAffectForObject()
    {
        StartCoroutine(Helper.EffectScaleObject(OriginObject, TIME_SCALE_FOR_APPEARANCE, OriginObject.transform.localScale));
    }

    public void Instantiate3DObject()
    {
        OriginObject.transform.position = OriginPosition;
        OriginObject.transform.rotation = OriginRotation;
        OriginObject.transform.localScale = OriginScale;
        OriginObject.SetActive(true);
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Add ARAnchor component to object
    /// </summary>
    public void AddARAnchorToObject()
    {
        if (OriginObject != null)
        {
            if (OriginObject.GetComponent<ARAnchor>() == null)
            {
                ARAnchor localAnchor = OriginObject.AddComponent<ARAnchor>();
                localAnchor.destroyOnRemoval = false;
            }
        }
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Get ARAnchor component from object
    /// </summary>
    /// <returns></returns>
    public ARAnchor GetARAnchorComponent()
    {
        if (OriginObject == null)
        {
            return null;
        }
        if (OriginObject.GetComponent<ARAnchor>() == null)
        {
            AddARAnchorToObject();
        }
        return OriginObject.GetComponent<ARAnchor>();
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Destroy original object
    /// </summary>
    public void DestroyOriginalObject()
    {
        if (OriginObject != null)
        {
            Destroy(OriginObject);
        }
        if (CurrentObject != null)
        {
            Destroy(CurrentObject);
        }
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Destroy AR Anchor component
    /// </summary>
    public void DestroyARAnchorComponent()
    {
        if (OriginObject != null)
        {
            ARAnchor localAnchor = OriginObject.GetComponent<ARAnchor>();
            if (localAnchor != null)
            {
                Destroy(localAnchor);
            }
        }
    }
    public void SetCollider(GameObject objectInstance)
    {
        objectInstance.tag = TagConfig.ORGAN_TAG;
        var transforms = objectInstance.GetComponentsInChildren<Transform>();
        if (transforms.Length <= 0)
        {
            return;
        }

        foreach (var item in transforms)
        {
            item.gameObject.AddComponent<MeshCollider>();
            item.tag = TagConfig.ORGAN;
        }
    }

    public void ScaleObjectWithBound(GameObject objectInstance)
    {
        boundOriginObject = Helper.CalculateBounds(objectInstance);
        FactorScaleInitial = Mathf.Min(Mathf.Min(X_SIZE_BOUND / boundOriginObject.size.x, Y_SIZE_BOUND / boundOriginObject.size.y), Z_SIZE_BOUND / boundOriginObject.size.z);
        objectInstance.transform.localScale = objectInstance.transform.localScale * FactorScaleInitial;
    }

    /// <summary>
    /// Assign organ model to gameobject
    /// </summary>
    public void InitGameObject(GameObject newGameObject)
    {
        OriginObject = newGameObject;

        OriginObject.name = StaticLesson.LessonTitle;
        ScaleObjectWithBound(OriginObject);
        SetCollider(OriginObject);
        XRayManager.Instance.DictionaryMaterialOriginal.Clear();
        XRayManager.Instance.GetOriginalMaterial(OriginObject);
        OriginScale = OriginObject.transform.localScale;
        ChangeCurrentObject(OriginObject);
        onReadyModel?.Invoke();
    }

    public void ChangeCurrentObject(GameObject newGameObject)
    {
        try
        {
            CurrentObject = newGameObject;
            ListchildrenOfOriginPosition = Helper.GetListOfInitialPositionOfChildren(CurrentObject);
            onChangeCurrentObject?.Invoke(CurrentObject.name);
        }
        catch (Exception e)
        {
            Debug.Log($"sonvdh change error {e.Message}");
        }
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Load object at runtime
    /// </summary>
    public async Task LoadObjectAtRunTime(string objectURL)
    {
        int lastIndex = objectURL.LastIndexOf("/", StringComparison.Ordinal);
        string modelName = objectURL.Remove(0, lastIndex + 1);
        string modelPathInLocalSource = Application.persistentDataPath + "/" + modelName; // path to model fil
        if (!File.Exists(modelPathInLocalSource))
        {
            // If model was not downloaded (not exist in local), then download it
            await DownloadObjectFromLogicServer(objectURL, modelPathInLocalSource);
        }

        // Load file from local to unity application
        StartCoroutine(LoadObjectFromLocal(modelPathInLocalSource));
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Download model from server
    /// </summary>
    async Task DownloadObjectFromLogicServer(string objectURL, string modelPathInLocalSource)
    {
        try
        {
            Debug.Log($"sonvdh Starting download model {objectURL}");
            WebClient client = new WebClient();
            await client.DownloadFileTaskAsync(new Uri(objectURL), modelPathInLocalSource);
        }
        catch (Exception exception)
        {
            Toast.ShowCommonToast(exception.Message, APIUrlConfig.SERVER_ERROR_RESPONSE_CODE);
        }
    }

    /// <summary>
    /// Author: sonvdh
    /// Purpose: Load model file from local to unity application
    /// </summary>
    IEnumerator LoadObjectFromLocal(string modelPathInLocalSource)
    {
        yield return new WaitForSeconds(0.3f);
        try
        {
            Debug.Log($"sonvdh Starting load object from local {modelPathInLocalSource}");
            AssetLoaderContext assetLoaderContext = AssetLoader.LoadModelFromFile(
                modelPathInLocalSource,
                assetLoaderContext =>
                {
                    GameObject loadedObject = assetLoaderContext.RootGameObject;
                    loadedObject.SetActive(false);
                },
                assetLoaderContext =>
                {
                    GameObject loadedObject = assetLoaderContext.RootGameObject;
                    loadedObject.SetActive(false);
                },
                (assetLoaderContext, progress) =>
                {
                    Debug.Log($"sonvdh process {progress}");
                    GameObject loadedObject = assetLoaderContext.RootGameObject;
                    if (progress == 1)
                    {
                        GameObject loadedObjectWithMaterials = loadedObject.transform.GetChild(0).gameObject;
                        onLoadedObjectAtRuntime?.Invoke(loadedObjectWithMaterials);
                        Destroy(loadedObject);
                    }
                    else
                    {
                        loadedObject.SetActive(false);
                    }
                },
                iContextualizedError =>
                {
                    Debug.Log($"sonvdh error loading model {iContextualizedError.GetInnerException().Message}");
                },
                null,
                AssetLoader.CreateDefaultLoaderOptions(),
                null,
                true
            );
        }
        catch (Exception exception)
        {
            Toast.ShowCommonToast(exception.Message, APIUrlConfig.SERVER_ERROR_RESPONSE_CODE);
        }
    }

    public bool CheckObjectHaveChild(GameObject obj)
    {
        if (obj.transform.childCount < 1)
            return false;
        else
        {
            foreach (Transform child in obj.transform)
            {
                if (child.gameObject.tag != TagConfig.LABEL_TAG)
                    return true;
            }
            return false;
        }
    }
}
