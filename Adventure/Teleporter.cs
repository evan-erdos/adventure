/* Ben Scott * bescott@andrew.cmu.edu * 2014-12-01 * Teleporter */

using UnityEngine;
using System.Collections;


namespace PathwaysEngine.Adventure {


	/** `Teleporter` : **`MonoBehaviour`**
	 *
	 * Extremely simple class which will teleport anything that
	 * touches its trigger `Collider`.
	 **/
	[RequireComponent(typeof(AudioSource))]
	class Teleporter : MonoBehaviour {
	    new AudioSource audio;
	    [SerializeField] AudioClip sound;
	    [SerializeField] Transform target;

	    void Awake() { audio = GetComponent<AudioSource>(); }

	    void OnTriggerEnter(Collider other) {
	        other.transform.position = target.position;
	        audio.clip = sound;
	        audio.Play();
	    }
	}
}
