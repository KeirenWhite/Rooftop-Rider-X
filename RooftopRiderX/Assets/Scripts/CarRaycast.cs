using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
//using UnityEngine.PhysicsModule;


/* Hierarchy of Bike raycast gameobject parts:
 
    Bike                 script (rigidbody 1000lbs, collider, playerinput)
        moveto          empty (for camera to follow)
        anchorFR        empty (for steer wheel angles)
            wheelFR     mesh (spherecollider)
            tireFR      trail renderer (position to bottom of wheel)
            raycastFR   how far to raycast for suspension  
        anchorBL 
            wheelBL 
            tireBL
            raycastBL
*/

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]   
[RequireComponent(typeof(PlayerInput))] 
public class CarRaycast : MonoBehaviour
{
    private IEnumerator coroutine;
    public struct InputInfo
    {
        public Vector2 steer;
        public float gas;
        public float reverse;
        public float brake;
        public int grounded;
        public float roll;
        public Vector2 flip;
        //public Vector2 wheelie;
        public bool downed;
        public float reset;
        public float jump;
    }
    [System.Serializable]
    public struct WheelInfo
    {
        public GameObject anchor;
        public GameObject wheel;
        public TrailRenderer tire;
        public TrailRenderer airTire;
        public GameObject raycast;      //how far down to ground to raycast        
        public GameObject groundRaycast;
        [HideInInspector]
        public float maxDistance;       //calculated in start (anchor - raycastTo position)        
        [HideInInspector]
        public float groundedMaxDistance;
        [HideInInspector]
        public float radius;            //calculated in start (mesh bounds)


        public static WheelInfo CreateDefault()
        {
            return new WheelInfo
            {
                //springStrength = 280000,
                //dampingFactor = 0.1f
            };            
        }
    }

    [SerializeField] private float stabilizationSensitivity = 60f;
    [SerializeField] private float airStabilizationSensitivityZ = 10f;
    [SerializeField] private float airTurnMult = 0.7f;
    public float airTurnSpeed = 1.5f;
    public float airFlipSpeed = 2f;
    public float wheelieSpeed = 100f;
    public float Movespeed = 35;
    public float turnSpeed = 90;
    public float brakeStrength = 5;
    public float jumpStrength = 5;
    public GameObject WheelF;
    public GameObject WheelB;
    public GameObject AxelF;
    public GameObject HandleBar;
    public WheelInfo FR = WheelInfo.CreateDefault();
    public WheelInfo BL = WheelInfo.CreateDefault();
    [SerializeField] private Collider bikeDownCol;
    [HideInInspector] public Rigidbody rb = null;
    private float origDrag;
    private float origAngDrag;
    [HideInInspector] public InputInfo input;
    private Quaternion initialRotationAxel;
    private Quaternion initialRotationWheelF;
    private Quaternion initialRotationHB;
    //private float maxRot = 45f;
    //public ConstantForce constantF;
    [SerializeField] private float extraGravity = 100f;

    private bool wasOnGround = false;
    private Quaternion holdoverRotation;
    private Quaternion deltaRotation;
    private Vector3 trickTrack;
    [SerializeField] private Boost boostScript;

    [SerializeField] private bool leanMode = false;
    [SerializeField] private GameObject bikeVisuals;
    [SerializeField] private GameObject visualsTransformTarget;
    //SerializeField] private float leanSpeedModifier = 1f;

    [Header("Jump Shtuff")]
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float jumpTimeToPeak = 10f;
    [SerializeField] private float jumpTimeToDescent = 10f;
    private float jumpVelocity;
    private float jumpGravity;
    private float fallGravity;

    [Header("Sound Effects")]
    public AudioSource landTrickAudio;
   

