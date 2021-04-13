using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableObject : MonoBehaviour, Rotatable, Connectable
{
    public GameObject rotatingPart;
    private List<Rotatable> connectedObjects = new List<Rotatable>();

    private float angularVelocity;
    private bool rotationActive = false;

    // Update is called once per frame
    void Update()
    {
        if (rotationActive)
        {
            rotatingPart.transform.RotateAround(rotatingPart.transform.position, rotatingPart.transform.up, this.angularVelocity * Time.deltaTime);
        }
    }

    public void addConnectedPart(Rotatable part)
    {
        connectedObjects.Add(part);
    }

    public void rotate(float angularVelocity) {
        this.angularVelocity = angularVelocity;
        this.rotationActive = true;

        foreach (Rotatable connectedObject in connectedObjects)
        {
            connectedObject.rotate(this.angularVelocity);
        }
    }

    public void stopRotation() {
        this.angularVelocity = 0.0f;
        this.rotationActive = false;

        foreach (Rotatable connectedObject in connectedObjects)
        {
            connectedObject.stopRotation();
        }
    }
}
