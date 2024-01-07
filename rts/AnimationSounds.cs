using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSounds : MonoBehaviour {

    public enum AnimationSound
    {
        FromAudioSource = 0,

    }

    AudioSource _as;

    void Start()
    {
        _as = GetComponent<AudioSource>();
        if (_as == null)
            _as = gameObject.AddComponent<AudioSource>();
    }

	public void PlaySound(int sound)
    {
        AnimationSound ans = (AnimationSound)sound;
        switch (ans)
        {
            case AnimationSound.FromAudioSource:
                {
                    _as.PlayOneShot(_as.clip);
                } break;
            default:
                Debug.LogError("Unknown sound id: " + sound);
                break;
        }
    }
}
