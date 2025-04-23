using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ObstacleCollision : MonoBehaviour
{
    private Rigidbody rb;
    private MeshCollider col;

    [SerializeField] private float bounceVel = 1000f;

    private bool isHit = false;
    private float timer = 0f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = rb.GetComponent<MeshCollider>();

        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Bike")
        {
            timer = 0f;

            col.isTrigger = false;
            rb.useGravity = true;

            //gameObject.layer = 7;

            StartCoroutine(ResetAfterHit());

            isHit = true;
        }
    }

    private void ResetObject()
    {
        //gameObject.layer = 0;

        col.isTrigger = true;
        rb.useGravity = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPos;
        transform.rotation = startRot;
    }

    private IEnumerator ResetAfterHit()
    {
        yield return new WaitForSeconds(8f);

        // fade in/out or become transparent

        yield return new WaitForSeconds(2f);
        ResetObject();
    }

}
