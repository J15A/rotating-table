using NUnit.Framework;
using UnityEngine;

public class PadRotator : MonoBehaviour
{
    public bool isRotating = false;
    public float speed = 600f; // degrees per second
    private float currentRotation = 0f;

    private void Start()
    {
        isRotating = false;   
    }

    public void RotatePads()
    {
        isRotating = true;
        currentRotation = 0f;
    }

    private void Update()
    {
        if (isRotating)
        {
            float step = speed * Time.deltaTime;
            
            if (currentRotation + step >= 360f)
            {
                step = 360f - currentRotation;
                transform.Rotate(Vector3.forward, step);
                currentRotation = 0f;
                isRotating = false;
            }
            else
            {
                transform.Rotate(Vector3.forward, step);
                currentRotation += step;
            }
        }
    }
}
