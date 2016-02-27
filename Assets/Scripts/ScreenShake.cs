using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour {

    public static ScreenShake S;

    public float shakeTimer = 0;
    private float shakeMult = 1;
    private Vector3 diff;
    // Use this for initialization
    void Start ()
    {
        S = this;
        diff = Vector3.zero;
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        Camera.main.transform.localPosition -= diff;

        if (shakeTimer > 0)
        {
            diff = Random.insideUnitSphere * shakeTimer * shakeTimer * shakeMult;
            shakeTimer = Mathf.Max(shakeTimer - Time.deltaTime, 0);
        }
        else
            diff = Vector3.zero;

        Camera.main.transform.localPosition += diff;
    }

    public static void Shake(float t)
    {
        S.shakeTimer = t;
    }

    public static Vector3 CameraOffset { get { return S.diff; } }
}
