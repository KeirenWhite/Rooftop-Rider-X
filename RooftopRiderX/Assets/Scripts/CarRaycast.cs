using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


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
        public float brake;
        public int grounded;
        public float roll;
        public Vector2 flip;
        public int downed;
    }
    [System.Serializable]
    public struct WheelInfo
    {
        public GameObject anchor;
        public GameObject wheel;
        public TrailRenderer tire;
        public GameObject raycast;      //how far down to ground to raycast        
        //public float springStrength;    //strong enough to hold up car (ie: ~180,000 for a 1000 lbs car)      
        //public float dampingFactor;     //stop the car from being too bouncy (boat 0.0f, car 0.3f)
        [HideInInspector]
        public float maxDistance;       //calculated in start (anchor - raycastTo position)        
        [HideInInspector]
        public float radius;            //calculated in start (mesh bounds)
        [HideInInspector]
        public Vector3 springForce;


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
    [SerializeField] private float airStabilizationSensitivityX = 10f;
    public float airTurnSpeed = 1.5f;
    public float airFlipSpeed = 2f;
    //public float wheelieSpeed = 100f;
    public float Movespeed = 35;
    public float Turnspeed = 90;
    public float BrakeStrength = 5;
    public WheelInfo FR = WheelInfo.CreateDefault();
    public WheelInfo BL = WheelInfo.CreateDefault();

    private Rigidbody rb = null;
    private float origDrag;
    private InputInfo input;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
       
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
        BikeMove();
        
        //SpinWheels();
    }

    void FrameStabilize()
    {
        RaycastHit hit;
        

        if (Physics.Raycast(transform.position, -this.transform.up, out hit))
        {
            float groundNormal = hit.normal.y;
           
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.MoveTowardsAngle(transform.eulerAngles.z, groundNormal, Time.deltaTime * stabilizationSensitivity));
            Debug.Log("stabilizing");
        }


        Debug.DrawRay(transform.position, -this.transform.up, Color.blue);
    }

    void AirFrameStabilize()
    {
        RaycastHit hit;


        if (Physics.Raycast(transform.position, -this.transform.up, out hit))
        {
            float groundNormal = hit.normal.y;

            transform.eulerAngles = new Vector3(Mathf.MoveTowardsAngle(transform.eulerAngles.x, 0, Time.deltaTime * airStabilizationSensitivityX), transform.eulerAngles.y, Mathf.MoveTowardsAngle(transform.eulerAngles.z, 0, Time.deltaTime * airStabilizationSensitivityZ));
            Debug.Log("stabilizing");
        }


        Debug.DrawRay(transform.position, -this.transform.up, Color.blue);
    }

    private void OnGUI()
    {
        
        //this is just to display debug text on screen
        GUIStyle style = new GUIStyle();
        style.fontSize = 28;
        style.normal.textColor = Color.black;
        GUI.Label(new Rect(10, 10, 300, 50), string.Format("steer {0}, gas {1}, brake {2}", input.steer.x, input.gas, input.brake), style);
        GUI.Label(new Rect(10, 30, 300, 50), string.Format("grounded {0}", input.grounded), style);
        GUI.Label(new Rect(10, 50, 300, 50), string.Format("suspension FR {0}", FR.springForce), style);
        GUI.Label(new Rect(10, 70, 300, 50), string.Format("suspension BL {0}", BL.springForce), style);
    }
    private void OnSteer(InputValue value)
    {
        input.steer = value.Get<Vector2>();
    }
    private void OnGas(InputValue value)
    {
        input.gas = value.Get<float>();
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

    private void OnReset(InputValue value)
    {
        //upright car
        this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);
    }
    private void BikeTurn()
    {
        if (input.grounded > 0)
        {
            FrameStabilize();

            //turn the car
            this.transform.Rotate(Vector3.up, Turnspeed * input.steer.x * Time.fixedDeltaTime);
            //this.transform.Rotate(Vector3.right, wheelieSpeed * input.flip.y * Time.fixedDeltaTime);
        }
        else
        {
            AirFrameStabilize();
            if (input.roll > 0)
            {
                this.transform.Rotate(Vector3.forward, airTurnSpeed * input.steer.x * Time.fixedDeltaTime);
                this.transform.Rotate(Vector3.right, airFlipSpeed * input.flip.y * Time.fixedDeltaTime);
            }
            else
            {
                this.transform.Rotate(Vector3.up, airTurnSpeed * input.steer.x * Time.fixedDeltaTime);
                this.transform.Rotate(Vector3.right, airFlipSpeed * input.flip.y * Time.fixedDeltaTime);
            }
        }
        //turn the front wheels
        //FR.anchor.transform.localRotation = Quaternion.AngleAxis(45f * input.steer.x, Vector3.up);
    }
    private void BikeMove()
    {
        if (input.grounded > 0)
        {
            //gas
            rb.AddRelativeForce(Vector3.forward * Movespeed * input.gas * Time.fixedDeltaTime, ForceMode.VelocityChange);
            //brake
            //rb.linearDamping = origDrag + (BrakeStrength * input.brake * Time.fixedDeltaTime);
        }
    }
    
    /*private void SpinWheels()
    {
        // convert linear speed to anglular speed. spin the wheel that much.
        float velocity = rb.linearVelocity.magnitude;
        float direction = Vector3.Dot(rb.linearVelocity, rb.transform.forward) > 0 ? 1f : -1f;
        FR.wheel.transform.Rotate(Vector3.right * direction, ((velocity / FR.radius) * Mathf.Rad2Deg) * Time.fixedDeltaTime);
        FL.wheel.transform.Rotate(Vector3.right * direction, ((velocity / FL.radius) * Mathf.Rad2Deg) * Time.fixedDeltaTime);
        BR.wheel.transform.Rotate(Vector3.right * direction, ((velocity / BR.radius) * Mathf.Rad2Deg) * Time.fixedDeltaTime);
        BL.wheel.transform.Rotate(Vector3.right * direction, ((velocity / BL.radius) * Mathf.Rad2Deg) * Time.fixedDeltaTime);
    }*/
    private void BikeGrounded()
    {
        //Purpose:
        //  check each wheel contact with ground
        //  'tire' trail emitting = wheel contact with ground

        input.grounded = 0;

        if (Physics.Raycast(FR.anchor.transform.position, -this.transform.up, FR.maxDistance) == true)
        {
            input.grounded++;
            //FR.tire.emitting = true;
        }
        else
        {
            //FR.tire.emitting = false;
        }

        

        if (Physics.Raycast(BL.anchor.transform.position, -this.transform.up, BL.maxDistance) == true)
        {
            input.grounded++;
            //BL.tire.emitting = true;
        }
        else
        {
            //BL.tire.emitting = false;         
        }

        

        

        
    }


    
    private void BikeDowned() 
    {
        input.downed = 0;

        

        if (Physics.Raycast(FR.anchor.transform.position, -this.transform.up / 1.34f) == true) //check if bike is on its side
        {
            Debug.DrawRay(FR.anchor.transform.position, -this.transform.up / 1.34f, Color.red);
            input.downed++;
        }
   
        if (Physics.Raycast(BL.anchor.transform.position, -this.transform.up / 1.34f) == true) //check if bike is on its side
        {
            Debug.DrawRay(BL.anchor.transform.position, -this.transform.up / 1.34f, Color.red);
            input.downed++;
        }

        
    }

    /*private void OnDrawGizmosSelected()
    {
        if (Physics.SphereCast(FR.anchor.transform.position, .5439239f, transform.forward, out hit) == true)
        {
            Gizmos.DrawWireSphere(SphereCase(), 0.5439239f);
        }
    }
    private Vector3 SphereCase()
    {
        Gizmos.color = Color.yellow;
        Vector3 midPoint = new Vector3();
        Ray r = new Ray(transform.position, transform.forward);
        Vector3 a = transform.position;
        Vector3 b = hit.point;
        Vector3 c = r.GetPoint(hit.distance - .5439239f);

        float v1 = Vector3.Dot((c - a), (c - a));
        float v2 = Vector3.Dot((b - a), (c - a));
        float t = v2 / v1;

        midPoint = (a + t * (c - a));
        return midPoint;
    }*/


}
