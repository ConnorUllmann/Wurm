using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public float minGroundSpeed;
    public float groundSpeedNormal;
    public float hopSpeedY;

    private float offsetRand; //A random value 0..1 for this instance.

    public GameObject target;
    public GameObject planet;

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
    public Vector3 vel = Vector3.zero;                //Velocity applied to the worm head.
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
        up = (transform.position - planet.transform.position).normalized;
        transform.position += up * offsetRand * 15;
    }

    // Update is called once per frame
    void Update()
    {
        OrientSelf();

        UpdateSpeed();

        RotateBody();

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
        diff = transform.position - planet.transform.position;
        up = diff.normalized;
        dCenter = diff.magnitude;
        facing = rb.velocity.normalized;
        forward = (facing - Vector3.Project(facing, up)).normalized;
        right = Vector3.Cross(up, facing);
        quatUp = Quaternion.LookRotation(forward, up);

        RaycastHit? hitInfo = GetEpicenter();
        if (hitInfo != null)
        {
            epicenter = hitInfo.Value.point;
            normal = hitInfo.Value.normal;
            dEpicenter = (epicenter.Value - planet.transform.position).magnitude;
        }
    }

    void UpdateSpeed()
    {
        if (ragdoll)
        {
            vel.y -= gravityValue;
            if (underground)
            {
                vel.y = Mathf.Abs(vel.y) * 0.9f;
                if(vel.y < 1)
                    Destroy(this.gameObject); //Destroy self if we are underground after being hit
            }
            rb.velocity += quatUp * vel;
        }
        else
        {
            groundSpeed = groundSpeedNormal;
            vel = new Vector3(1, vel.y, groundSpeed);
            if (underground)
            {
                transform.position += up * depth;
                vel.y = hopSpeedY;
            }
            else
                vel.y -= gravityValue;
            rb.velocity = quatUp * vel;
        }
    }

    void RotateBody()
    {
        bodyRotation = Quaternion.Slerp(bodyRotation, Quaternion.LookRotation(-facing, normal), 0.1f);
        bodyAirRotationAmount += (-vel.y / 2.8f - bodyAirRotationAmount) * 0.3f;
        var _bodyAirRotation = Quaternion.Euler(new Vector3(bodyAirRotationAmount, 0, 0));
        transform.rotation = bodyRotation * _bodyAirRotation;
    }

    void OnTriggerEnter(Collider c)
    {
        Debug.Log(c.gameObject.name + " hit!");
    }

    public void Hit(Worm w)
    {
        var wp = w.transform.position;
        rb.velocity = (3 * w.rb.velocity.magnitude) * Vector3.Lerp((transform.position - wp).normalized, normal, 0.5f);
        vel = Vector3.zero;
        ragdoll = true;
    }

    //Returns the position of the worm as projected onto the surface of the planet.
    RaycastHit? GetEpicenter()
    {
        var diff = transform.position - planet.transform.position;
        var direction = diff.normalized;
        var origin = direction * planet.GetComponent<PlanetObj>().safeHeight + planet.transform.position;

        RaycastHit hitInfo;
        LayerMask layerPlanet = 1 << 8;
        if (Physics.Raycast(origin, -direction, out hitInfo, planet.GetComponent<PlanetObj>().safeHeight, layerPlanet))
            return hitInfo;

        return null;
    }
}
