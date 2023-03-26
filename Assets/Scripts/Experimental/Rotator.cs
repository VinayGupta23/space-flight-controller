using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 initialOrientation;
    public Vector3 finalOrientation;
    public float rotationTime = 2f;

    private Quaternion curRotation;
    private bool isRotating = false;
    private float curTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRotating)
        {
            return;
        }

        curTime += Time.deltaTime;
        if (curTime > rotationTime)
        {
            isRotating = false;
            return;
        }

        float alpha = curTime / rotationTime;
        // Vector3 curOrientation = initialOrientation * (1 - alpha) + finalOrientation * alpha;
        Vector3 curOrientation = Vector3.Lerp(initialOrientation, finalOrientation, alpha);
        curRotation.eulerAngles = curOrientation;
        transform.rotation = curRotation;
    }

    [ContextMenu("Restart")]
    public void Restart()
    {
        curRotation.eulerAngles = initialOrientation;
        transform.rotation = curRotation;
        isRotating = true;
        curTime = 0;
    }

    [ContextMenu("Pitch")]
    public void Pitch()
    {
        curRotation.eulerAngles += new Vector3(90, 0, 0);
        transform.rotation = curRotation;
    }

    [ContextMenu("Roll")]
    public void Roll()
    {
        curRotation.eulerAngles += new Vector3(0, 0, 90);
        transform.rotation = curRotation;
    }

    [ContextMenu("Yaw")]
    public void Yaw()
    {
        curRotation.eulerAngles += new Vector3(0, 90, 0);
        transform.rotation = curRotation;
    }
}