    private void Start()
    {
        Physics.bounceThreshold = 2000000;
        rb = GetComponent<Rigidbody>();
        //bikeBody = GetComponent<BoxCollider>();
        origDrag = rb.drag;
        origAngDrag = rb.angularDrag;

        initialRotationAxel = AxelF.transform.localRotation;
        initialRotationWheelF = WheelF.transform.localRotation;
        initialRotationHB = HandleBar.transform.localRotation;
        
        //size of each wheel (used to spin wheels)
        FR.radius = FR.wheel.GetComponent<Renderer>().bounds.extents.y;
        BL.radius = BL.wheel.GetComponent<Renderer>().bounds.extents.y;

        //distance to raycast to ground
        FR.maxDistance = FR.anchor.transform.position.y - FR.raycast.transform.position.y;
        BL.maxDistance = BL.anchor.transform.position.y - BL.raycast.transform.position.y;

        FR.groundedMaxDistance = FR.anchor.transform.position.y - FR.groundRaycast.transform.position.y;
        BL.groundedMaxDistance = BL.anchor.transform.position.y - BL.groundRaycast.transform.position.y;

        //coroutine = WaitForTurn(1);

        // initialize jump variables
        jumpVelocity = (2f * jumpHeight) / jumpTimeToPeak;
        jumpGravity = (-2f * jumpHeight) / (jumpTimeToPeak * jumpTimeToPeak);
        fallGravity = (-2f * jumpHeight) / (jumpTimeToDescent * jumpTimeToDescent);

    }

    private float GetGravity()
    {
        if (input.grounded == 2)
            return jumpGravity / 2;
        return rb.velocity.y < 0f ? fallGravity : jumpGravity;
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rb.velocity += new Vector3(0f, GetGravity() * Time.deltaTime, 0f) ;

        BikeGrounded();
        BikeDowned();
        
        FrameStabilize();

        BikeTurn();
        BikeJump();
        BikeMove();
        BikeBrake();

        UpdateBikeVisuals();
        //BikeLean();

        Trick();

        SpinWheels();

        if (input.grounded == 2)
            wasOnGround = true;

        //Debug.Log("speed: " + rb.velocity.magnitude);
        
    }

    [SerializeField] private GameObject wallRaycastPosition;
    [SerializeField] private float wallRaycastLength = 6f;
    //[SerializeField] private float slopeIgnore = 0.5f;
    [SerializeField] private float bikeHeight = 1.05f;
    private float snapCooldown = 0f;
    [SerializeField] private float snapCooldownTime = 0.35f;
    [SerializeField] private float snapSpeed;
    private void SnapToWall()
    {
        if (snapCooldown > 0f)
        {
            snapCooldown -= Time.deltaTime;
            return;
        }
        //Debug.Log("snap");

        RaycastHit hit;

        bool wallRaycastHit = false;

        if (Physics.Raycast(wallRaycastPosition.transform.position, transform.TransformDirection(Vector3.right), out hit, wallRaycastLength)) // shoot ray right
        {
            if (Vector3.Dot(Vector3.up, hit.normal) > 0.9)
            {
                return;
            }
            AirStabilizeTimer(true);
            wallRaycastHit = true;
        }
        else if (Physics.Raycast(wallRaycastPosition.transform.position, transform.TransformDirection(-Vector3.right), out hit, wallRaycastLength)) // shoot ray left
        {
            if (Vector3.Dot(Vector3.up, hit.normal) > 0.9)
            {
                return;
            }
            AirStabilizeTimer(true);
            wallRaycastHit = true;
        }

        if (wallRaycastHit)
        {
            Vector3 targetPosition = transform.position + hit.normal * ((0.5f * bikeHeight) - hit.distance);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapSpeed);

            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
    }

    private void UpdateBikeVisuals()
    {
        bikeVisuals.transform.position = visualsTransformTarget.transform.position;

        if (input.grounded >= 0)
            bikeVisuals.transform.rotation = Quaternion.Euler(
                visualsTransformTarget.transform.eulerAngles.x,
                visualsTransformTarget.transform.eulerAngles.y,
                visualsTransformTarget.transform.eulerAngles.z + (30 * input.steer.x));
        if (input.roll > 0 || input.downed == true)
            bikeVisuals.transform.rotation = visualsTransformTarget.transform.rotation;
    }

