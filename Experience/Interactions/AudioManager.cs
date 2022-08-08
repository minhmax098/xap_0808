using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<AudioManager>();
            return instance;
        }
    }

    AudioSource audioPlayer;
    public bool IsPlayingAudio { get; set; }
    public bool IsDisplayAudio { get; set; }

    // UI
    public Text timeCurrentAudio;
    public Text timeEndAudio;
    public GameObject sliderControlAudio;
    public Button btnControlAudio;
    public GameObject panelAudio;
    public GameObject audioSource;
    public GameObject btnAudio;
    public GameObject panelLoading;
    public Button btnExitAudio;
    float timeCurrentPlay;

    private void Awake()
    {
        audioPlayer = audioSource.AddComponent<AudioSource>();
        audioPlayer.playOnAwake = false;
        audioPlayer.loop = true;
    }

    void Update()
    {
        StartCoroutine(Network.CronjobCheckNetWorkDisPlayToast());
        if (audioPlayer.clip != null && IsDisplayAudio)
        {
            SetValueForSlider(audioPlayer.time);
            StartCoroutine(SetValueForSliderByDragHandle());
        }
    }

    IEnumerator SetValueForSliderByDragHandle()
    {
        sliderControlAudio.GetComponent<Slider>().onValueChanged.AddListener((value) =>
        {
            audioPlayer.time = value;
        });
        yield return new WaitForSeconds(3);

        sliderControlAudio.GetComponent<Slider>().value = (float)audioPlayer.time;
    }

    void SetValueForSlider(float value)
    {
        timeCurrentAudio.text = Helper.FormatTime(value);
        sliderControlAudio.GetComponent<Slider>().value = value;
    }

    async void GetAudioClip(string audioURL)
    {
        try
        {
            audioPlayer.clip = await UnityHttpClient.GetAudioClip(audioURL);

            panelLoading.SetActive(false);
            timeEndAudio.GetComponent<Text>().text = Helper.FormatTime(audioPlayer.clip.length);
            sliderControlAudio.GetComponent<Slider>().maxValue = audioPlayer.clip.length;
            btnControlAudio.interactable = true;
            btnExitAudio.interactable = true;

            IsPlayingAudio = true;
            ControlAudio(IsPlayingAudio);
        }
        catch (Exception exception)
        {
            Debug.Log($"quyen debug: {exception.Message}");
        }
    }



    public void ShowAudioByClickItemMedia(MediaOrganItem dataItemVideo, GameObject itemMedia)
    {
        if (audioPlayer.clip != null)
            ResetAudio();

        // Hide list media
        PopupManager.Instance.IsClickedMenu = false;
        PopupManager.Instance.ShowListMedia(PopupManager.Instance.IsClickedMenu);

        // Set UI Audio
        IsDisplayAudio = true;
        panelAudio.SetActive(true);
        panelLoading.SetActive(true);
        btnControlAudio.interactable = false;
        btnExitAudio.interactable = false;
        btnAudio.SetActive(false);

        // Show icon tick on item selected
        itemMedia.transform.GetChild(2).gameObject.SetActive(true);

        // Get Audio clip from url
        GetAudioClip(dataItemVideo.AudioURL);
    }

    // Show panel audio
    public void ShowAudioCurrent(bool _IsDisplayAudio)
    {
        IsDisplayAudio = _IsDisplayAudio;
        if (IsDisplayAudio)
        {
            audioPlayer.time = timeCurrentPlay;
            btnAudio.SetActive(false);
            panelAudio.SetActive(true);
        }
        else
        {
            Debug.Log($"{audioPlayer}");
            if (audioPlayer.clip != null)
            {
                PauseAudio();
                IsPlayingAudio = false;
                ControlAudio(IsPlayingAudio);
                panelAudio.SetActive(false);
                btnAudio.SetActive(true);
            }
        }

    }

    public void ControlAudio(bool _IsPlayingAudio)
    {
        IsPlayingAudio = _IsPlayingAudio;
        if (audioPlayer.clip != null)
        {
            if (IsPlayingAudio)
            {
                PlayAudio();
                btnControlAudio.GetComponent<Image>().sprite = Resources.Load<Sprite>(PathConfig.AUDIO_PAUSE_IMAGE);
            }
            else
            {
                PauseAudio();
                btnControlAudio.GetComponent<Image>().sprite = Resources.Load<Sprite>(PathConfig.AUDIO_PLAY_IMAGE);
            }
        }
    }

    public void PauseAudio()
    {
        timeCurrentPlay = audioPlayer.time;
        audioPlayer.Pause();
    }

    public void PlayAudio()
    {
        audioPlayer.Play();
    }

    public void ResetAudio()
    {
        audioPlayer.Stop();
        btnControlAudio.GetComponent<Image>().sprite = Resources.Load<Sprite>(PathConfig.AUDIO_PLAY_IMAGE);
        MediaManager.Instance.DeleteAllIconTickOnItemMedia();
    }
}
