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

    // Use this for initialization
    void Start()
    {
        bp = Instantiate<GameObject>(breakablePrefab);
        bp.SetActive(false);
        if (maximalElement(transform.lossyScale) <= 100f)
        {
            foreach (Transform t in bp.transform)
            {
                t.localScale = transform.lossyScale;
                t.localPosition = new Vector3(t.localScale.x * t.localPosition.x,
                                              t.localScale.y * t.localPosition.y,
                                              t.localScale.z * t.localPosition.z);
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
        if (!epi.HasValue || diff.magnitude <= (epi.Value.point - PlanetObj.position).magnitude - maximalElement(transform.lossyScale))
            Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        //Physics.IgnoreCollision(collisionInfo.collider, this.GetComponent<Collider>());
    }

    public void BreakUp(GameObject breaker=null)
    {
        if (maximalElement(transform.lossyScale) >= 50f)
        {
            bp.SetActive(true);
            bp.transform.position = transform.position;
            bp.transform.rotation = transform.rotation;
            if (maximalElement(transform.lossyScale) <= 100f)
            {
                foreach (Transform t in bp.transform)
                {
                    t.GetComponent<Rigidbody>().isKinematic = false;
                    if (breaker && breaker.GetComponent<Rigidbody>())
                    {
                        t.GetComponent<Rigidbody>().velocity -= (transform.position - breaker.transform.position).normalized * breaker.GetComponent<Rigidbody>().velocity.magnitude * velocityMultiplier;
                        t.GetComponent<Rigidbody>().velocity = Mathf.Min(t.GetComponent<Rigidbody>().velocity.magnitude, velocityMax) * t.GetComponent<Rigidbody>().velocity.normalized;
                    }
                }
            }
            Destroy(this.gameObject);
        }
        else
        {
            /*if (Random.value < 0.25f)
                Destroy(this.gameObject);
            else */
            if (breaker && breaker.GetComponent<Rigidbody>())
            {
                //GetComponent<Rigidbody>().velocity += (transform.position - breaker.transform.position).normalized * Mathf.Min(breaker.GetComponent<Rigidbody>().velocity.magnitude * velocityMultiplier, velocityMax);
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