    [SerializeField] private float spherecastRadius = 0.25f;
    [SerializeField] private float suctionPower = 10f;
    private Vector3 collisionNormal = Vector3.up;
    private void FrameStabilize()
    {
        RaycastHit backHit;
        RaycastHit frontHit;

        Vector3 groundNormal = Vector3.up;


        bool[] frontBackIsHitting = new bool[2];


        if (Physics.SphereCast(BL.anchor.transform.position, spherecastRadius, -this.transform.up, out backHit, BL.maxDistance))
        {
            frontBackIsHitting[1] = true;
            groundNormal = backHit.normal;
        }
        else
        {
            frontBackIsHitting[1] = false;
        }

        if (Physics.SphereCast(FR.anchor.transform.position, spherecastRadius, -this.transform.up, out frontHit, FR.maxDistance))
        {
            frontBackIsHitting[0] = true;
            groundNormal = frontHit.normal;
        }
        else
        {
            frontBackIsHitting[0] = false;
        }

        if ((frontBackIsHitting[0] || frontBackIsHitting[1]) && input.grounded == 2)
        {
            AirStabilizeTimer(true);
            transform.rotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
        }

        //Debug.Log(frontBackIsHitting[0] + ", " + frontBackIsHitting[1]);

        
        //Debug.Log(frontHit.collider.name + ", " + backHit.collider.name);

        if (frontBackIsHitting[0] && frontBackIsHitting[1])
        {

            if (input.grounded == 0)
            {
                collisionNormal = ((backHit.normal + frontHit.normal) / 2).normalized;
                rb.AddForce(suctionPower * -collisionNormal, ForceMode.Force);
            }

            if (!wasOnGround && input.grounded == 2)
            {
                //Debug.Log("bounce negation");
                Vector3 velocityAlongNormal = Vector3.Project(rb.velocity, collisionNormal);
                rb.velocity = rb.velocity - velocityAlongNormal;
            }
        }
        
        /*if (!wasOnGround)
        {
            _Debug.Log(frontBackIsHitting[0] + ", " + frontBackIsHitting[1]);
        }*/
    }

