using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

    public GameObject explosionParticlesPrefab;

    private SphereCollider sc;
    private float radiusStart;
    private float radiusEnd;
    public float radius;

    private float flashTimer = 0.1f;
    public Vector3 scale;

	// Use this for initialization
	void Start ()
    {
        sc = GetComponent<SphereCollider>();
        radiusStart = sc.radius;
        radius = radiusEnd = radiusStart * 10f;
        scale = transform.localScale;

        transform.Rotate(new Vector3(360 * Random.value, 360 * Random.value, 360 * Random.value));
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                flashTimer = 0;
                radius = radiusStart;
            }
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.yellow, Color.red, (radius - radiusStart) / (radiusEnd - radiusStart));
            radius += radius * Time.deltaTime * 15;
            if (radius > radiusEnd)
            {
                var o = Instantiate<GameObject>(explosionParticlesPrefab);
                o.transform.position = transform.position;
                o.transform.localScale = transform.localScale * 0.1f;
                Destroy(this.gameObject);
            }
        }
        transform.localScale = radius / radiusStart * scale;
	}
}
