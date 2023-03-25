using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonFollower : MonoBehaviour
{
    public ShipController targetShip;

    public Vector3 cameraOffset = new Vector3(0, 4, -11);
    public float rotateResponseTime = 0.25f;
    public float moveResponseTime = 0.1f;
    public float maxRotateSpeed = 60f;
    public float maxMoveSpeed = 65f;

    float _rotateVelocity = 0;
    Vector3 _moveVelocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = targetShip.transform.rotation;
        transform.position = targetShip.transform.TransformPoint(cameraOffset);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Note: We can visualize the TPS camera as an object that rotates
        // around the ship (imagine a sphere with the ship as the center).

        // Let the ship turn causing the forward vector to rotate by some angle.
        // We need to move the camera along this very spherical path, as defined
        // by the forward vectors before and after.

        if (transform.rotation != targetShip.transform.rotation)
        {
            float errorAngle = Quaternion.Angle(transform.rotation, targetShip.transform.rotation);
            float currentAngle = Mathf.SmoothDamp(0, errorAngle, ref _rotateVelocity, rotateResponseTime, maxRotateSpeed);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetShip.transform.rotation, currentAngle);
        }

        // Perform similar smoothing on movement, ideally with faster response time.
        // This creates better "feel" as user can see the ship leaning towards the desired axis.

        Vector3 targetPosition = targetShip.transform.TransformPoint(cameraOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _moveVelocity, moveResponseTime, maxMoveSpeed);
    }
}
