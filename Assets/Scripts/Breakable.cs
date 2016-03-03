using UnityEngine;
using System.Collections;

public class Breakable : MonoBehaviour
{
    public float gravityValue;
    public Vector3 diff;
    public float velocityMultiplier = 0.5f;
    public float velocityMax = 200;

    public GameObject breakablePrefab;
    public GameObject bp;

    public int breaks;
    public int gravityBreakLimit; //Number of breaks remaining when the breakable will activate gravity.

    public bool HasGravity { get { return  breaks <= gravityBreakLimit; } }
    public void SetBreaks(int _breaks)
    {
        breaks = _breaks;
        if (breaks <= gravityBreakLimit)
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    // Use this for initialization
    void Start()
    {
        SetBreaks(breaks);
        if (breaks > 0)
        {
            bp = Instantiate<GameObject>(breakablePrefab);
            bp.SetActive(false);
            foreach (Transform t in bp.transform)
            {
                var pos = t.position;
                pos.x *= transform.lossyScale.x;
                pos.y *= transform.lossyScale.y;
                pos.z *= transform.lossyScale.z;
                t.position = pos;
                t.localScale = transform.lossyScale;
                t.GetComponent<Breakable>().enabled = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        diff = transform.position - PlanetObj.position;
        var rb = GetComponent<Rigidbody>();
        if (!rb.isKinematic)
        {
            rb.velocity += -gravityValue * diff.normalized;
        }
        var epi = PlanetObj.GetEpicenter(transform.position);
        //if (!epi.HasValue || diff.magnitude <= (epi.Value.point - PlanetObj.position).magnitude - maximalElement(transform.lossyScale))
       //     Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        //Physics.IgnoreCollision(collisionInfo.collider, this.GetComponent<Collider>());
    }

    public void BreakUp(GameObject breaker=null)
    {
        if (breaks > 0)
        {
            bp.SetActive(true);
            bp.transform.position = transform.position;
            bp.transform.rotation = transform.rotation;
            foreach (Transform t in bp.transform)
            {
                t.GetComponent<Breakable>().SetBreaks(breaks - 1);
                if (t.GetComponent<Breakable>().HasGravity)
                {
                    if (breaker && breaker.GetComponent<Rigidbody>())
                    {
                        t.GetComponent<Rigidbody>().velocity -= (transform.position - breaker.transform.position).normalized * breaker.GetComponent<Rigidbody>().velocity.magnitude * velocityMultiplier;
                        t.GetComponent<Rigidbody>().velocity = Mathf.Min(t.GetComponent<Rigidbody>().velocity.magnitude, velocityMax) * t.GetComponent<Rigidbody>().velocity.normalized;
                    }
                }
            }
            breaks = 0;
            Destroy(this.gameObject);
        }
        else
        {
            if (breaker && breaker.GetComponent<Rigidbody>())
            {
                GetComponent<Rigidbody>().velocity -= (transform.position - breaker.transform.position).normalized * breaker.GetComponent<Rigidbody>().velocity.magnitude * velocityMultiplier;
                GetComponent<Rigidbody>().velocity = Mathf.Min(GetComponent<Rigidbody>().velocity.magnitude, velocityMax) * GetComponent<Rigidbody>().velocity.normalized;
            }
        }
    }

    float maximalElement(Vector3 p)
    {
        return Mathf.Max(p.x, p.y, p.z);
    }
}
