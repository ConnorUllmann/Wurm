using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

    public float lookSpeed = 5f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if(Mathf.Abs(mouseX) > Mathf.Abs(mouseY)) {
            mouseY = 0;
        } else
        {
            mouseX = 0;
        }

        transform.Rotate(-mouseY * lookSpeed, mouseX * lookSpeed, 0);
        if(transform.rotation.y > 45)
        {
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, 45, transform.rotation.eulerAngles.z));
        }
        else if(transform.rotation.y < -45)
        {
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, -45, transform.rotation.eulerAngles.z));
        }
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0));
	}
}
