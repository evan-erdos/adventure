//#if TEMP
using System;
using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Utilities {
    public class ProtectCameraFromWallClip : MonoBehaviour {
        public float clipMoveTime = 0.05f;                  // time taken to move when avoiding cliping (low value = fast, which it should be)
        public float returnTime = 0.4f;                     // time taken to move back towards desired position, when not clipping (typically should be a higher value than clipMoveTime)
        public float sphereCastRadius = 0.1f;               // the radius of the sphere used to test for object between camera and target
        public bool visualiseInEditor;                      // toggle for visualising the algorithm through lines for the raycast in the editor
        public float closestDistance = 0.5f;                // the closest distance the camera can be from the target
        public bool protecting { get; private set; }        // used for determining if there is an object between the target and the camera
        string[] dontClipTags = {"Player", "Inventory"};
        // don't clip against objects with this tag (useful for not clipping against the targeted object)

        private Transform cam;                              // the transform of the camera
        private Transform pivot;                            // the point at which the camera pivots around
        private float originalDist;                         // the original distance to the camera before any modification are made
        private float moveVelocity;                         // the velocity at which the camera moved
        private float currentDist;                          // the current distance from the camera to the target
        private Ray ray;                                    // the ray used in the lateupdate for casting between the camera and the target
        private RaycastHit[] hits;                          // the hits between the camera and the target
        private RayHitComparer rayHitComparer;              // variable to compare raycast hit distances

        public LayerMask layerMask;


        void Start() {
            layerMask = ~(LayerMask.NameToLayer("Player"));
            // find the camera in the object hierarchy
            cam = GetComponentInChildren<Camera>().transform;
            pivot = cam.parent;
            originalDist = cam.localPosition.magnitude;
            currentDist = originalDist;

            // create a new RayHitComparer
            rayHitComparer = new RayHitComparer();
        }


        void LateUpdate() {

            // initially set the target distance
            float targetDist = originalDist;

            ray.origin = pivot.position + pivot.forward * sphereCastRadius;
            ray.direction = -pivot.forward;

            // initial check to see if start of spherecast intersects anything
            Collider[] cols = Physics.OverlapSphere(ray.origin, sphereCastRadius,layerMask);

            bool initialIntersect = false;
            bool hitSomething = false;

            // loop through all the collisions to check if something we care about
            for (int i = 0; i < cols.Length; i++) {
                var c = cols[i];
                if (!c.isTrigger
                && !c.gameObject.CompareTag(dontClipTags[0])
                && !c.gameObject.CompareTag(dontClipTags[1])) {
                    initialIntersect = true;
                    break;
                }
            }

            // if there is a collision
            if (initialIntersect) {
                ray.origin += pivot.forward * sphereCastRadius;

                // do a raycast and gather all the intersections
                hits = Physics.RaycastAll(ray,originalDist-sphereCastRadius,layerMask);
            } else {

                // if there was no collision do a sphere cast to see if there were any other collisions
                hits = Physics.SphereCastAll(ray, sphereCastRadius, originalDist + sphereCastRadius,layerMask);
            }

            // sort the collisions by distance
            Array.Sort(hits, rayHitComparer);

            // set the variable used for storing the closest to be as far as possible
            float nearest = Mathf.Infinity;

            // loop through all the collisions
            for (int i = 0; i < hits.Length; i++) {

                // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
                var h = hits[i];
                if (h.distance<nearest && (!h.collider.isTrigger)
                && !h.collider.gameObject.CompareTag(dontClipTags[0])
                && !h.collider.gameObject.CompareTag(dontClipTags[1])) {
                    // change the nearest collision to latest
                    nearest = hits[i].distance;
                    targetDist = -pivot.InverseTransformPoint(hits[i].point).z;
                    hitSomething = true;
                }
            }

            // visualise the cam clip effect in the editor
            Debug.DrawRay(ray.origin, -pivot.forward * (targetDist + sphereCastRadius), hitSomething ? Color.red : Color.green);

            // hit something so move the camera to a better position
            protecting = hitSomething;
            currentDist = Mathf.SmoothDamp(currentDist, targetDist, ref moveVelocity, currentDist > targetDist ? clipMoveTime : returnTime );
            currentDist = Mathf.Clamp(currentDist,closestDistance,originalDist);
            cam.localPosition = -Vector3.forward * currentDist;

        }


        // comparer for check distances in ray cast hits
        public class RayHitComparer: IComparer
        {
            public int Compare(object x, object y) {
                return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
            }
        }
    }
}
//#endif

