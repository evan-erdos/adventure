/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-17 * Death Area */

using UnityEngine;
using System.Collections;
using util=PathwaysEngine.Utilities;


namespace PathwaysEngine.Adventure {


    /** `DeathArea` : **`class`**
     *
     * Anything which derives from `ILiving` will be instantly
     * killed when entering any trigger `Collider` attached to
     * this `GameObject` or any child if there is a `Rigidbody`
     * attached to this `GameObject`. Also optionally displays
     * a cute death message in the `Terminal`.
     **/
    public class DeathArea : MonoBehaviour {
        bool wait = false;
        public string message = "Yikes.";

        public void OnTriggerEnter(Collider o) {
            if (Player.IsCollider(o)) StartCoroutine(Kill()); }


        /** `Kill` : **`coroutine`**
         *
         * Kills the `Player` that entered the `Collider`.
        **/
        public IEnumerator Kill() {
            if (!wait) {
                wait = true;
                yield return new WaitForSeconds(2f);
                util::CameraFade.StartAlphaFade(
                    new Color(255,255,255),false,8f,2f,
                    ()=> {
                        if (!Pathways.mainCamera) return;
                        Pathways.mainCamera.cullingMask = 0;
                        Pathways.mainCamera.clearFlags =
                            CameraClearFlags.SolidColor;
                        Pathways.mainCamera.backgroundColor =
                            new Color(255,255,255); });
                Player.Kill(message);
                yield return new WaitForSeconds(12f);
                wait = false;
            }
        }
    }
}
