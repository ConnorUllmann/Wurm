using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour {
    
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static float angleDiff(float a, float b)
    {
        var diff = b - a;

        while (diff > Mathf.PI) { diff -= 2 * Mathf.PI; }
        while (diff <= -Mathf.PI) { diff += 2 * Mathf.PI; }

        return diff;
    }
    public static float angleDiffDeg(float a, float b)
    {
        var diff = b - a;

        while (diff > 180) { diff -= 360; }
        while (diff <= -180) { diff += 360; }

        return diff;
    }

    public static int sign(float x, bool canReturnZero = true)
    {
        if (canReturnZero && x == 0)
            return 0;
        return x >= 0 ? 1 : -1;
    }

    public static float easeOutElastic(float t)
    {
        var p = 0.3;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((float)((t - p / 4f) * (2f * Mathf.PI) / p)) + 1;
    }

    public static Vector3 mouse { get { return Camera.main.ScreenToWorldPoint(Input.mousePosition); } }
    public static float GetAngleToMouse(Vector3 pos) { var m = mouse; return Mathf.Atan2(m.y - pos.y, m.x - pos.x); }
    
}
