using UnityEngine;
using System.Collections;

public class Breakable : MonoBehaviour
{
    public float gravityValue;
    public Vector3 diff;
    public float velocityMultiplier = 0.5f;
    public float velocityMax = 200;

    public GameObject breakablePrefab;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        diff = transform.position - PlanetObj.S.transform.position;
        var rb = GetComponent<Rigidbody>();
        if (!rb.isKinematic)
        {
            rb.velocity += -gravityValue * diff.normalized;
        }
        var epi = PlanetObj.GetEpicenter(transform.position);
        if (!epi.HasValue || diff.magnitude <= (epi.Value.point - PlanetObj.S.transform.position).magnitude - maximalElement(transform.lossyScale))
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
            GameObject bp = Instantiate<GameObject>(breakablePrefab);
            bp.transform.position = transform.position;
            bp.transform.rotation = transform.rotation;
            if (maximalElement(transform.lossyScale) <= 100f)
            {
                foreach (Transform t in bp.transform)
                {
                    t.localScale = transform.lossyScale;
                    t.localPosition = new Vector3(t.localScale.x * t.localPosition.x,
                                                  t.localScale.y * t.localPosition.y,
                                                  t.localScale.z * t.localPosition.z);
                    t.GetComponent<Rigidbody>().isKinematic = false;
                    if (breaker && breaker.GetComponent<Rigidbody>())
                        t.GetComponent<Rigidbody>().velocity += (transform.position - breaker.transform.position).normalized * Mathf.Min(breaker.GetComponent<Rigidbody>().velocity.magnitude * velocityMultiplier, velocityMax);

                    t.GetComponent<Breakable>().enabled = true;
                }
            }
            Destroy(this.gameObject);
        }
        else
        {
            if (Random.value < 0.25f)
                Destroy(this.gameObject);
            else if(breaker && breaker.GetComponent<Rigidbody>())
                GetComponent<Rigidbody>().velocity += (transform.position - breaker.transform.position).normalized * breaker.GetComponent<Rigidbody>().velocity.magnitude;

        }
    }

    float maximalElement(Vector3 p)
    {
        return Mathf.Max(p.x, p.y, p.z);
    }
}
