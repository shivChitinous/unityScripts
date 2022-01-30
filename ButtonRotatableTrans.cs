using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonRotatableTrans : MonoBehaviour
{
    float deltaRotation = 0.6f;
    float deltamove = 0.005f;

    public float Rotation
    {
        get
        {
            return transform.localEulerAngles.y;
        }
        set
        {
            float delta = value - transform.localEulerAngles.y;
            transform.Rotate(0, delta, 0);
        }
    }

    public void RotateLeft(float sensitivity = 1.0f)
    {
        transform.Rotate(0, sensitivity * deltaRotation, 0);
    }

    public void RotateRight(float sensitivity = 1.0f)
    {
        transform.Rotate(0, sensitivity * -deltaRotation, 0);
    }

    public void moveForward(float sensitivity = 1.0f)
    {
        transform.Translate(deltamove * sensitivity * new Vector3(1,0,0));
    }

    // Update is called once per frame.
    private void Update()
    {
        // These arrow-key controls really only work in "Game" mode.

        if (Input.GetKey("left"))
        {
            RotateLeft();
        }
        if (Input.GetKey("right"))
        {
            RotateRight();
        }

        if (Input.GetKey("up"))
        {
            moveForward();
        }

    }
}
