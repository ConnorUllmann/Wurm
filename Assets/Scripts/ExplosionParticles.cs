using UnityEngine;
using System.Collections;

public class ExplosionParticles : MonoBehaviour {

    public float timer;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                GetComponent<ParticleSystem>().enableEmission = false;
            }
        }

        if (GetComponent<ParticleSystem>().particleCount <= 0)
            Destroy(this.gameObject);
	}
}
