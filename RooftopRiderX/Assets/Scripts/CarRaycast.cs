using System;
using System.Collections;
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
    private IEnumerator enumerator;
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
        [HideInInspector]
        public float maxDistance;       //calculated in start (anchor - raycastTo position)        
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
    [SerializeField] private Collider bikeBody;
    private Rigidbody rb = null;
    private float origDrag;
    private float origAngDrag;
    [HideInInspector] public InputInfo input;
    private Quaternion initialRotationAxel;
    private Quaternion initialRotationWheelF;
    private Quaternion initialRotationHB;
    private float maxRot = 45f;

    private bool wasOnGround = false;
    private Quaternion holdoverRotation;
    private Quaternion deltaRotation;
    private Vector3 trickTrack;
    [SerializeField] private Boost boostScript;

    private bool wasInAir = true;

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
    }
    private void FixedUpdate()
    {
        BikeGrounded();
        BikeDowned();
        
        //FrameStabilize();

        BikeTurn();
        BikeJump();
        BikeMove();

        Trick();

        SpinWheels();

        Debug.Log("speed: " + rb.velocity.magnitude);
    }

    void FrameStabilize()
    {
        RaycastHit hit;
        

        if (Physics.SphereCast(FR.anchor.transform.position, .25f, -this.transform.up, out hit, FR.maxDistance) == true || Physics.SphereCast(BL.anchor.transform.position, .25f, -this.transform.up, out hit, BL.maxDistance) == true)
        {
            float groundNormal = hit.normal.y;
           
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.MoveTowardsAngle(transform.eulerAngles.z, groundNormal, Time.deltaTime * stabilizationSensitivity));
            //Debug.Log("stabilizing"); 
        }


        Debug.DrawRay(transform.position, -this.transform.up, Color.blue);
    }

    void AirFrameStabilize()
    {
        RaycastHit hit;


        if (Physics.Raycast(transform.position, -this.transform.up, out hit))
        {
            float groundNormal = hit.normal.y;

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.MoveTowardsAngle(transform.eulerAngles.z, 0, Time.deltaTime * airStabilizationSensitivityZ));
            //Debug.Log("stabilizing");
        }


        Debug.DrawRay(transform.position, -this.transform.up, Color.blue);
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
            this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z);
        }
        //input.reset = value.Get<float>();
    }

    private void Trick()
    {
        if (input.grounded > 0)
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
        boostScript.RefillBoost(trickTrack.x + trickTrack.y + trickTrack.z);
        trickTrack = Vector3.zero;
    }


    private void BikeTurn()
    {

        Vector2 inputVal = new Vector2(input.steer.x, input.flip.y);

        if (input.grounded > 1)
        {
            //rb.drag = origDrag;
            //rb.angularDrag = origAngDrag;
            FrameStabilize();

            //turn the car
            this.transform.Rotate(Vector3.up, turnSpeed * input.steer.x * Time.fixedDeltaTime);
            //this.transform.Rotate(Vector3.right, wheelieSpeed * input.flip.y * Time.fixedDeltaTime);
        }
        else
        {
            //rb.drag = 0.3f;
            //rb.angularDrag = 1f;
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
                Vector3 torque = new Vector3(inputVal.y, inputVal.x, 0);

                rb.angularVelocity += transform.rotation * torque * airTurnMult;

                //this.transform.Rotate(Vector3.up, airTurnSpeed * input.steer.x * Time.fixedDeltaTime);
                //this.transform.Rotate(Vector3.right, airFlipSpeed * input.flip.y * Time.fixedDeltaTime);
            }

        }
        //turn the front wheels
        AxelF.transform.localRotation = initialRotationAxel * Quaternion.AngleAxis(45f * input.steer.x, Vector3.forward);
        HandleBar.transform.localRotation = initialRotationHB * Quaternion.AngleAxis(25f * input.steer.x, Vector3.forward);
        //WheelF.transform.localRotation = initialRotationWheelF * Quaternion.AngleAxis(45f * input.steer.x, Vector3.right);
    }
    private void BikeMove()
    {
        if (input.grounded > 0)
        {
            //gas
            rb.AddRelativeForce(Vector3.forward * Movespeed * input.gas * Time.fixedDeltaTime, ForceMode.VelocityChange);


            //reverse
            //rb.AddRelativeForce(Vector3.forward * (-Movespeed / 2) * input.reverse * Time.fixedDeltaTime, ForceMode.VelocityChange);

            //brake
            rb.drag = origDrag + (input.brake * brakeStrength * Time.deltaTime);
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
            rb.AddForce(Vector3.up * jumpStrength * input.jump);
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
    private void BikeGrounded()
    {

        //Purpose:
        //  check each wheel contact with ground
        //  'tire' trail emitting = wheel contact with ground

        input.grounded = 0;
        RaycastHit hit;

        if (Physics.SphereCast(FR.anchor.transform.position, .25f, -this.transform.up, out hit, FR.maxDistance) == true)
        {
            input.grounded++;
            //Debug.Log("groundedFR");
            FR.tire.emitting = true;
            FR.airTire.emitting = false;
            if (!wasOnGround)
                FinishTrick();
            wasOnGround = true;
        }
        else
        {
            FR.tire.emitting = false;
            FR.airTire.emitting = true;
        }

        if (Physics.SphereCast(BL.anchor.transform.position, .25f, -this.transform.up, out hit, BL.maxDistance) == true)
        {
            input.grounded++;
            //Debug.Log("groundedBL");
            BL.tire.emitting = true;
            BL.airTire.emitting = false;
            if (!wasOnGround)
                FinishTrick();
            wasOnGround = true;
        }
        else
        {
            BL.tire.emitting = false;
            BL.airTire.emitting = true;
        }

        // testing out resetting angular velocity on landing
        if (input.grounded == 2)
        {
            if (wasInAir)
            {
                rb.angularVelocity = Vector3.zero;
            }
            wasInAir = false;
        }

        if (input.grounded == 0 && wasOnGround)
        {
            wasOnGround = false;
            holdoverRotation = transform.rotation;
        }
        else if (input.grounded == 0)
            wasInAir = true;
            
    }

    private void OnCollisionStay(Collision col)
    {
        foreach(ContactPoint contact in col.contacts)
        {
            //Debug.Log(contact.thisCollider);
            if(contact.thisCollider == bikeBody)
            {
                if (col.collider.CompareTag("Ground"))
                {
                    //Debug.Log("gasdf");
                    input.downed = true;
                    break;
                }

            }
            else
            {
                input.downed = false;
            }
        }

    }
    
    private void BikeDowned() 
    {


       
    }

   /* private void OnDrawGizmos()
    {
            Gizmos.DrawWireSphere(FR.anchor.transform.position, .4f);
        
    }*/
    


}
