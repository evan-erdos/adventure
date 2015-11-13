using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode()]
public class RailiingPlacer : MonoBehaviour {
#if DUMB
	public float radius, sector;
	public uint count;
	public Vector3 center, offset;
	public GameObject post, rail;
	public List<GameObject> posts;

	//uint Count {
	//	get { return _count; }
	//	set { _theta=0.0; _count=value; }
	//} uint _count = 4;

	float theta {
		get { if (_theta==0f)
				_theta = (360f/count);
			return _theta;
		}
	} float _theta = 0f;


	void Start() {
		if (post && rail) {
			for (int i=0;i<count;++i) {
				var s0 = new Vector3(
					Mathf.Cos((i*theta)/Mathf.Rad2Deg)*radius+transform.position.x+offset.x,transform.position.y+offset.y,
					Mathf.Sin((i*theta)/Mathf.Rad2Deg)*radius+transform.position.z+offset.x);
				var temp = Instantiate(post,s0,Quaternion.identity) as GameObject;
				temp.transform.parent = this.transform;
				temp.transform.Rotate(new Vector3(0f, theta*i,0f));
				Debug.Log(temp.transform.parent);
				Debug.Log(this.transform);
				posts.Add(temp);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		for (int i=0;i<36;++i) {
			var s0 = new Vector3(
				Mathf.Cos((i*10*radius)/Mathf.Rad2Deg)+transform.position.x+offset.x,transform.position.y+offset.y,
				Mathf.Sin((i*10*radius)/Mathf.Rad2Deg)+transform.position.z+offset.z);
			var s1 = new Vector3(
				Mathf.Cos(((i+1)*10*radius)/Mathf.Rad2Deg)+transform.position.x+offset.x,transform.position.y+offset.y,
				Mathf.Sin(((i+1)*10*radius)/Mathf.Rad2Deg)+transform.position.z+offset.z);
			dev.Draw.Line(s1,s0,Color.white);
		}
	}
#endif
}