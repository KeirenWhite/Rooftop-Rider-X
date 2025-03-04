using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public GameObject moveto = null;
    public GameObject lookat = null;
    public GameObject farmoveto = null;
    public float Movespeed = 2;
    public float reverseSpeedMult = 2;
    public float Lookspeed = 120;
    private float RecallCameraY = 0; //this is so the camera doesn't rollover with the car

    [SerializeField] private float offsetAmount = 10f;
    [SerializeField] private float reachAroundAmount = 0.5f;

    public CarRaycast bike = null;

    private Vector2 steer;

    private void Start()
    {
        //the RecallCameraY is so the camera doesn't flip with the car and go under the ground by accident
        if (moveto != null)
            RecallCameraY = Mathf.Abs(moveto.transform.position.y - lookat.transform.position.y);
    }

    private void OnCamSteer(InputValue value)
    {
        steer = value.Get<Vector2>();
        Debug.Log(steer);
    }

    private void LateUpdate()
    {
        //the new position, but keep the camera at original y so it doesn't flip with the car
        if (farmoveto != null && bike.input.reverse > 0 && bike.input.grounded > 0)
        {
            float moveTargetY;
            Vector3 cameraMovePos;
            if (lookat != null)
            {
                moveTargetY = lookat.transform.position.y + RecallCameraY;
                cameraMovePos = new Vector3(farmoveto.transform.position.x, moveTargetY, farmoveto.transform.position.z);
                this.transform.position = Vector3.Lerp(this.transform.position, cameraMovePos, (Movespeed * reverseSpeedMult) * Time.deltaTime);
            }
            else
            {
                this.transform.position = Vector3.Lerp(this.transform.position, moveto.transform.position, Movespeed * Time.deltaTime);
            }
        }
        else if (moveto != null)
        {
            float moveTargetY;
            Vector3 cameraMovePos;
            if (lookat != null)
            {
                float airReachAroundReduction = bike.input.grounded == 0 ? -1f : 1f;

                Vector3 camMoveOffset = new Vector3(steer.x, steer.y, 0f);
                camMoveOffset.z = new Vector2(steer.x, steer.y).magnitude * (reachAroundAmount * airReachAroundReduction);
                camMoveOffset *= offsetAmount;
                camMoveOffset = Quaternion.AngleAxis(bike.transform.eulerAngles.y, Vector3.up) * camMoveOffset;

                Vector3 pushAwayIfClose = Vector3.zero;
                if (bike.rb.velocity.magnitude < 10f)
                {
                    pushAwayIfClose = Quaternion.AngleAxis(bike.transform.eulerAngles.y, Vector3.up) * (Vector3.back * 3);
                    if (bike.input.grounded == 0)
                    {
                        pushAwayIfClose = Quaternion.AngleAxis(bike.transform.eulerAngles.y, Vector3.up) * (Vector3.back * 10);
                    }
                }
                moveTargetY = lookat.transform.position.y + RecallCameraY;
                cameraMovePos = new Vector3(moveto.transform.position.x, moveTargetY, moveto.transform.position.z) + camMoveOffset;
                cameraMovePos += pushAwayIfClose;

                this.transform.position = Vector3.Lerp(this.transform.position, cameraMovePos, Movespeed * Time.deltaTime);
            }
            else
            {
                this.transform.position = Vector3.Lerp(this.transform.position, moveto.transform.position, Movespeed * Time.deltaTime);
            }
        }
        

        //rotation
        if (lookat != null)
        {
            Quaternion rotTarget = Quaternion.LookRotation(lookat.transform.position - this.transform.position);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotTarget, Lookspeed * Time.deltaTime);
        }       

    }
}
