﻿/* Ben Scott * bescott@andrew.cmu.edu * 2014-12-01 * Teleporter */

using UnityEngine;
using System.Collections;


namespace PathwaysEngine.Adventure {


	/** `Teleporter` : **`class`**
	 *
	 * Extremely simple class which will teleport anything that
	 * touches its trigger `Collider`.
	 **/
	[RequireComponent(typeof(AudioSource))]
	class Teleporter : MonoBehaviour {
	    AudioSource _audio;
	    [SerializeField] AudioClip sound;
	    [SerializeField] Transform tgt;

	    void Awake() { _audio = GetComponent<AudioSource>(); }

	    void OnTriggerEnter(Collider other) {
	        other.transform.position = tgt.position;
	        _audio.clip = sound;
	        _audio.Play();
	    }
	}
}
