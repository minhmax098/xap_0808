using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using System.Reflection;
using System.Runtime.Versioning;

namespace BuildLesson
{
    public class BuildLessonManager : MonoBehaviour
    {
        public Button btnLabel;
        public Button btnSeparate;
        public Button btnXray;
        public Button btnAdd;
        public Animator toggleListItemAnimator;
        public GameObject record; 
        public GameObject saveRecord; 
        public GameObject addVideo; 
        public GameObject upload; 
        public GameObject addAudio; 
        private GameObject label2D;
        
        void Start()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            ObjectManagerBuildLesson.Instance.InitOriginalExperience();
            InitInteractions();
            InitEvents();
        }

        void Update()
        {
            // Check whether the pannel is opened 
            Debug.Log("Value of the animator: " + toggleListItemAnimator.GetBool(AnimatorConfig.isShowMeetingMemberList));
            label2D = GameObject.FindWithTag("Tag2D");
            Debug.Log("Label 2d: " + label2D);
            if (!toggleListItemAnimator.GetBool(AnimatorConfig.isShowMeetingMemberList) && 
                !record.activeSelf && 
                !saveRecord.activeSelf && 
                !addVideo.activeSelf && 
                !upload.activeSelf && 
                !addAudio.activeSelf && 
                label2D == null)
            {
                TouchHandler.Instance.HandleTouchInteraction();
            }
            EnableFeature();
        }

        private void UpdateTagFromServer(){
            Debug.Log("LOADING LABELS FROM SERVER " + ObjectManagerBuildLesson.Instance.OriginObject.name);  // Chay lai cai, chua in ra dong nay 
            TagHandler.Instance.GetTagFromServer(LessonManager.lessonId, ObjectManagerBuildLesson.Instance.OriginObject);
        }

        void EnableFeature()
        {
            if (ObjectManagerBuildLesson.Instance.CurrentObject != null)
            {
                if (ObjectManagerBuildLesson.Instance.CurrentObject.transform.childCount == 0)
                {
                    btnLabel.interactable = false;
                    btnSeparate.interactable = false;
                }
                else
                {
                    btnLabel.interactable = true;
                    btnSeparate.interactable = true;
                }
            }
        }

        void OnEnable()
        {
            // Mi viết hay mi clone đây =))) dua theo viet lai a, ben  ni  nói clone cha trớt đi, giờ sài ko biết sài 
            TouchHandler.onSelectChildObject += OnSelectChildObject;
            TreeNodeManager.onClickNodeTree += OnClickNodeTree;
            ObjectManagerBuildLesson.onResetObject += OnResetObject;
            ObjectManagerBuildLesson.onInitOrganSuccessfully += UpdateTagFromServer;
        }

        void OnDisable()
        {
            TouchHandler.onSelectChildObject -= OnSelectChildObject;
            TreeNodeManager.onClickNodeTree -= OnClickNodeTree; 
            ObjectManagerBuildLesson.onResetObject -= OnResetObject;
            ObjectManagerBuildLesson.onInitOrganSuccessfully -= UpdateTagFromServer;
        }

        void OnResetObject()
        {
            TreeNodeManager.Instance.ClearAllNodeTree();
            XRayManager.Instance.HandleXRayView(XRayManager.Instance.IsMakingXRay);
            Helper.ResetStatusFeature();
        }

        void OnSelectChildObject(GameObject selectedObject)
        {
            OnResetStatusFeature();
            TreeNodeManager.Instance.DisplaySelectedObject(selectedObject, ObjectManagerBuildLesson.Instance.CurrentObject);
            ObjectManagerBuildLesson.Instance.ChangeCurrentObject(selectedObject);
            TreeNodeManager.Instance.CreateChildNodeUI(selectedObject.name);
        }

        void OnClickNodeTree(string nodeName)
        {
            OnResetStatusFeature();
            XRayManager.Instance.HandleXRayView(false);
            if (nodeName != ObjectManagerBuildLesson.Instance.CurrentObject.name)
            {
                GameObject selectedObject = GameObject.Find(nodeName);
                TreeNodeManager.Instance.DisplayAllChildSelectedObject(selectedObject);
                // Conjoined
                ObjectManagerBuildLesson.Instance.CurrentObject = selectedObject;
                SeparateManagerBuildLesson.Instance.HandleSeparate(false);

                ObjectManagerBuildLesson.Instance.ChangeCurrentObject(selectedObject);
                TreeNodeManager.Instance.RemoveItem(nodeName);
                // StartCoroutine(Helper.MoveObject(Camera.main.gameObject, Camera.main.transform.position));
            }
            ObjectManagerBuildLesson.Instance.OriginObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        void OnResetStatusFeature()
        {
            LabelManagerBuildLesson.Instance.IsShowingLabel = false;
            LabelManagerBuildLesson.Instance.HandleLabelView(LabelManagerBuildLesson.Instance.IsShowingLabel);

            SeparateManagerBuildLesson.Instance.IsSeparating = false;
            SeparateManagerBuildLesson.Instance.HandleSeparate(SeparateManagerBuildLesson.Instance.IsSeparating);
        }

        void InitInteractions()
        {
            LabelManagerBuildLesson.Instance.IsShowingLabel = false;
        }

        void InitEvents()
        {
            btnAdd.onClick.AddListener(ToggleMenuAdd); 
            btnLabel.onClick.AddListener(HandleLabelView);
            btnSeparate.onClick.AddListener(HandleSeparation); 
            btnXray.onClick.AddListener(HandleXRayView);
        }

        void ToggleMenuAdd()
        {
            PopUpBuildLessonManager.Instance.IsClickedAdd = !PopUpBuildLessonManager.Instance.IsClickedAdd;
            PopUpBuildLessonManager.Instance.ShowListAdd(PopUpBuildLessonManager.Instance.IsClickedAdd);
        }

        void HandleLabelView()
        {
            LabelManagerBuildLesson.Instance.IsShowingLabel = !LabelManagerBuildLesson.Instance.IsShowingLabel;
            LabelManagerBuildLesson.Instance.HandleLabelView(LabelManagerBuildLesson.Instance.IsShowingLabel);
        }

        void HandleSeparation()
        {
            SeparateManagerBuildLesson.Instance.IsSeparating = !SeparateManagerBuildLesson.Instance.IsSeparating;
            SeparateManagerBuildLesson.Instance.HandleSeparate(SeparateManagerBuildLesson.Instance.IsSeparating);
        }

        void HandleXRayView()
        {
            XRayManagerBuildLesson.Instance.IsMakingXRay = !XRayManagerBuildLesson.Instance.IsMakingXRay;
            XRayManagerBuildLesson.Instance.HandleXRayView(XRayManagerBuildLesson.Instance.IsMakingXRay);
        }
    }
}
