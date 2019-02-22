using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundManager : MonoBehaviour {

    public AudioClip[] footstepSounds;

    private AudioSource source;

	void Awake () {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
	}
	
	void Update () {
		
	}

    public void SoundFootstep() {
        source.clip = footstepSounds[Random.Range(0, footstepSounds.Length - 1)];
        source.Play();
    }
}
