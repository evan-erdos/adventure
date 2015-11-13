/* Ben Scott * bescott@andrew.cmu.edu * 2014-08-09 * Spawn Manager */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rand=System.Random;

namespace PathwaysEngine.Utilities {
	public class SpawnManager : MonoBehaviour {
		public bool isPlayer = true;
		public Transform src;
		Transform[] tgts;
		public key invt;

		public SpawnManager() { invt = new key((n)=>invt.input=n); }

		void Awake() {
			tgts = GetComponentsInChildren<Transform>();
			var temp = new List<Transform>();
			foreach (var elem in tgts)
				if (elem.CompareTag("SpawnPoint")) temp.Add(elem);
			tgts = temp.ToArray();
		}

		void Update() {
			if (Input.GetButtonDown("Invt")) Spawn();
		}

		void Start() {
			if (isPlayer) src = Pathways.player.transform;
			Spawn();
		}

		void Spawn() {
			Pathways.player.ResetPlayerLocalPosition();
			var i = new Rand().Next(0,tgts.Length);
			src.position = tgts[i].position;
			src.rotation = tgts[i].rotation;
		}
	}
}
