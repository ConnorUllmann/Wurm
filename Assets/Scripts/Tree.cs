using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tree : MonoBehaviour {

    Vector3 wind = Vector3.zero;

    private List<Transform> leaves;
    private List<Vector3> leavesOffsetStart;
    private List<Vector3> leavesOffset;

    public Vector3 tipDirection;
    public float tipAngleRate = 0;
    public float tipAngle = 0;
    public float tipAngleMax = 90;
    public bool tip = false;

    Vector3 n;

    // Use this for initialization
    void Start () {
        leaves = new List<Transform>();
        leavesOffsetStart = new List<Vector3>();
        leavesOffset = new List<Vector3>();
        foreach (Transform t in transform)
        {
            if (t.name.Contains("Leaves"))
            {
                leaves.Add(t);
                leavesOffsetStart.Add(t.localPosition);
                leavesOffset.Add(Vector3.zero);
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {

        var epicenter = PlanetObj.GetEpicenter(transform.position).Value;
        transform.position = epicenter.point;

        var diff = transform.position - PlanetObj.position;
        n = epicenter.normal;
        var facing = Random.insideUnitSphere;
        var forward = (facing - Vector3.Project(facing, n)).normalized;
        transform.LookAt(forward, n);
        transform.Rotate(new Vector3(-90, 0, 0));

        if (tip)
        {
            tipAngleRate += Time.deltaTime * tipDirection.magnitude / 30f;
            tipAngle = Mathf.Min(tipAngle + tipAngleRate, tipAngleMax);
            var right = Vector3.Cross(tipDirection, n).normalized;
            transform.Rotate(right, tipAngle);
            Debug.DrawRay(transform.position, tipDirection, Color.white);
        }
        //transform.Rotate(new Vector3(tipAngle, 0, 0));

        if (!tip)
        {
            if (wind.magnitude < 0.001f)
            {
                wind = 40 * Time.deltaTime * Random.insideUnitSphere;
            }
        }
        else
            wind *= 0.9f;

        for (int i = 0; i < leaves.Count; i++)
        {
            leavesOffset[i] += Random.value * wind;
            leavesOffset[i] = Vector3.Lerp(leavesOffset[i], Vector3.zero, 0.1f);
            leaves[i].localPosition = leavesOffsetStart[i] + leavesOffset[i];
        }
    }

    public void TipOver(GameObject o)
    {
        if (!tip)
        {
            tip = true;
            //Store direction of tip as well as amount of tip.
            tipDirection = o.GetComponent<Rigidbody>().velocity.magnitude * (o.GetComponent<Rigidbody>().velocity - Vector3.Project(o.GetComponent<Rigidbody>().velocity, n)).normalized;
            //tipDirection = (transform.position - o.transform.position).normalized * o.GetComponent<Rigidbody>().velocity.magnitude;
        }
    }
}
