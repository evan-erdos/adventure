/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-11 * Look */

using UnityEngine;
using System.Collections;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
    public class Look : MonoBehaviour {
        public bool recenter, usePrev, inControl;
        uint size, ind;
        float ratio;
        public float speed;
        public enum rotAxes : byte { MouseXY, MouseX, MouseY }
        public rotAxes rotAxis = rotAxes.MouseXY;
        Vector2 Sensitivity, cr, avg; // current rotation, avg
        Vector2[] rarr; // rotation array
        Vector3 pr; // previous rotation
        Vector4 Maxima;
        Quaternion dr, lr; // delta rotation, last rotation
        public util::axis mouseX, mouseY;

        public Look() {
            recenter    = false;    usePrev = false;
            inControl   = true;     speed   = 2.0f;
            size        = 8;        ind     = 0;

            mouseX      = new util::axis((n)=>mouseX.input=n);
            mouseY      = new util::axis((n)=>mouseY.input=n);
        }

        void Awake() {
            ratio = Screen.width/Screen.height;
            rarr = new Vector2[(int)size];
            Maxima.Set(-360f,360f,-60f,60f);
            lr = transform.localRotation;
            if (GetComponent<Rigidbody>() && !recenter)
                GetComponent<Rigidbody>().freezeRotation = true;
            Sensitivity.Set(speed*ratio, speed);
        }

        void Update() {
            if (!inControl) return;// || (am && am.isPlaying)) return;
            pr = (usePrev)?transform.localEulerAngles:Vector3.zero;
            if (recenter) cr.Set(cr.x*0.5f,cr.y*0.5f);
            avg.Set(0f,0f);
            cr.x += mouseX.input*Sensitivity.x;
            cr.y += mouseY.input*Sensitivity.y;
            cr.y = ClampAngle(cr.y, Maxima.z, Maxima.w);
            rarr[(int)ind] = cr;
            foreach (Vector2 elem in rarr) avg += elem;
            avg /= (int)size;
            avg.y = ClampAngle(avg.y, Maxima.z, Maxima.w);
            switch (rotAxis) {
                case rotAxes.MouseXY :
                    dr = Quaternion.Euler(-avg.y,avg.x,pr.z); break;
                case rotAxes.MouseX :
                    dr = Quaternion.Euler(-pr.y,avg.x,pr.z); break;
                case rotAxes.MouseY :
                    dr = Quaternion.Euler(-avg.y,pr.x,pr.z); break;
            } ind++;
            transform.localRotation = lr*dr;
            if ((int)ind >= (int)size) ind -= size;
        }

        static float ClampAngle(float delta, float maximaL, float maximaH) {
            delta %= 360f;
            if (delta<=-360f) delta += 360f;
            else if (delta>=360f) delta -= 360f;
            return Mathf.Clamp(delta, maximaL, maximaH);
        }
    }
}
