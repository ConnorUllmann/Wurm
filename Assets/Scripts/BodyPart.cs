using UnityEngine;
using System.Collections;

public class BodyPart : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider c)
    {
        Debug.Log(c.gameObject.name + " hit! " + c.gameObject.tag);
        if (c.gameObject.tag == "Enemy")
        {
            c.gameObject.GetComponent<Enemy>().Hit(worm);
        }
        else if (c.gameObject.tag == "Wall")
        {
            c.GetComponent<Breakable>().BreakUp(gameObject);
        }
        else if(c.gameObject.tag == "Spear")
        {
            c.GetComponent<Spear>().Hit(gameObject);
        }
    }

    Worm worm { get { return transform.parent.GetComponent<Worm>(); } }
}
