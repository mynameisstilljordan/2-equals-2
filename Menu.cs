using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Lofelt.NiceVibrations;

public class Menu : MonoBehaviour{
    [SerializeField] TMP_Text _playButtonSubtext;
    [SerializeField] private Button _playButton, _configButton, _closeButton, _soundOnButton, _soundOffButton, _vibrationOnButton, _vibrationOffButton, _privacyButton;
    [SerializeField] private Image _configOverlay;

    private Vector2[] _configOverlayPositions = new Vector2[] { new Vector2(0, -1400), Vector2.zero};
    
    // Start is called before the first frame update
    void Start() {
        _configOverlay.gameObject.SetActive(false); //disable the overlay by default
        _configOverlay.transform.localPosition = _configOverlayPositions[0]; //set original position
        
        _playButtonSubtext.text = "CURRENT LEVEL: " + PlayerPrefs.GetInt("level", 1); //the play button subtext
        
        _playButton.onClick.AddListener(OnPlayButtonPressed);
        _configButton.onClick.AddListener(OnConfigButtonPressed);
        _closeButton.onClick.AddListener(OnCloseButtonPressed);
        
        _soundOnButton.onClick.AddListener(SoundOn);
        _soundOffButton.onClick.AddListener(SoundOff);
        _vibrationOnButton.onClick.AddListener(VibrationOn);
        _vibrationOffButton.onClick.AddListener(VibrationOff);
        
        _privacyButton.onClick.AddListener(PrivacyButtonPressed);
    }

    //when the play button is pressed
    void OnPlayButtonPressed() {
        PlayButtonFeedback(); //play the button feedback
        SceneManager.LoadScene("ingame"); //load the ingame scene
    }

    //when the config button is pressed
    void OnConfigButtonPressed() {
        PlayButtonFeedback(); //play the button feedback
        _configOverlay.gameObject.SetActive(true);
        _configOverlay.transform.DOLocalMove(_configOverlayPositions[1], 0.5f)
            .SetEase(Ease.OutSine); 
    }

    //when the close button is pressed
    void OnCloseButtonPressed() {
        PlayButtonFeedback(); //play the button feedback
        _configOverlay.transform.DOLocalMove(_configOverlayPositions[0], 0.5f).OnComplete(() => {
            _configOverlay.gameObject.SetActive(false);
        });
    }

    //this method plays the button sound
    void PlayButtonFeedback() {
        SoundManager.PlaySound("click"); //play sound
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play impact
    }

    void SoundOn() {
        PlayerPrefs.SetInt("sound",1);
        PlayButtonFeedback(); //play the button feedback
    }
    
    void SoundOff() {
        PlayerPrefs.SetInt("sound",0);
        PlayButtonFeedback(); //play the button feedback
    }

    void VibrationOn() {
        PlayerPrefs.SetInt("vibration", 1);
        HapticController.hapticsEnabled = true;
        PlayButtonFeedback(); //play the button feedback
    }

    void VibrationOff() {
        PlayerPrefs.SetInt("vibration", 0);
        HapticController.hapticsEnabled = false;
        PlayButtonFeedback(); //play the button feedback
    }

    void PrivacyButtonPressed() {
        GlobalGameManager.Instance.ShowGdprForm();
        PlayButtonFeedback();
    }
}
