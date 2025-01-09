using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//app updates
using Google.Play.AppUpdate;
using Google.Play.Common;

//reviews
using Google.Play.Review;

using Lofelt.NiceVibrations;

public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager Instance; //the instance var

    //app update
    private AppUpdateManager _appUpdateManager; 
    
    //app review
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;

    private GDPR _gdpr;
    
    private void Awake() {
            DontDestroyOnLoad(gameObject); //dont destroy the gameobject on load
            if (Instance == null) Instance = this; //if there isnt an existing instance of the gameobject
            else Destroy(gameObject); //destroy the gameobject
    }

    private void Start() {
        //everything here is called once at the start of the game
        
        Advertisements.Instance.Initialize(); //initialize the ads
        _gdpr = GetComponent<GDPR>();
        
        //haptics control
        if (PlayerPrefs.GetInt("vibration", 1) == 1) HapticController.hapticsEnabled = true;
        else HapticController.hapticsEnabled = false;

        //if player is at level 100+ and the player can be asked to leave a review
        if (PlayerPrefs.GetInt("level", 0) > 99 && PlayerPrefs.GetInt("canPlayerBeAskedToReview", 1) == 1) {
            PlayerPrefs.SetInt("canPlayerBeAskedToReview",0); //don't ask player for another review
            StartCoroutine(RequestReview()); //request the review
        }
    }

    public void ShowGdprForm() {
        _gdpr.ShowConsentForm();
    }
    
    #region Review
    IEnumerator RequestReview() {
        _reviewManager = new ReviewManager();

        //request a reviewinfo object
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError) {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();

        //launch the inapp review flow
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError) {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
        // The flow has finished. The API does not indicate whether the user
        // reviewed or not, or even whether the review dialog was shown. Thus, no
        // matter the result, we continue our app flow.
    }
    #endregion
}
