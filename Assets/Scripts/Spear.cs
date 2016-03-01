using UnityEngine;
using System.Collections;

public class Spear : MonoBehaviour {

    public float gravityValue;
    public bool inGround;
    public bool inWorm;

    public Quaternion localRotationOnWormHit;
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
        RaycastHit? hitInfo;
        if (inWorm)
        {
            transform.localRotation = localRotationOnWormHit;
            hitInfo = PlanetObj.GetEpicenter(transform.position);
            if (hitInfo.HasValue)
            {
                if ((transform.position - PlanetObj.position).magnitude <
                   (hitInfo.Value.point - PlanetObj.position).magnitude - transform.lossyScale.x * 20)
                {
                    var v = -Time.deltaTime / 100f;
                    if(transform.localScale.z + v <= 0.01f)                
                        Destroy(this.gameObject);
                    else
                        transform.localScale += new Vector3(0, 0, v);
                }
            }
        }
        else
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

            hitInfo = PlanetObj.GetEpicenter(transform.position);
            if (hitInfo.HasValue)
            {
                var epicenter = hitInfo.Value.point;

                if (diff.magnitude < (epicenter - PlanetObj.position).magnitude)
                {
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                    inGround = true;
                }
            }
        }
	}

    public void Hit(Transform o, bool isWorm)
    {
        if (inWorm)
            return;

        inWorm = true;
        localRotationOnWormHit = transform.localRotation;
        GetComponent<Rigidbody>().isKinematic = true;
        //var radius = o.GetComponent<SphereCollider>().radius;

        if (isWorm)
        {
            o = o.FindChild("Head");
        }
        /*var d = (transform.position - o.position).magnitude;
        var dM = radius * 0.95f;
        if (d > dM)
            transform.position = (transform.position - o.position).normalized * dM + o.position;*/
        transform.position -= (transform.position - o.position) * 0.25f;
        transform.SetParent(o, true);
    }
}
