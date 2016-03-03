using UnityEngine;
using System.Collections;

public class Orientation
{
    public Quaternion quatUp;
    public Vector3? epicenter;
    public Vector3? normal = Vector3.up;
    public Vector3 up = Vector3.up;
    public Vector3 forward = Vector3.forward;
    public Vector3 facing = Vector3.forward;
    public Vector3 right = Vector3.right;
    
    public float? dEpicenter;
    public float dCenter;

    public float depth { get { if (!dEpicenter.HasValue) return float.MinValue; return dEpicenter.Value - dCenter; } } //positive means below-ground.
    public bool underground { get { if (!dEpicenter.HasValue) return false; return dCenter <= dEpicenter.Value; } }

    private GameObject o;

    //If true, then quatUp will be oriented such that the normal is upwards.
    //Otherwise, quatUp will be oriented such that up is the vector from the planet to this object.
    private bool normalIsUp;

    public Orientation(GameObject _o, bool _normalIsUp=false)
    {
        o = _o;
        normalIsUp = _normalIsUp;
    }
    
    public void Update()
    {
        var rb = o.GetComponent<Rigidbody>();
        var diff = o.transform.position - PlanetObj.position;
        dCenter = diff.magnitude;
        up = diff.normalized;
        facing = rb.velocity.sqrMagnitude == 0 ? Vector3.forward : rb.velocity.normalized;
        forward = (facing - Vector3.Project(facing, up)).normalized;
        right = Vector3.Cross(up, facing);
        quatUp = Quaternion.LookRotation(forward, normalIsUp ? normal.Value : up);

        RaycastHit? hitInfo = PlanetObj.GetEpicenter(o.transform.position);
        if (hitInfo != null)
        {
            epicenter = hitInfo.Value.point;
            normal = hitInfo.Value.normal;
            dEpicenter = (epicenter.Value - PlanetObj.position).magnitude;
        }
    }
}
