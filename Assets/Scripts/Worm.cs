using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Worm : MonoBehaviour {

    public GameObject head;
    public List<GameObject> mandibles;
    public float mandibleAngleMin = -80;
    public float mandibleAngleMax = 20;

    public float bodyLength;
    public int nBodyParts;
    public GameObject bodyPartPrefab;
    public List<GameObject> bodyParts;

    public float groundSpeed;
    public float minGroundSpeed;
    public float airSpeed;
    public float frequencyMult;
    public float time = 0;
    public float ySpeedMax = 1000;
    public float ySpeedMin = -500;
    public float headRotationAmountOnKeyPress = 90; //Degrees

    public GameObject target;
    public GameObject planet;
    public GameObject debugSphere;

    /* Updated every frame */
    public Vector3? epicenter;         //Position of the worm on the planet's surface.
    public float? dEpicenter;          //Distance from the center of the planet to the surface at the position of the worm.
    public float dCenter;              //Distance from center of the planet to the worm.
    public Vector3 forward = Vector3.forward;            //Direction the worm is moving tangentially to the planet SPHERE
    public Vector3 normal = Vector3.up;             //Direction of the surface at the epicenter;
    public Vector3 facing = Vector3.forward;             //Direction of motion;
    public Vector3 up = Vector3.up;                 //Direction from the center of the sphere up.
    public Vector3 diff = Vector3.zero;               //Vector from the planet to the worm.
    public float gravityValue;         //Amount of gravity (> 0)
    public Rigidbody rb;               //The player's rigidbody
    public Vector3 vel = Vector3.zero;                //Velocity applied to the worm head.
    /***********************/

    public float depthTo;       //The depth that the worm will tween to when underground.
    public float depth { get { if (!dEpicenter.HasValue) return float.MinValue; return dEpicenter.Value - dCenter; } } //positive means below-ground.
    public bool underground {  get { if (!dEpicenter.HasValue) return false; return dCenter <= dEpicenter.Value; } }
    private bool undergroundLast = false;

    public List<Vector3> positions;
    public List<Quaternion> quaternions;

	// Use this for initialization
	void Start () {
        foreach (Transform t0 in transform)
        {
            if (t0.name == "Head")
            {
                foreach (Transform t in t0.transform)
                {
                    if (t.name.Substring(0, Mathf.Min(t.name.Length, 8)) == "Mandible")
                    {
                        mandibles.Add(t.gameObject);
                    }
                }
            }
        }
        for(int i = 0; i < nBodyParts; i++)
        {
            bodyParts.Add(Instantiate<GameObject>(bodyPartPrefab));
            bodyParts[i].transform.parent = transform;
            bodyParts[i].transform.position = transform.position;
            bodyParts[i].transform.localScale = new Vector3(10, 10, 10) * Filter(i * 1.0f / (nBodyParts - 1));
        }
    }

    float nVelY = 1;
    float velYSignLast = 1;
    float cameraSideToSideMult = 150;
    // Update is called once per frame
    void Update () {
        time += Time.deltaTime;

        rb = GetComponent<Rigidbody>();
        diff = transform.position - planet.transform.position;
        up = diff.normalized;
        dCenter = diff.magnitude;
        facing = rb.velocity.normalized;
        forward = (facing - Vector3.Project(facing,up)).normalized;
        var right = Vector3.Cross(up, facing);
        Quaternion quatUp = Quaternion.LookRotation(forward, up);

        RaycastHit? hitInfo = GetEpicenter(); 
        if (hitInfo != null)
        {
            epicenter = hitInfo.Value.point;
            normal = hitInfo.Value.normal;
            dEpicenter = (epicenter.Value - planet.transform.position).magnitude;
        }

        //Debug.DrawRay(epicenter.Value, forward * 1000);
        Debug.DrawRay(epicenter.Value, up * 1100, Color.blue);
        Debug.DrawRay(epicenter.Value, right * 1000, Color.red);
        Debug.DrawRay(transform.position, facing * 1000, Color.green);
        
        for (int i = 0; i < mandibles.Count; i++)
        {
            var nsin = (Mathf.Sin(time * 4 + 0.5f * Mathf.Sin(i  % (mandibles.Count / 2)) * 2 * Mathf.PI / (mandibles.Count / 2)) + 1) / 2;
            mandibles[i].transform.localEulerAngles = new Vector3((mandibleAngleMin + (mandibleAngleMax - mandibleAngleMin) * nsin + 360) % 360, mandibles[i].transform.localEulerAngles.y, mandibles[i].transform.localEulerAngles.z);
        }

        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");

        vel.z = Utils.sign(vel.z + v_input, false) * Mathf.Max(Mathf.Min(Mathf.Abs(vel.z + v_input), groundSpeed), minGroundSpeed);
        vel.x += (Utils.sign(h_input) * 12 * (Mathf.Abs(vel.z) / groundSpeed) - vel.x) * 0.15f;


        if (underground)
        {
            if (Input.GetKey(KeyCode.Z) && vel.y > 0)
            {
                vel.y += 500 * Time.deltaTime;
            }

            vel.y -= (depthTo - depth) * 0.5f;
            //If the player is slowing down, slow the worm's vertical movement
            if (vel.y < 0)
            {
                var tempVelY = vel.y - vel.y / 4 * Mathf.Abs(Mathf.Min(v_input, 0));
                vel.y = tempVelY;
            }
        }
        else
        {
            vel.y -= gravityValue;
        }
        if (Input.GetKey(KeyCode.X) && vel.y > 0)
        {
            if (vel.y > 100)
                vel.y *= 0.75f;
            else
                vel.y *= 0.99f;
        }

        if (underground && vel.y < ySpeedMin)
            vel.y = ySpeedMin;
        if (vel.y > ySpeedMax)
            vel.y = ySpeedMax;
        //vel.y = Utils.sign(vel.y) * Mathf.Min(Mathf.Abs(vel.y), ySpeedMax);

        float velYSign = Utils.sign(vel.y, false);
        bool changedVerticalDirection = velYSignLast != velYSign;
        if (changedVerticalDirection)
        {
            if(velYSign > 0)
            {
                cameraSideToSideMult = -cameraSideToSideMult;
            }
        }

        Debug.DrawRay(transform.position + new Vector3(5, 0, 0), quatUp * Vector3.forward * 1000, Color.yellow);
        Debug.DrawRay(transform.position + new Vector3(0, 5, 0), quatUp * Vector3.up * 1000, Color.cyan);// 1000 * vel.normalized, Color.cyan);
        Debug.DrawRay(transform.position, quatUp * Vector3.right * 1000, Color.magenta);

        rb.velocity = quatUp * vel;

        float _nVelY = Mathf.Pow(Mathf.Max(-depth, 0)/100f, 0.1f);
        nVelY = Mathf.Sin(Mathf.Max(-depth, 0) / 100f * Mathf.PI / 2);// += Utils.sign(_nVelY - nVelY) * 0.001f;
        Camera.main.transform.position += (transform.position + right * cameraSideToSideMult * nVelY + up * 400 - forward * 300 - Camera.main.transform.position) * 0.05f;
        Camera.main.transform.LookAt(transform.position, up);
        

        Quaternion quatHead = head.transform.rotation;
        if (rb.velocity.sqrMagnitude > 1)
        {
            head.transform.LookAt(head.transform.position + rb.velocity, up);
            quatHead = head.transform.rotation;
            head.transform.Rotate(new Vector3(headRotationAmountOnKeyPress * h_input, 90, 90));
        }
        quaternions.Add(quatHead);
        positions.Add(transform.position);
        
        float startOffset = 0.1f / (nBodyParts / 10);
        float stretchMult = 0.6f;
        float _bodyLength = bodyLength * transform.localScale.x;
        while (GetPathLength() > _bodyLength)
        {
            positions.RemoveAt(0);
            quaternions.RemoveAt(0);
        }
        float bodyPercent = _bodyLength / GetPathLength();
        for (int i = 0; i < nBodyParts; i++)
        {
            var n = i * 1.0f / (nBodyParts - 1);
            n = 1 - Defilter(n);
            bodyParts[i].transform.position = GetPositionAlongPath(1 - n * bodyPercent * stretchMult - startOffset);
            bodyParts[i].transform.rotation = GetQuaternionAlongPath(1 - n * bodyPercent * stretchMult - startOffset);
        }
        for(int i = 0; i < positions.Count-1; i++)
        {
            var o = (i + 1) % positions.Count;
            Debug.DrawLine(positions[i], positions[o], Color.Lerp(Color.white, Color.black, i * 1f / (positions.Count - 2)));
        }

        undergroundLast = underground;
        velYSignLast = velYSign;
    }

    void OnTriggerEnter(Collider c)
    {
        Debug.Log(c.gameObject.name + " hit!");
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

    float Filter(float t)
    {
        return Mathf.Sqrt(t);
    }
    float Defilter(float t)
    {
        return t * t;
    }
    float GetPathLength(bool loop=false)
    {
        float dPositions = 0;
        for (int i = 0; i < positions.Count - (loop ? 0 : 1); i++)
        {
            var o = (i + 1) % positions.Count;
            dPositions += (positions[o] - positions[i]).magnitude;
        }
        return dPositions;
    }
    Vector3 GetPositionAlongPath(float t, bool loop = false)
    {
        float dPositions = GetPathLength(loop);
        float dSum = 0;
        for (int i = 0; i < positions.Count - (loop ? 0 : 1); i++)
        {
            var o = (i + 1) % positions.Count;
            var diff = positions[o] - positions[i];
            var dDiff = diff.magnitude;
            var dSumPrev = dSum;
            dSum += dDiff;
            if (t <= dSum / dPositions && t >= dSumPrev / dPositions)
            {
                float percent = (t - dSumPrev / dPositions) * dPositions / dDiff;
                return positions[i] + diff.normalized * percent * dDiff;
            }
        }
        return Vector3.zero;
    }
    Quaternion GetQuaternionAlongPath(float t, bool loop = false)
    {
        float dPositions = GetPathLength(loop);
        float dSum = 0;
        for (int i = 0; i < positions.Count - (loop ? 0 : 1); i++)
        {
            var o = (i + 1) % positions.Count;
            var diff = positions[o] - positions[i];
            var dDiff = diff.magnitude;
            var dSumPrev = dSum;
            dSum += dDiff;
            if (t <= dSum / dPositions && t >= dSumPrev / dPositions)
            {
                float percent = (t - dSumPrev / dPositions) * dPositions / dDiff;
                return Quaternion.Slerp(quaternions[i], quaternions[o], percent * dDiff);
            }
        }
        return Quaternion.identity;
    }
}