/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Camera Collider * /

//using System;
using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Utilities {
    public class ProtectCameraFromWallClip : MonoBehaviour {
        public bool visualiseInEditor;
        float originalDist, moveVelocity, currentDist;
        public float clipMoveTime = 0.05f, returnTime = 0.4f;
        public float sphereCastRadius = 0.1f, closestDistance = 0.5f;
        public string dontClipTag = "Player";

        public bool protecting {get; private set;} // object blocking

        Transform cam, pivot;
        Ray ray; // casting between the camera and the target
        RaycastHit[] hits;
        RayHitComparer rayHitComparer;
        public LayerMask layerMask;

        void Start() {
#if OLD
            layerMask = ~(LayerMask.NameToLayer("Player")
                |LayerMask.NameToLayer("EquippedItems")
                |LayerMask.NameToLayer("Items"));
            layerMask = (LayerMask.NameToLayer("Default"));
#endif
            cam = GetComponentInChildren<Camera>().transform;
            pivot = cam.parent;
            originalDist = cam.localPosition.magnitude;
            currentDist = originalDist;
            rayHitComparer = new RayHitComparer();
        }

        void LateUpdate() {
            float targetDist = originalDist;
            ray.origin = pivot.position+pivot.forward*sphereCastRadius;
            ray.direction = -pivot.forward;
            Collider[] cols = Physics.OverlapSphere(ray.origin,sphereCastRadius,layerMask);
            bool initialIntersect = false;
            bool hitSomething = false;
#if OLD
            for (int i=0;i<cols.Length;i++) { // loop collisions
                if ((!cols[i].isTrigger) && !(cols[i].attachedRigidbody!=null
                && cols[i].attachedRigidbody.CompareTag(dontClipTag))) {
                    initialIntersect = true;
                    break;
                }
#endif
            for (int i=0;i<cols.Length;i++) { // loop collisions
                if ((!cols[i].isTrigger) && cols[i].gameObject.CompareTag(dontClipTag)) {
                    initialIntersect = true;
                    break;
                }
            } if (initialIntersect) {
                ray.origin += pivot.forward * sphereCastRadius;
                // do a raycast and gather all the intersections
                hits = Physics.RaycastAll(ray, originalDist - sphereCastRadius, layerMask);
            } else hits = Physics.SphereCastAll(ray, sphereCastRadius, originalDist + sphereCastRadius, layerMask);
            System.Array.Sort(hits, rayHitComparer); // sort by distance
            float nearest = Mathf.Infinity; // set storing the closest far as possible
            for (int i=0;i<hits.Length;i++) {
                // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
#if OLD
                if (hits[i].distance<nearest && (!hits[i].collider.isTrigger)
                && !(hits[i].collider.attachedRigidbody!=null
                && hits[i].collider.attachedRigidbody.CompareTag(dontClipTag))) {
                    nearest = hits[i].distance;
                    targetDist = -pivot.InverseTransformPoint(hits[i].point).z;
                    hitSomething = true;
                }
#endif
                if (hits[i].distance<nearest && (!hits[i].collider.isTrigger)
                && hits[i].collider.gameObject.CompareTag(dontClipTag)) {
                    nearest = hits[i].distance;
                    targetDist = -pivot.InverseTransformPoint(hits[i].point).z;
                    hitSomething = true;
                }
            }

            if (hits!=null && hits.Length>0) print(hits[0]);
            if (hitSomething) Debug.DrawRay(
                ray.origin,-pivot.forward*(targetDist+sphereCastRadius),Color.red);
            protecting = hitSomething; // hit something so move the camera to a better position
            currentDist = Mathf.SmoothDamp(currentDist, targetDist, ref moveVelocity, currentDist > targetDist ? clipMoveTime : returnTime );
            currentDist = Mathf.Clamp(currentDist,closestDistance,originalDist);
            cam.localPosition = -Vector3.forward * currentDist;

        }

        public class RayHitComparer: IComparer { // cmp for distances in raycast
            public int Compare(object x, object y) {
                return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
            }
        }
    }
} //*/

