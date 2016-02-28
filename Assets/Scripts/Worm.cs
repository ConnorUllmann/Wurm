using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Worm : MonoBehaviour {

    public GameObject head;
    public List<GameObject> mandibles;
    public float mandibleAngleMin = -80; //Jaws open all the way!
    public float mandibleAngleMax = 20; //Jaws shut all the way!

    public float bodyLength;
    public int nBodyParts;
    public GameObject bodyPartPrefab;
    public List<GameObject> bodyParts;

    public float minGroundSpeed;
    public float groundSpeedX;
    public float groundSpeedNormal;
    public float airSpeed;
    public float frequencyMult;
    public float time = 0;
    public float ySpeedMax = 1000;
    public float ySpeedMin = -500;
    //public float headRotationAmountOnKeyPress = 90; //Degrees
    public float headRotation = 0;

    public GameObject target;
    public GameObject debugSphere;

    /* Updated every frame */
    public Quaternion quatUp;
    public float groundSpeed;
    public Vector3? epicenter;         //Position of the worm on the planet's surface.
    public float? dEpicenter;          //Distance from the center of the planet to the surface at the position of the worm.
    public float dCenter;              //Distance from center of the planet to the worm.
    public Vector3 forward = Vector3.forward;            //Direction the worm is moving tangentially to the planet SPHERE
    public Vector3 normal = Vector3.up;             //Direction of the surface at the epicenter;
    public Vector3 facing = Vector3.forward;             //Direction of motion;
    public Vector3 up = Vector3.up;                 //Direction from the center of the sphere up.
    public Vector3 right = Vector3.right;           //Direction from the center of the sphere right.
    public Vector3 diff = Vector3.zero;               //Vector from the planet to the worm.
    public float gravityValue;         //Amount of gravity (> 0)
    public Rigidbody rb;               //The player's rigidbody
    public Vector3 vel = Vector3.zero;                //Velocity applied to the worm head.
    /***********************/

    public float depthTo;       //The depth that the worm will tween to when underground.
    public float depth { get { if (!dEpicenter.HasValue) return float.MinValue; return dEpicenter.Value - dCenter; } } //positive means below-ground.
    public bool underground {  get { if (!dEpicenter.HasValue) return false; return dCenter <= dEpicenter.Value; } }
    private bool undergroundLast = false;

    float nVelY = 1;
    float velYSignLast = 1;
    float velYSign = 1;
    float cameraSideToSideMult = 150;

    //Inputs
    float h_input;
    float v_input;
    bool z_input;
    bool x_input;

    public List<Vector3> positions;
    public List<Quaternion> quaternions;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();
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

    // Update is called once per frame
    void Update () {
        time += Time.deltaTime;

        OrientSelf();

        //Debug.DrawRay(epicenter.Value, forward * 1000);
        Debug.DrawRay(epicenter.Value, up * 1100, Color.blue);
        Debug.DrawRay(epicenter.Value, right * 1000, Color.red);
        Debug.DrawRay(transform.position, facing * 1000, Color.green);

        UpdateMandibles();

        UpdateInputs();

        UpdateSpeed();
        Debug.DrawRay(transform.position + new Vector3(5, 0, 0), quatUp * Vector3.forward * 1000, Color.yellow);
        Debug.DrawRay(transform.position + new Vector3(0, 5, 0), quatUp * Vector3.up * 1000, Color.cyan);// 1000 * vel.normalized, Color.cyan);
        Debug.DrawRay(transform.position, quatUp * Vector3.right * 1000, Color.magenta);

        UpdateCamera();
        UpdateBodyParts();
        DrawPositions();

        undergroundLast = underground;
        velYSignLast = velYSign;
    }

    //Sets all of the vectors and distance variables to orient the worm in the world.
    //Returns the quaternion used to take local space coordinates and convert to world space.
    void OrientSelf()
    {
        diff = transform.position - PlanetObj.S.transform.position;
        up = diff.normalized;
        dCenter = diff.magnitude;
        facing = rb.velocity.normalized;
        forward = (facing - Vector3.Project(facing, up)).normalized;
        right = Vector3.Cross(up, facing);
        quatUp = Quaternion.LookRotation(forward, up);

        RaycastHit? hitInfo = PlanetObj.GetEpicenter(transform.position);
        if (hitInfo != null)
        {
            epicenter = hitInfo.Value.point;
            normal = hitInfo.Value.normal;
            dEpicenter = (epicenter.Value - PlanetObj.S.transform.position).magnitude;
        }
    }

    float angX;
    void UpdateMandibles()
    {
        for (int i = 0; i < mandibles.Count; i++)
        {
            var nsin = (Mathf.Sin(time * 4 + 0.5f * Mathf.Sin(i % (mandibles.Count / 2)) * 2 * Mathf.PI / (mandibles.Count / 2)) + 1) / 2;

            var m = 100;
            if (depth >= 0)
            {
                angX += (mandibleAngleMax - angX) * 0.5f;
            }
            else if (depth < -m)
            {
                //Up in the air
                angX = mandibleAngleMin;
            }
            else if(vel.y > 0)
            {
                var n = depth / m + 1;
                angX += ((mandibleAngleMax - mandibleAngleMin) * n + mandibleAngleMin - angX) * 0.5f;
            }
            else if(vel.y < 0)
            {
                var n = depth / m + 1;
                angX += ((mandibleAngleMax - mandibleAngleMin) * n + mandibleAngleMin - angX) * 0.5f;
            }

            mandibles[i].transform.localEulerAngles = new Vector3(
                angX, 
                mandibles[i].transform.localEulerAngles.y, 
                mandibles[i].transform.localEulerAngles.z
            );
        }
    }

    void UpdateBodyParts()
    {
        Quaternion quatHead = head.transform.rotation;
        if (rb.velocity.sqrMagnitude > 1)
        {
            head.transform.LookAt(head.transform.position + rb.velocity, up);
            headRotation -= h_input * 2f;
            if (depth > -transform.localScale.x * 20 && x_input && Mathf.Abs(h_input) <= 0.5f)
                headRotation += 35 * Mathf.Sin(Time.time * 5) / (Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z) / minGroundSpeed);
            head.transform.Rotate(new Vector3(headRotation, 90, 90));
            quatHead = head.transform.rotation;
        }
        headRotation %= 360;
        headRotation += Utils.angleDiffDeg(headRotation, 0) * 0.03f;

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
            bodyParts[i].transform.rotation = GetQuaternionAlongPath(1 - n * bodyPercent * stretchMult - startOffset) * Quaternion.Euler(new Vector3(-90, -90, 0));
        }
    }

    void UpdateInputs()
    {
        h_input = Input.GetAxis("Horizontal");
        v_input = Input.GetAxis("Vertical");
        z_input = Input.GetKey(KeyCode.Z);
        x_input = Input.GetKey(KeyCode.X);
    }

    void UpdateSpeed()
    {
        vel.z = Utils.sign(vel.z + v_input, false) * Mathf.Max(Mathf.Min(Mathf.Abs(vel.z + v_input), groundSpeed), minGroundSpeed);
        vel.x += (Utils.sign(h_input) * 12 * (Mathf.Abs(vel.z) / groundSpeed) - vel.x) * 0.15f;
        //vel.x += 0.5f * Mathf.Sin(Time.time * Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z) / minGroundSpeed);


        if (underground)
        {
            if (z_input)
            {
                if (vel.y > 0)
                    vel.y += 750 * Time.deltaTime;
            }

            vel.y -= (depthTo - depth) * 0.25f;
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
        if (x_input && vel.y > 0)
        {
            if (vel.y > 100)
                vel.y *= 0.75f;
            else
                vel.y *= 0.99f;
        }

        if (x_input)
        {
            groundSpeed = groundSpeedX;
        }
        else
        {
            groundSpeed = groundSpeedNormal;
        }

        if (underground && vel.y < ySpeedMin)
            vel.y = ySpeedMin;
        if (vel.y > ySpeedMax)
            vel.y = ySpeedMax;
        //vel.y = Utils.sign(vel.y) * Mathf.Min(Mathf.Abs(vel.y), ySpeedMax);
        
        rb.velocity = quatUp * vel;
    }

    void UpdateCamera()
    {
        velYSign = Utils.sign(vel.y, false);
        bool changedVerticalDirection = velYSignLast != velYSign;
        if (changedVerticalDirection)
        {
            if (velYSign > 0)
            {
                cameraSideToSideMult = -cameraSideToSideMult;
            }
        }

        float _nVelY = Mathf.Pow(Mathf.Max(-depth, 0) / 100f, 0.1f);
        nVelY = Mathf.Sin(Mathf.Max(-depth, 0) / 100f * Mathf.PI / 2);// += Utils.sign(_nVelY - nVelY) * 0.001f;
        var origin = transform.position;
        if (underground && epicenter.HasValue)
            origin = epicenter.Value;
        Camera.main.transform.position += (origin + right * cameraSideToSideMult * nVelY + up * 400 - forward * (230 + 15 * Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z) / minGroundSpeed) - Camera.main.transform.position) * 0.05f;
        Camera.main.transform.LookAt(transform.position, up);
    }

    //Draws a continuous line through all positions in the positions list.
    void DrawPositions()
    {
        for (int i = 0; i < positions.Count - 1; i++)
        {
            var o = (i + 1) % positions.Count;
            Debug.DrawLine(positions[i], positions[o], Color.Lerp(Color.white, Color.black, i * 1f / (positions.Count - 2)));
        }
    }

    void OnTriggerEnter(Collider c)
    {
        Debug.Log(c.gameObject.name + " hit! " + c.gameObject.tag);
        if(c.gameObject.tag == "Enemy")
        {
            c.gameObject.GetComponent<Enemy>().Hit(this);
        }
        else if(c.gameObject.tag == "Wall")
        {
            c.GetComponent<Breakable>().BreakUp(this.gameObject);
        }
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
