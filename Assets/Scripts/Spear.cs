using UnityEngine;
using System.Collections;

public class Spear : MonoBehaviour {

    public float gravityValue;
    public bool inGround;

    public float dieTimerMax;
    public float dieTimer;

	// Use this for initialization
	void Start ()
    {
        dieTimer = dieTimerMax;
    }
	
	// Update is called once per frame
	void Update ()
    {
        var diff = transform.position - PlanetObj.position;
        GetComponent<Rigidbody>().velocity += diff.normalized * -gravityValue;

        if (inGround)
        {
            dieTimer -= Time.deltaTime;
            if (dieTimer <= 0)
            {
                Destroy(this.gameObject);
            }
            else
            {
                var mrm = transform.FindChild("default").GetComponent<MeshRenderer>().material;
                mrm.color = new Color(mrm.color.r, mrm.color.g, mrm.color.b, Mathf.Min(dieTimer * 3 / dieTimerMax, 1));
            }
        }
        else
        {
            transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);
        }

        RaycastHit? hitInfo = PlanetObj.GetEpicenter(transform.position);
        if (hitInfo.HasValue)
        {
            var epicenter = hitInfo.Value.point;

            if(diff.magnitude < (epicenter - PlanetObj.position).magnitude)
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                inGround = true;
            }
        }
	}
}