    void AirFrameStabilize()
    {
        AirStabilizeTimer();
        if (airStabilizeTimer > 0f)
            return;

        RaycastHit hit;


        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            float groundNormal = hit.normal.y;

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.MoveTowardsAngle(transform.eulerAngles.z, 0, Time.deltaTime * airStabilizationSensitivityZ));
            //Debug.Log("stabilizing");
        }


        //Debug.DrawRay(transform.position, -this.transform.up, Color.blue);
    }

    private float airStabilizeTimer = 0f;
    [SerializeField] private float airStabilizeWaitTime = 1f;
    private void AirStabilizeTimer(bool reset = false, float overrideWaitTime = -1f) // if false, increments timer; if true, resets
    {
        if (reset)
        {
            airStabilizeTimer = overrideWaitTime == -1f ? airStabilizeWaitTime : overrideWaitTime;
            return;
        }

        airStabilizeTimer -= Time.deltaTime;
    }

    private void OnGUI()
    {
        
        //this is just to display debug text on screen
        /*
        GUIStyle style = new GUIStyle();
        style.fontSize = 28;
        style.normal.textColor = Color.black;
        GUI.Label(new Rect(10, 10, 300, 50), string.Format("steer {0}, gas {1}, brake {2}", input.steer.x, input.gas, input.brake), style);
        GUI.Label(new Rect(10, 30, 300, 50), string.Format("grounded {0}", input.grounded), style);
        */
    }
    private void OnSteer(InputValue value)
    {
        input.steer = value.Get<Vector2>();
        AirStabilizeTimer(true, 0.5f);
    }
    private void OnGas(InputValue value)
    {
        input.gas = value.Get<float>();
    }

    private void OnReverse(InputValue value)
    {
        input.reverse = value.Get<float>();
    }
    private void OnBrake(InputValue value)
    {
        input.brake = value.Get<float>();
    }

    private void OnRoll(InputValue value)
    {
        input.roll = value.Get<float>();
    }

    private void OnFlip(InputValue value)
    {
        input.flip = value.Get<Vector2>();
    }

    private void OnJump(InputValue value)
    {
        input.jump = value.Get<float>();
        AirStabilizeTimer(true);
    }

    /*private void onWheelie(InputValue value)
    {
        input.wheelie = value.Get<Vector2>();
    }*/

    private void OnReset(InputValue value)
    {
        //upright car
        if (input.downed)
        {
            snapCooldown = snapCooldownTime;
            this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z);
            input.downed = false;
        }
        //input.reset = value.Get<float>();
    }

    private void Trick()
    {
        if (input.grounded > 1)
            return;

        deltaRotation = Quaternion.Inverse(transform.rotation) * holdoverRotation;

        /* So multiplying deltaRotation by Vector3.up gives the correct x and z value, but since it's based on the rotation around the y axis,
         * it doesn't take yaw into account. Thus, by multiplying deltaRotation by Vector3.forward, the x value properly represents rotation
         * around the y axis. */
        trickTrack += new Vector3(Mathf.Abs((deltaRotation * Vector3.up).x), Mathf.Abs((deltaRotation * Vector3.forward).x), Mathf.Abs((deltaRotation * Vector3.up).z));

        holdoverRotation = transform.rotation;
    }

    private void FinishTrick()
    {
        if (input.downed == true)
        {
            trickTrack = Vector3.zero;
        }
        else
        {
            float[] trickScore = new float[3];

            trickScore[0] = trickTrack.x;
            trickScore[1] = trickTrack.y;
            trickScore[2] = trickTrack.z;

            //Debug.Log(trickScore[0] + ", " + trickScore[1] + ", " + trickScore[2]);

            for (int i = 0; i < 3; i++)
            {
                //Debug.Log(trickScore[i] / (2 * Mathf.PI));
                if (trickScore[i] / (2 * Mathf.PI) >= 2)
                {
                    float removeMult = (trickScore[i] / (2 * Mathf.PI)) - 1;
                    float scoreRemove = Mathf.Pow(2, (-0.2f * removeMult) +2f ) - 4;
                    //Debug.Log(scoreRemove);
                    trickScore[i] = trickScore[i] + (scoreRemove * (2 * Mathf.PI));
                }
            }

            //Debug.Log(trickScore[0] + ", " + trickScore[1] + ", " + trickScore[2] + "\n");

            boostScript.RefillBoost(trickScore[0] + trickScore[1] + trickScore[2]);
            trickTrack = Vector3.zero;
            if (trickScore[0] + trickScore[1] + trickScore[2] > 5)
                landTrickAudio.Play();
        }
    }


    private void BikeTurn()
    {

        Vector2 inputVal = new Vector2(input.steer.x, input.flip.y);
        

        if (input.grounded > 1)
        {
            //constantF.enabled = false;
            rb.drag = origDrag;
            rb.angularDrag = origAngDrag;

            //turn the car
            this.transform.Rotate(Vector3.up, turnSpeed * input.steer.x * Time.fixedDeltaTime);
            //this.transform.Rotate(Vector3.right, wheelieSpeed * input.flip.y * Time.fixedDeltaTime);
        }
        else
        {
            //constantF.enabled = true;
            rb.drag = 0.22f;
            rb.angularDrag = 3f;

            AirFrameStabilize();

            if (input.roll > 0)
            {

                Vector3 torque = new Vector3(inputVal.y, 0, inputVal.x);
               

                rb.angularVelocity += (transform.rotation * torque) * airTurnMult;

                //this.transform.Rotate(Vector3.forward, airTurnSpeed * input.steer.x * Time.fixedDeltaTime);
                //this.transform.Rotate(Vector3.right, airFlipSpeed * input.flip.y * Time.fixedDeltaTime);
            }
            else
            {
                if (input.downed == false)
                {
                    Vector3 torque = new Vector3(inputVal.y, inputVal.x, 0);
                 

                    rb.angularVelocity += transform.rotation * torque * airTurnMult;
                }
                
                //this.transform.Rotate(Vector3.up, airTurnSpeed * input.steer.x * Time.fixedDeltaTime);
                //this.transform.Rotate(Vector3.right, airFlipSpeed * input.flip.y * Time.fixedDeltaTime);
            }

            SnapToWall();

        }

        //turn the front wheels
        AxelF.transform.localRotation = initialRotationAxel * Quaternion.AngleAxis(45f * input.steer.x, Vector3.forward);
        HandleBar.transform.localRotation = initialRotationHB * Quaternion.AngleAxis(25f * input.steer.x, Vector3.forward);
        //WheelF.transform.localRotation = initialRotationWheelF * Quaternion.AngleAxis(45f * input.steer.x, Vector3.right);
    }
    private void BikeMove()
    {
        if (input.grounded > 0 && !input.downed)
        {
            //gas
            rb.AddRelativeForce(Vector3.forward * Movespeed * input.gas * Time.fixedDeltaTime, ForceMode.VelocityChange);


            //reverse
            rb.AddRelativeForce(Vector3.forward * (-Movespeed / 2) * input.reverse * Time.fixedDeltaTime, ForceMode.VelocityChange);

            //brake
            
            //rb.drag = origDrag + (input.brake * brakeStrength * Time.deltaTime);
        }
        else
        {
            //rb.drag = origDrag;
        }
    }

    private void BikeJump()
    {
        if (input.grounded > 1)
        {
            //jump
            rb.AddForce(transform.rotation * (Vector3.up * jumpStrength * input.jump));
        }
    }

    public void BikeBrake()
    {
        if (input.grounded == 0)
            return;

        if (rb.velocity.magnitude > .01f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, input.brake * brakeStrength * Time.deltaTime);
        }
    }

    private void SpinWheels()
    {
        // convert linear speed to anglular speed. spin the wheel that much.
        float velocity = rb.velocity.magnitude;
        float direction = Vector3.Dot(rb.velocity, rb.transform.forward) > 0 ? 1f : -1f;
        WheelF.transform.Rotate(Vector3.forward * direction, ((velocity / FR.radius) * Mathf.Rad2Deg) * Time.fixedDeltaTime);
        WheelB.transform.Rotate(Vector3.forward * direction, ((velocity / BL.radius) * Mathf.Rad2Deg) * Time.fixedDeltaTime);
    }


    private bool wasInAirChangedThisFrame = false;
    [SerializeField] private float groundedSpherecastRadius = 0.25f;
    private void BikeGrounded()
    {

        //Purpose:
        //  check each wheel contact with ground
        //  'tire' trail emitting = wheel contact with ground
        
        wasInAirChangedThisFrame = false;
        input.grounded = 0;
        RaycastHit hit;

        if (Physics.SphereCast(FR.anchor.transform.position, groundedSpherecastRadius, -this.transform.up, out hit, FR.groundedMaxDistance) == true)
        {
            input.grounded++;
            //Debug.Log("groundedFR");
            FR.tire.emitting = true;
            FR.airTire.emitting = false;
            /*if (!wasOnGround)
                FinishTrick();*/
            //wasOnGround = true;
        }
        else
        {
            FR.tire.emitting = false;
            FR.airTire.emitting = true;
        }

        if (Physics.SphereCast(BL.anchor.transform.position, groundedSpherecastRadius, -this.transform.up, out hit, BL.groundedMaxDistance) == true)
        {
            input.grounded++;
            //Debug.Log("groundedBL");
            BL.tire.emitting = true;
            BL.airTire.emitting = false;
            /*if (!wasOnGround)
                FinishTrick();*/
            //wasOnGround = true;
        }
        else
        {
            BL.tire.emitting = false;
            BL.airTire.emitting = true;
        }



        // testing out resetting angular velocity on landing
        if (input.grounded == 2)
        {
            if (!wasOnGround)
            {
                FinishTrick();
                rb.angularVelocity = Vector3.zero;
            }
            bikeDownFrameCount = 0;
            input.downed = false;
        }

        if (input.grounded == 0 && wasOnGround)
        {
            snapCooldown = snapCooldownTime;
            wasOnGround = false;
            holdoverRotation = transform.rotation;
        }
        else if (input.grounded == 0)
        {
            if (rb.velocity.y < 0 && !input.downed)
                rb.AddForce(new Vector3(0f, Time.deltaTime * -extraGravity, 0f), ForceMode.Acceleration);
        }
            
    }

    [SerializeField] private int framesUntilBikeDown = 30;
    private int bikeDownFrameCount = 0;
    private void OnCollisionStay(Collision col)
    {

        foreach(ContactPoint contact in col.contacts)
        {
            //Debug.Log(contact.thisCollider);
            if(contact.thisCollider == bikeDownCol)
            {
                if (col.collider.CompareTag("Ground"))
                {
                    bikeDownFrameCount++;

                    //Debug.Log(bikeDownFrameCount);

                    if (bikeDownFrameCount >= framesUntilBikeDown)
                        input.downed = true;                   
                }
                /*else
                {
                    input.downed = false;
                }*/
            }
            
        }

    }

    private void BikeDowned() 
    {


       
    }

    /*private IEnumerator WaitForTurn(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        rb.angularDrag = 0f;
    }*/

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(FR.anchor.transform.position, groundedSpherecastRadius);
        Gizmos.DrawWireSphere(FR.groundRaycast.transform.position, groundedSpherecastRadius);
        Gizmos.DrawWireSphere(BL.anchor.transform.position, groundedSpherecastRadius);
        Gizmos.DrawWireSphere(BL.groundRaycast.transform.position, groundedSpherecastRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(FR.anchor.transform.position, spherecastRadius);
        Gizmos.DrawWireSphere(FR.raycast.transform.position, spherecastRadius);
        Gizmos.DrawWireSphere(BL.anchor.transform.position, spherecastRadius);
        Gizmos.DrawWireSphere(BL.raycast.transform.position, spherecastRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(wallRaycastPosition.transform.position + transform.right * wallRaycastLength, wallRaycastPosition.transform.position - transform.right * wallRaycastLength);
    }
    


}
