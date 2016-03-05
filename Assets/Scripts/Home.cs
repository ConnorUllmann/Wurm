using UnityEngine;
using System.Collections;

public class Home : MonoBehaviour {

    public GameObject homeBreakable;
    private GameObject breaker;

    public Material glowingWhite;

    public float scale = 1;
    public float scaleMax = 1.25f;
    public Vector3 scaleStart;

    private float dieTimer = 0.25f;


	// Use this for initialization
	void Start () {
        scaleStart = transform.localScale;
        homeBreakable.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        transform.localScale = scaleStart * scale;

        if(scale == scaleMax)
        {
            homeBreakable.SetActive(true);
            foreach(Transform t in homeBreakable.transform)
            {
                //t.GetComponent<Breakable>().BreakUp(breaker);
                t.GetComponent<Rigidbody>().velocity = (t.position - homeBreakable.transform.position).normalized * (Random.value * 200 + 200);
            }
            dieTimer -= Time.deltaTime;
            if(dieTimer <= 0)
            {
                transform.FindChild("HomeBreakable").SetParent(null);
                Destroy(this.gameObject);
            }
        }
    }

    public void Hit(GameObject _breaker=null)
    {
        if (scale == scaleMax)
            return;
        breaker = _breaker;
        scale = scaleMax;
        transform.FindChild("default").GetComponent<MeshRenderer>().material = glowingWhite;
    }
}
