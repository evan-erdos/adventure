/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-01 * Static Restore */

using UnityEngine;
using System.Collections;
using util = PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
    public class StaticControlRestore : MonoBehaviour {
        public void Start () {
            Camera newCamera = gameObject.GetComponentInChildren<Camera>();
            if (newCamera) newCamera.backgroundColor = RenderSettings.fogColor;
            util::CameraFade.StartAlphaFade(RenderSettings.fogColor,false,2f,2f, ()=>{Application.LoadLevel(1);});
        }
    }
}