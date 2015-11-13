/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Camera Fade */

using UnityEngine;
using System.Collections;
using Action = System.Action;

namespace PathwaysEngine.Utilities {
	public class CameraFade : MonoBehaviour {
		private static CameraFade mInstance = null;

		private static CameraFade instance {
			get {
				if (mInstance==null) {
					mInstance = GameObject.FindObjectOfType(typeof (CameraFade)) as CameraFade;
					if (mInstance==null)
						mInstance = new GameObject("CameraFade").AddComponent<CameraFade>();
				} return mInstance;
			}
		}

		void Awake() {
			if (mInstance == null) {
				mInstance = this as CameraFade;
				instance.init();
			}
		}

		public GUIStyle m_BackgroundStyle = new GUIStyle();			// Style for background tiling
		public Texture2D m_FadeTexture;								// 1x1 pixel texture used for fading
		public Color m_colorCurrent = new Color(0,0,0,0);			// default starting color: black and fully transparrent
		public Color m_colorTarget = new Color(0,0,0,0);			// default target color: black and fully transparrent
		public Color m_DeltaColor = new Color(0,0,0,0);				// the delta-color is basically the "speed / second"
		public int m_FadeGUIDepth = -1000;							// make sure this texture is drawn on top of everything

		public float m_FadeDelay = 0;
		public Action m_OnFadeFinish = null;

		public void init() {
			instance.m_FadeTexture = new Texture2D(1,1);
	        instance.m_BackgroundStyle.normal.background = instance.m_FadeTexture;
		}

		void OnGUI() {
			if (Time.time > instance.m_FadeDelay) {
				if (instance.m_colorCurrent!=instance.m_colorTarget) {
					if (Mathf.Abs(instance.m_colorCurrent.a-instance.m_colorTarget.a)<Mathf.Abs(instance.m_DeltaColor.a)*Time.deltaTime) {
						instance.m_colorCurrent = instance.m_colorTarget;
						SetScreenOverlayColor(instance.m_colorCurrent);
						instance.m_DeltaColor = new Color(0,0,0,0);
						if (instance.m_OnFadeFinish != null) instance.m_OnFadeFinish();
						Die();
					} else SetScreenOverlayColor(instance.m_colorCurrent+instance.m_DeltaColor * Time.deltaTime);
				}
			} if (m_colorCurrent.a > 0) {
	    		GUI.depth = instance.m_FadeGUIDepth;
	    		GUI.Label(new Rect(-10,-10,Screen.width+10,Screen.height+10),
	    			instance.m_FadeTexture,instance.m_BackgroundStyle);
			}
	    }

		public static void SetScreenOverlayColor(Color colorNew) {
			if (instance.m_FadeTexture) {
				instance.m_colorCurrent = colorNew;
				instance.m_FadeTexture.SetPixel(0, 0, instance.m_colorCurrent);
				instance.m_FadeTexture.Apply();
			}
		}

		public static void StartAlphaFade(Color colorNew, bool isFade, float timeFade) {
			if (timeFade <= 0.0f)
				SetScreenOverlayColor(colorNew);
			else {
				if (isFade) {
					instance.m_colorTarget = new Color(colorNew.r,colorNew.g,colorNew.b,0);
					SetScreenOverlayColor(colorNew);
				} else {
					instance.m_colorTarget = colorNew;
					SetScreenOverlayColor(new Color(colorNew.r,colorNew.g,colorNew.b,0));
				} instance.m_DeltaColor = (instance.m_colorTarget - instance.m_colorCurrent) / timeFade;
			}
		}

		public static void StartAlphaFade(Color colorNew, bool isFade, float timeFade, float fadeDelay) {
			if (timeFade<=0.0f) {
				SetScreenOverlayColor(colorNew);
			} else {
				instance.m_FadeDelay = Time.time + fadeDelay;
				if (isFade) {
					instance.m_colorTarget = new Color(colorNew.r,colorNew.g,colorNew.b,0);
					SetScreenOverlayColor(colorNew);
				} else {
					instance.m_colorTarget = colorNew;
					SetScreenOverlayColor(new Color(colorNew.r,colorNew.g,colorNew.b,0));
				} instance.m_DeltaColor = (instance.m_colorTarget - instance.m_colorCurrent) / timeFade;
			}
		}

		public static void StartAlphaFade(
										Color colorNew,
										bool isFade,
										float timeFade,
										float fadeDelay,
										Action OnFadeFinish) {
			if (timeFade <= 0.0f)
				SetScreenOverlayColor(colorNew);
			else {
				instance.m_OnFadeFinish = OnFadeFinish;
				instance.m_FadeDelay = Time.time + fadeDelay;
				if (isFade) {
					instance.m_colorTarget = new Color( colorNew.r, colorNew.g, colorNew.b, 0 );
					SetScreenOverlayColor( colorNew );
				} else {
					instance.m_colorTarget = colorNew;
					SetScreenOverlayColor( new Color( colorNew.r, colorNew.g, colorNew.b, 0 ) );
				} instance.m_DeltaColor = (instance.m_colorTarget - instance.m_colorCurrent) / timeFade;
			}
		}

		void Die() { mInstance = null; Destroy(gameObject); }

		void OnApplicationQuit() { mInstance = null; }
	}
}

