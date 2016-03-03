using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    private static float forwardThrowSpeed = 300;
    private static float upwardThrowSpeed = 800;

    public float minGroundSpeed;
    public float groundSpeedNormal;
    public float hopSpeedY;

    private float offsetRand; //A random value 0..1 for this instance.

    public GameObject target;
    public GameObject spearPrefab;

    public Orientation or;
    public Rigidbody rb;
    public Rigidbody rb_e;

    /* Updated every frame */
    public Quaternion bodyRotation;
    public float bodyAirRotationAmount;
    public float groundSpeed;
    public float gravityValue;         //Amount of gravity (> 0)
    public float ySpeed;                //y-speed (in local space)
    /***********************/

    public bool ragdoll = false;

    // Use this for initialization
    void Start()
    {
        or = new Orientation(gameObject, true);
        rb = GetComponent<Rigidbody>();

        offsetRand = Random.value;
        transform.position += (transform.position - PlanetObj.position).normalized * offsetRand * 15;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        or.Update();

        UpdateSpeed();

        RotateBody();

        //Spawn spear at random
        if(Mathf.Floor(Random.value * 400f) == 0)
        {
            var o = Instantiate<GameObject>(spearPrefab);
            o.transform.position = transform.position;
            o.GetComponent<Rigidbody>().velocity = or.forward * forwardThrowSpeed + or.up * upwardThrowSpeed;
        }

        /*Debug.DrawRay(epicenter.Value, normal * 1000);
        Debug.DrawRay(epicenter.Value, up * 1100, Color.blue);
        Debug.DrawRay(epicenter.Value, right * 1000, Color.red);
        Debug.DrawRay(transform.position, facing * 1000, Color.green);
        Debug.DrawRay(transform.position, quatUp * Vector3.forward * 1000, Color.yellow);
        Debug.DrawRay(transform.position, quatUp * Vector3.up * 1000, Color.cyan);// 1000 * vel.normalized, Color.cyan);
        Debug.DrawRay(transform.position, quatUp * Vector3.right * 1000, Color.magenta);*/
    }

    void UpdateSpeed()
    {
        if (ragdoll)
        {
            ySpeed -= gravityValue;
            if (or.underground)
            {
                ySpeed = Mathf.Abs(ySpeed) * 0.25f;
                if (ySpeed < 1)
                {
                    rb_e.transform.SetParent(transform);
                    Destroy(this.gameObject); //Destroy self if we are underground after being hit
                }
            }
            var epicenter_e = PlanetObj.GetEpicenter(rb_e.transform.position);
            if (epicenter_e.HasValue && (rb_e.transform.position - PlanetObj.position).sqrMagnitude <= (epicenter_e.Value.point - PlanetObj.position).sqrMagnitude)
            {
                var proj = Vector3.Project(rb_e.velocity, or.up);
                var dot = Vector3.Dot(proj, or.up); //This will be positive if the projection is in the same general direction as the up vector
                var ySpeed_e = proj.magnitude;
                if(dot > 0)
                {

                }
                else
                {
                    rb_e.velocity -= 1.25f * ySpeed_e * or.up; //Invert ySpeed of e 
                    if (ySpeed_e < 1)
                    {
                        rb_e.transform.SetParent(transform);
                        Destroy(this.gameObject);
                    }
                }        
            }
            rb.velocity += ySpeed * or.up;
            rb_e.velocity += -gravityValue * or.up;
        }
        else
        {
            groundSpeed = groundSpeedNormal;
            var t = transform.position;
            var s = target.transform.position;
            var d = (s - t - Vector3.Project(s - t, t - PlanetObj.position)).normalized;

            
            if (or.depth > -0.3f * transform.lossyScale.y)
                ySpeed = hopSpeedY;
            else
                ySpeed -= gravityValue;
            rb.velocity = ySpeed * or.up + new Vector3(groundSpeed * d.x, groundSpeed * d.y, groundSpeed * d.z);
        }

        //Debug.DrawRay(epicenter.Value, rb.velocity, Color.black);
    }

    void RotateBody()
    {
        bodyRotation = Quaternion.Slerp(bodyRotation, Quaternion.LookRotation(-or.forward, or.normal.Value), 0.8f);
        bodyAirRotationAmount += (-ySpeed / 2.8f - bodyAirRotationAmount) * 0.3f;
        var _bodyAirRotation = Quaternion.Euler(new Vector3(bodyAirRotationAmount, 0, 0));
        transform.rotation = bodyRotation * _bodyAirRotation;
    }

    void OnTriggerEnter(Collider c)
    {
        //Debug.Log(c.gameObject.name + " hit!");
    }

    public void Hit(Worm w)
    {
        if (ragdoll)
            return;

        var wp = w.transform.position;
        rb.velocity = (3 * w.rb.velocity.magnitude) * Vector3.Lerp((transform.position - wp).normalized, or.normal.Value, 0.5f);
        ySpeed = 0;
        ragdoll = true;

        rb_e = transform.FindChild("EnemyHead").GetComponent<Rigidbody>();
        transform.FindChild("EnemyHead").GetComponent<SphereCollider>().enabled = true;
        transform.FindChild("EnemyHead").SetParent(null);
        rb_e.isKinematic = false;
        rb_e.velocity = (3 * w.rb.velocity.magnitude) * Vector3.Lerp((rb_e.transform.position - wp).normalized, or.normal.Value, 0.5f);
    }
}
