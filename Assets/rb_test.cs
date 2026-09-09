using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rb_test : MonoBehaviour
{
    public GameObject target;

    float MOVE_force = 20;
    float positionDamper = 50;

    float ROT_force = 45;
    float rotationDamper = 400;

    void FixedUpdate()
    {
        if (target == null) return;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // --- Position (velocity based) ---
        Vector2 toTarget = target.transform.position - transform.position;
        Vector2 desiredVelocity = toTarget * MOVE_force;
        desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, positionDamper);
        rb.velocity = desiredVelocity; // use rb.velocity if on Unity < 6

        // --- Rotation (angular velocity based) ---
        float angleDiff = Mathf.DeltaAngle(rb.rotation, target.transform.eulerAngles.z);
        float desiredAngularVelocity = angleDiff * ROT_force;
        desiredAngularVelocity = Mathf.Clamp(desiredAngularVelocity, -rotationDamper, rotationDamper);
        rb.angularVelocity = desiredAngularVelocity;
    }
}
