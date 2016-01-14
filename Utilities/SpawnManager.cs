/* Ben Scott * bescott@andrew.cmu.edu * 2014-08-09 * Spawn Manager */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rand=System.Random;


namespace PathwaysEngine.Utilities {


    /** `SpawnManager` : **`MonoBehaviour`**
     *
     * This class deals with where to put new user-entities in
     * the game, and does so in `Start()`. It requires an array
     * (or just one?) of children with the tag `SpawnPoint`.
     * Then, from any of those, it will instantiate the `src`
     * object at the position & rotation of a random child.
     **/
    class SpawnManager : MonoBehaviour {
        public bool isPlayer = true;
        public Transform source;
        Transform[] targets;


        /** `Spawn()` : **`function`**
         *
         * Puts the `source` at a randomly-chosen `SpawnPoint`.
         **/
        void Spawn() {
            var i = new Rand().Next(0,targets.Length);
            source.position = targets[i].position;
            source.rotation = targets[i].rotation;
        }

        void Awake() {
            targets = GetComponentsInChildren<Transform>();
            var temp = new List<Transform>();
            foreach (var elem in targets)
                if (elem.CompareTag("SpawnPoint"))
                    temp.Add(elem);
            targets = temp.ToArray();
        }

        void Start() {
            if (isPlayer && Player.Current)
                source = Player.Current.transform;
            Spawn();
        }
    }
}
