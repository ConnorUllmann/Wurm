using UnityEngine;
using System.Collections;

public class Breakable : MonoBehaviour
{
    public float gravityValue;
    public Vector3 diff;
    public float velocityMultiplier = 0.5f;
    public float velocityMax = 200;

    public GameObject explosionPrefab;

    public int breaks;
    public int gravityBreakLimit; //Number of breaks remaining when the breakable will activate gravity.

    public bool HasGravity { get { return breaks <= gravityBreakLimit; } }
    public void SetBreaks(int _breaks)
    {
        breaks = _breaks;
        if (HasGravity)
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    // Use this for initialization
    void Start()
    {
        SetBreaks(breaks);
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
        /*var epi = PlanetObj.GetEpicenter(transform.position);
        if (diff.magnitude <= (epi.Value.point - PlanetObj.position).magnitude)
             transform.position = epi.Value.point;*/
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        //Physics.IgnoreCollision(collisionInfo.collider, this.GetComponent<Collider>());
    }

    public void BreakUp(GameObject breaker = null)
    {
        if (breaks > 0)
        {
            for(int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    for (int k = -1; k <= 1; k += 2)
                    {
                        var b = Instantiate<GameObject>(gameObject);
                        b.transform.localScale = transform.lossyScale * 0.5f;
                        var pos = new Vector3(i, j, k) * 0.5f;
                        pos.x *= b.transform.localScale.x;
                        pos.y *= b.transform.localScale.y;
                        pos.z *= b.transform.localScale.z;
                        b.transform.position = transform.position + pos;

                        b.GetComponent<Breakable>().enabled = true;
                        b.GetComponent<Breakable>().SetBreaks(breaks - 1);
                        if (b.GetComponent<Breakable>().HasGravity)
                        {
                            if (breaker && breaker.GetComponent<Rigidbody>())
                            {
                                b.GetComponent<Rigidbody>().velocity -= (transform.position - breaker.transform.position).normalized * breaker.GetComponent<Rigidbody>().velocity.magnitude * velocityMultiplier;
                                b.GetComponent<Rigidbody>().velocity = Mathf.Min(b.GetComponent<Rigidbody>().velocity.magnitude, velocityMax) * b.GetComponent<Rigidbody>().velocity.normalized;
                            }
                        }
                    }
                }
            }

            int num = (int)(1 + (Random.value) * 3);
            for (int i = 0; i < num; i++)
            {
                var explosion = Instantiate<GameObject>(explosionPrefab);
                var pos = transform.lossyScale;
                var sphere = Random.insideUnitSphere;
                pos.x *= sphere.x;
                pos.y *= sphere.y;
                pos.z *= sphere.z;
                explosion.transform.position = transform.position + pos;
                explosion.GetComponent<Explosion>().scale = explosion.transform.localScale = maximalElement(transform.lossyScale) * Vector3.one * (Random.value + 1) / 10f;
            }
            breaks = 0;
            Destroy(gameObject);
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
