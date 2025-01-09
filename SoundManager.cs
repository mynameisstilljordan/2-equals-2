using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static AudioClip _win, _fail, _click;
    static AudioSource audioSrc;

    // Start is called before the first frame update
    void Start() {
        _click = Resources.Load<AudioClip>("click");
        _win = Resources.Load<AudioClip>("win");
        _fail = Resources.Load<AudioClip>("fail");
        audioSrc = GetComponent<AudioSource>();
    }

    //this method plays the inputted sound
    public static void PlaySound(string clip) {
        if (PlayerPrefs.GetInt("sound", 1) == 1) {
            switch (clip) {
                case "click":
                    audioSrc.PlayOneShot(_click);
                    break;
                case "win":
                    audioSrc.PlayOneShot(_win);
                    break;
                case "fail":
                    audioSrc.PlayOneShot(_fail);
                    break;
            }
        }
    }
}