using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateController : MonoBehaviour
{
    public float speed = 5;
    public float direction = 1;
    public bool isTurning;

    public void Rotate()
    {
        gameObject.transform.Rotate(Vector3.up * speed * direction * Time.deltaTime);
    }



    // Update is called once per frame
    void Update()
    {
        if (isTurning)
            Rotate();
    }
}
