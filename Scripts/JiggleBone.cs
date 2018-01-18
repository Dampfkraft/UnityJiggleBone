using UnityEngine;

public class JiggleBone : MonoBehaviour {
    // physics constants
    public float stiffness = 120f;
    public float mass = 1f;
    public float damping = 15f;
    public float gravity = 9.81f;

    // stretch deform
    public bool stretch = true;
    public float sideStretch = 0.15f;
    public float frontStretch = 0.2f;

    // limits
    public Vector3 lowerAngularLimit = Vector3.negativeInfinity;
    public Vector3 upperAngularLimit = Vector3.positiveInfinity;

    public Vector3 lowerScaleLimit = Vector3.one;
    public Vector3 upperScaleLimit = Vector3.positiveInfinity;

    // display debug lines
    public bool debugRender = false;

    // physics variables
    Vector3 vel = new Vector3();
    Vector3 dynamicPos = new Vector3();

    // bind rotation of the bone
    Quaternion bindRot = new Quaternion();

    void Awake()
    {
        // save the original forward direction of the bone, the bone will strive to return to this direction
        bindRot = transform.localRotation;
        
        // initially set the dynamic point to the forward direction of the bone in world space
        dynamicPos = transform.position + transform.forward;
    }

    // run on LateUpdate so that movement of the model that affects jiggle is already calculated
    void LateUpdate()
    {
        // sanitize input
        if (damping < 0f) damping = 0f;
        if (mass <= 0f) mass = 0.01f;

        // return to bind rotation to calculate the deviation of the bone forward direction
        // from the target point in world space.
        transform.localRotation = bindRot;

        Vector3 forwardVector = transform.forward;
        Vector3 targetPos = transform.position + forwardVector;
        Vector3 displacement = (targetPos - dynamicPos);

        // damping represents the friction that reduces speed over time.
        // low damping allows the bone to actually jiggle by speeding over
        // its target and accelerating back and forth.
        vel /= 1 + damping * Time.deltaTime;

        // determine force to apply by multiplying displacement with stiffness factor.
        // greater displacement or higher stiffness therefore increase the pressure
        // to return into the bones bind position.
        Vector3 force = displacement * stiffness;

        // the higher the mass, the more force is needed to accelerate it
        Vector3 acc = force / mass;
        acc.y -= gravity;

        // accelerate into the direction of the force
        vel += acc * Time.deltaTime;

        // move the world space dynamic point based on the current velocity
        dynamicPos += vel * Time.deltaTime;

        // rotate the bone forward vector towards the dynamic point in world space
        transform.LookAt(dynamicPos, transform.up);

        Vector3 rot = transform.eulerAngles;

        // clamp
        if (rot.x < lowerAngularLimit.x) rot.x = lowerAngularLimit.x;
        else if (rot.x > upperAngularLimit.x) rot.x = upperAngularLimit.x;
        if (rot.y < lowerAngularLimit.y) rot.y = lowerAngularLimit.y;
        else if (rot.y > upperAngularLimit.y) rot.y = upperAngularLimit.y;
        if (rot.z < lowerAngularLimit.z) rot.z = lowerAngularLimit.z;
        else if (rot.z > upperAngularLimit.z) rot.z = upperAngularLimit.z;

        transform.eulerAngles = rot;

        if (stretch)
        {
            // stretch the bone towards the dynamic point
            Vector3 scale = lowerScaleLimit + new Vector3(-sideStretch, -sideStretch, frontStretch) * displacement.magnitude;

            // clamp
            if (scale.x > upperScaleLimit.x) scale.x = upperScaleLimit.x;
            if (scale.y > upperScaleLimit.y) scale.y = upperScaleLimit.y;
            if (scale.z > upperScaleLimit.z) scale.z = upperScaleLimit.z;

            transform.localScale = scale;
        }

        if (debugRender)
        {
            Debug.DrawRay(transform.position, forwardVector, Color.blue); // bind pose forward vector
            Debug.DrawRay(transform.position, transform.forward, Color.magenta); // current forward vector
            Debug.DrawRay(dynamicPos, Vector3.up * 0.2f, Color.red); // current target point
            Debug.DrawRay(transform.position + transform.forward * displacement.magnitude,
                Vector3.up * 0.2f, Color.green); // stretch magnitude visualisation
        }
    }
}
