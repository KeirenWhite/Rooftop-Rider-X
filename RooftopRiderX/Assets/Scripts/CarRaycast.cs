using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

/* Hierarchy of Car raycast gameobject parts:
 
    Car                 script (rigidbody 1000lbs, collider, playerinput)
        moveto          empty (for camera to follow)
        anchorFR        empty (for steer wheel angles)
            wheelFR     mesh (spherecollider)
            tireFR      trail renderer (position to bottom of wheel)
            raycastFR   how far to raycast for suspension
        anchorFL    
            wheelFL
            tireFL
            raycastFL
        anchorBR  
            wheelBR
            tireBR
            raycastBR
        anchorBL 
            wheelBL 
            tireBL
            raycastBL
*/

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]   //remember: set mass=1000, drag=1, andgulardrag=5
[RequireComponent(typeof(PlayerInput))] //remember: set the component's inputaction asset
public class CarRaycast : MonoBehaviour
{
    private IEnumerator enumerator;
    public struct InputInfo
    {
        public Vector2 steer;
        public float gas;
        public float brake;
        public int grounded;
        public float slip;
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
        public float springStrength;    //strong enough to hold up car (ie: ~180,000 for a 1000 lbs car)      
        public float dampingFactor;     //stop the car from being too bouncy (boat 0.0f, car 0.3f)
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
                springStrength = 280000,
                dampingFactor = 0.1f
            };            
        }
    }

    [SerializeField] private float stabilizationSensitivity = 60f;
    public float airTurnSpeed = 1.5f;
    public float airFlipSpeed = 2f;
    public float turnMulti = 2f;
    public float Movespeed = 35;
    public float Turnspeed = 90;
    public float BrakeStrength = 5;
    public WheelInfo FR = WheelInfo.CreateDefault();
    public WheelInfo BL = WheelInfo.CreateDefault();
    public bool isCarOnMenu;

    private Rigidbody rb = null;
    private float origDrag;
    private InputInfo input;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //origDrag = rb.linearDamping;
        isCarOnMenu = (gameObject.scene.name == "Menu");

        //size of each wheel (used to spin wheels)
        FR.radius = FR.wheel.GetComponent<Renderer>().bounds.extents.y;
        BL.radius = BL.wheel.GetComponent<Renderer>().bounds.extents.y;

        //distance to raycast to ground
        FR.maxDistance = FR.anchor.transform.position.y - FR.raycast.transform.position.y;
        BL.maxDistance = BL.anchor.transform.position.y - BL.raycast.transform.position.y;
    }
    private void FixedUpdate()
    {
        CarGrounded();
        BikeDowned();
        FrameStabilize();


        if (isCarOnMenu == false)
        {
            CarTurn();
            CarMove();
        }       

        CarSuspension(ref FR);
        CarSuspension(ref BL);
        
        //SpinWheels();
    }

    void FrameStabilize()
    {
        RaycastHit hit;
        

        if (Physics.Raycast(transform.position, -this.transform.up, out hit))
        {
            Vector3 groundNormal = hit.normal;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.MoveTowardsAngle(groundNormal.z, 0, Time.deltaTime * stabilizationSensitivity));
            Debug.Log("stabilizing");
        }


        Debug.DrawRay(transform.position, -this.transform.up, Color.blue);
    }

    private void OnGUI()
    {
        if (isCarOnMenu == true)
            return;
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

    private void OnSlip(InputValue value)
    {
        input.slip = value.Get<float>();
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
    private void CarTurn()
    {
        if (input.grounded > 0)
        {
            //turn the car
            this.transform.Rotate(Vector3.up, Turnspeed * input.steer.x * Time.fixedDeltaTime);
        }
        else
        {
            if (input.slip > 0 && input.downed == 0)
            {
                this.transform.Rotate(Vector3.up, airTurnSpeed * input.steer.x * turnMulti * Time.fixedDeltaTime);
                this.transform.Rotate(Vector3.right, airFlipSpeed * input.flip.y * turnMulti * Time.fixedDeltaTime);
            }
            else if (input.downed == 0)
            {
                this.transform.Rotate(Vector3.up, airTurnSpeed * input.steer.x * Time.fixedDeltaTime);
                this.transform.Rotate(Vector3.right, airFlipSpeed * input.flip.y * Time.fixedDeltaTime);
            }
        }
        //turn the front wheels
        //FR.anchor.transform.localRotation = Quaternion.AngleAxis(45f * input.steer.x, Vector3.up);
    }
    private void CarMove()
    {
        if (input.grounded > 0)
        {
            //gas
            rb.AddRelativeForce(Vector3.forward * Movespeed * input.gas * Time.fixedDeltaTime, ForceMode.VelocityChange);
            //brake
            //rb.linearDamping = origDrag + (BrakeStrength * input.brake * Time.fixedDeltaTime);
        }
    }
    private void CarSuspension(ref WheelInfo w)
    {
        //Purpose: calculate the force up to apply to suspend the car at a wheel
        RaycastHit hit;
        if (Physics.Raycast(w.anchor.transform.position, -this.transform.up, out hit, w.maxDistance) == true)
        {
            // GetPointVelocity = what is the rigidbody velocity at anchor point (re: rocking car, independent suspensioin at each wheel)
            Vector3 pointVelocity = rb.GetPointVelocity(w.anchor.transform.position);
            // Dot = force depends on current angle to ground at anchor point (re: better rocking)
            float damping = w.dampingFactor * Vector3.Dot(pointVelocity, w.anchor.transform.up);            
            // how strong the spring pushes depends on the extension of the spring
            w.springForce = this.transform.up * w.springStrength * Time.fixedDeltaTime * (w.maxDistance - hit.distance * w.radius) / w.maxDistance;            
            // force the car up at wheel position
            rb.AddForceAtPosition(w.springForce * damping, w.anchor.transform.position, ForceMode.Force);
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
    private void CarGrounded()
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

        if (Physics.Raycast(FR.anchor.transform.position, -this.transform.right / 2, 100f) == true) //check if bike is on its side
        {
            Debug.DrawRay(FR.anchor.transform.position, -this.transform.right / 2, Color.red);
            input.downed++;
        }
        if (Physics.Raycast(FR.anchor.transform.position, this.transform.right / 2, 100f) == true) //check if bike is on its side
        {
            Debug.DrawRay(FR.anchor.transform.position, this.transform.right / 2, Color.red);
            input.downed++;
        }
        if (Physics.Raycast(BL.anchor.transform.position, -this.transform.right / 2, 100f) == true) //check if bike is on its side
        {
            Debug.DrawRay(BL.anchor.transform.position, -this.transform.right / 2, Color.red);
            input.downed++;
        }
        if (Physics.Raycast(BL.anchor.transform.position, this.transform.right / 2, 100f) == true) //check if bike is on its side
        {
            Debug.DrawRay(BL.anchor.transform.position, this.transform.right / 2, Color.red);
            input.downed++;
        }

    }

    
}
