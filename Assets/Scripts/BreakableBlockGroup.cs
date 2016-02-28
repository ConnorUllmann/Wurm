using UnityEngine;
using System.Collections;

public class BreakableBlockGroup : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        bool anyExist = false;
	    foreach(Transform t in transform)
        {
            if(t != null)
            {
                anyExist = true;
                break;
            }
        }
        if (!anyExist)
            Destroy(this.gameObject);
	}
}
