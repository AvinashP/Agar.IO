using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgarObject))]
public class MouseController : MonoBehaviour
{
    AgarObject agarObject;

    void Awake()
    {
        agarObject = GetComponent<AgarObject>();
    }

    void Update()
    {
        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePoint.z = 0;

        agarObject.SetMovePosition(mousePoint);
    }
}
