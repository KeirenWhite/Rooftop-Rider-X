using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPoint : MonoBehaviour
{
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject bike;
    [SerializeField] private bool disableArrow = false;
    

    private Vector3 target = Vector3.one;
        
    void Update()
    {
        Point();
    }

    private void Point()
    {
        if (target == Vector3.zero || disableArrow)
        {
            arrow.SetActive(false);
            return;
        }
        arrow.SetActive(true);

        Vector3 bikePos = new Vector3(bike.transform.position.x, bike.transform.position.y + 2, bike.transform.position.z);
        Vector3 pointDir = (target - bikePos).normalized;

        arrow.transform.position = bikePos + (pointDir);
        arrow.transform.LookAt(target);
        
    }

    public void ChangeTarget(Vector3 targetPosition)
    {
        target = targetPosition;
    }
}
