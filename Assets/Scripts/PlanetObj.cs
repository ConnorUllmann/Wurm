using UnityEngine;
using System.Collections;

public class PlanetObj : MonoBehaviour {

    public static PlanetObj S;

    public GameObject _player;

    public float safeHeight;

	// Use this for initialization
	void Awake ()
    {
        S = this;
        safeHeight = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) / 2 + 1000000;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Returns the position as projected onto the surface of the planet.
    public static RaycastHit? GetEpicenter(Vector3 pos)
    {
        var diff = pos - position;
        var direction = diff.normalized;
        var origin = direction * S.safeHeight + position;

        RaycastHit hitInfo;
        LayerMask layerPlanet = 1 << 8;
        if (Physics.Raycast(origin, -direction, out hitInfo, S.safeHeight, layerPlanet))
            return hitInfo;
        //Debug.Log("Null!");
        return null;
    }

    public static Vector3 position { get { return PlanetObj.S.transform.position; } set { PlanetObj.S.transform.position = value; } }
    public static GameObject player { get { return PlanetObj.S._player; } set { PlanetObj.S._player = value; } }
}
