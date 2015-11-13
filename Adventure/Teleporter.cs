using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Teleporter : MonoBehaviour {
	AudioSource au;
	public AudioClip sound;
	public Transform tgt;

	void Awake() { au = GetComponent<AudioSource>(); }

	void OnTriggerEnter(Collider other) {
	    other.transform.position = tgt.position;
	    au.clip = sound;
	    au.Play();
	}
}

