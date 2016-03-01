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

    public Rigidbody rb_e;

    /* Updated every frame */
    public Quaternion quatUp;
    public Quaternion bodyRotation;
    public float bodyAirRotationAmount;
    public float groundSpeed;
    public Vector3? epicenter;         //Position of the worm on the planet's surface.
    public float? dEpicenter;          //Distance from the center of the planet to the surface at the position of the worm.
    public float dCenter;              //Distance from center of the planet to the worm.
    public Vector3 forward = Vector3.forward;            //Direction the worm is moving tangentially to the planet SPHERE
    public Vector3 normal = Vector3.up;             //Direction of the surface at the epicenter;
    public Vector3 facing = Vector3.forward;             //Direction of motion;
    public Vector3 up = Vector3.up;                 //Direction from the center of the sphere up.
    public Vector3 right = Vector3.right;                 //Direction from the center of the sphere right.
    public Vector3 diff = Vector3.zero;               //Vector from the planet to the worm.
    public float gravityValue;         //Amount of gravity (> 0)
    public Rigidbody rb;               //The player's rigidbody
    public float ySpeed;                //y-speed (in local space)
    /***********************/

    public float depthTo;       //The depth that the worm will tween to when underground.
    public float depth { get { if (!dEpicenter.HasValue) return float.MinValue; return dEpicenter.Value - dCenter; } } //positive means below-ground.
    public bool underground { get { if (!dEpicenter.HasValue) return false; return dCenter <= dEpicenter.Value; } }
    private bool undergroundLast = false;

    public bool ragdoll = false;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        offsetRand = Random.value;
        up = (transform.position - PlanetObj.position).normalized;
        transform.position += up * offsetRand * 15;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        OrientSelf();

        UpdateSpeed();

        RotateBody();

        //Spawn spear at random
        if(Mathf.Floor(Random.value * 100f) == 0)
        {
            var o = Instantiate<GameObject>(spearPrefab);
            o.transform.position = transform.position;
            o.GetComponent<Rigidbody>().velocity = forward * forwardThrowSpeed + up * upwardThrowSpeed;
        }

        /*Debug.DrawRay(epicenter.Value, normal * 1000);
        Debug.DrawRay(epicenter.Value, up * 1100, Color.blue);
        Debug.DrawRay(epicenter.Value, right * 1000, Color.red);
        Debug.DrawRay(transform.position, facing * 1000, Color.green);
        Debug.DrawRay(transform.position, quatUp * Vector3.forward * 1000, Color.yellow);
        Debug.DrawRay(transform.position, quatUp * Vector3.up * 1000, Color.cyan);// 1000 * vel.normalized, Color.cyan);
        Debug.DrawRay(transform.position, quatUp * Vector3.right * 1000, Color.magenta);*/
        
        undergroundLast = underground;
    }

    void OrientSelf()
    {
        diff = transform.position - PlanetObj.position;
        up = diff.normalized;
        dCenter = diff.magnitude;
        facing = rb.velocity.sqrMagnitude == 0 ? Vector3.forward : rb.velocity.normalized;
        forward = (facing - Vector3.Project(facing, up)).normalized;
        right = Vector3.Cross(up, facing);
        quatUp = Quaternion.LookRotation(forward, normal);

        RaycastHit? hitInfo = PlanetObj.GetEpicenter(transform.position);
        if (hitInfo != null)
        {
            epicenter = hitInfo.Value.point;
            normal = hitInfo.Value.normal;
            dEpicenter = (epicenter.Value - PlanetObj.position).magnitude;
        }
    }

    void UpdateSpeed()
    {
        if (ragdoll)
        {

            ySpeed -= gravityValue;
            if (underground)
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
                var proj = Vector3.Project(rb_e.velocity, up);
                var dot = Vector3.Dot(proj, up); //This will be positive if the projection is in the same general direction as the up vector
                var ySpeed_e = proj.magnitude;
                if(dot > 0)
                {

                }
                else
                {
                    rb_e.velocity -= 1.25f * ySpeed_e * up; //Invert ySpeed of e 
                    if (ySpeed_e < 1)
                    {
                        rb_e.transform.SetParent(transform);
                        Destroy(this.gameObject);
                    }
                }        
            }
            rb.velocity += ySpeed * up;
            rb_e.velocity += -gravityValue * up;
        }
        else
        {
            groundSpeed = groundSpeedNormal;
            var t = transform.position;
            var s = target.transform.position;
            var d = (s - t - Vector3.Project(s - t, t - PlanetObj.position)).normalized;

            
            if (depth > -0.3f * transform.lossyScale.y)
                ySpeed = hopSpeedY;
            else
                ySpeed -= gravityValue;
            rb.velocity = ySpeed * up + new Vector3(groundSpeed * d.x, groundSpeed * d.y, groundSpeed * d.z);
        }

        //Debug.DrawRay(epicenter.Value, rb.velocity, Color.black);
    }

    void RotateBody()
    {
        bodyRotation = Quaternion.Slerp(bodyRotation, Quaternion.LookRotation(-forward, normal), 0.8f);
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
        rb.velocity = (3 * w.rb.velocity.magnitude) * Vector3.Lerp((transform.position - wp).normalized, normal, 0.5f);
        ySpeed = 0;
        ragdoll = true;

        rb_e = transform.FindChild("EnemyHead").GetComponent<Rigidbody>();
        transform.FindChild("EnemyHead").GetComponent<SphereCollider>().enabled = true;
        transform.FindChild("EnemyHead").SetParent(null);
        rb_e.isKinematic = false;
        rb_e.velocity = (3 * w.rb.velocity.magnitude) * Vector3.Lerp((rb_e.transform.position - wp).normalized, normal, 0.5f);
    }
}
