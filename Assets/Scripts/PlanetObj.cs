using UnityEngine;
using System.Collections;

public class PlanetObj : MonoBehaviour {

    public float safeHeight;

	// Use this for initialization
	void Start ()
    {
        safeHeight = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) / 2 + 10000;
        /*var mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] v = new Vector3[mesh.vertexCount];
        bool[] b = new bool[mesh.vertexCount];
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            v[i] = Vector3.zero;
            b[i] = false;
        }
        for(int i = 0; i < mesh.vertexCount; i++)
        {
            if (b[i])
                continue;

            var val = mesh.vertices[i] * (Random.value * 0.01f + 1);
            for(int j = i+1; j < mesh.vertexCount; j++)
            {
                if (b[j])
                    continue;

                if (mesh.vertices[i] == mesh.vertices[j])
                {
                    v[j] = val;
                    b[j] = true;
                }
            }
            v[i] = val;
            b[i] = true;
        }
        mesh.vertices = v;*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
