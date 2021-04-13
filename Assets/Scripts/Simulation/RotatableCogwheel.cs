using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableCogwheel : MonoBehaviour, Rotatable, Interlockable
{
    public GameObject rotatingPart;
    private List<Interlockable> interlockingObjects = new List<Interlockable>();

    private float angularVelocity;
    private bool rotationActive = false;
    private Vector3 positionBeforeSimulation;
    private Quaternion rotationBeforeSimulation;

    // Update is called once per frame
    void Update()
    {
        if (rotationActive)
        {
            rotatingPart.transform.RotateAround(rotatingPart.transform.position, rotatingPart.transform.up, this.angularVelocity * Time.deltaTime);
        }
    }

    public void addInterlockingPart(Interlockable part)
    {
        interlockingObjects.Add(part);
    }

    public void activatePhysics()
    {
        saveTransform();
        if (rotatingPart.transform.GetComponentInChildren<Rigidbody>() != null)
            rotatingPart.transform.GetComponentInChildren<Rigidbody>().isKinematic = false;
    }

    public void saveTransform()
    {
        this.positionBeforeSimulation = this.rotatingPart.transform.localPosition;
        this.rotationBeforeSimulation = this.rotatingPart.transform.localRotation;
    }

    public void deactivatePhysics()
    {
        if (rotatingPart.transform.GetComponentInChildren<Rigidbody>() != null)
            rotatingPart.transform.GetComponentInChildren<Rigidbody>().isKinematic = true;

        restoreTransform();
    }

    public void restoreTransform()
    {
        this.rotatingPart.transform.localPosition = this.positionBeforeSimulation;
        this.rotatingPart.transform.localRotation = this.rotationBeforeSimulation;
    }

    public void rotate(float angularVelocity) {
        this.angularVelocity = angularVelocity;
        this.rotationActive = true;
        saveTransform();

        foreach (Interlockable interlockingObject in interlockingObjects) {
            interlockingObject.activatePhysics();
        }
    }

    public void stopRotation() {
        this.angularVelocity = 0.0f;
        this.rotationActive = false;
        restoreTransform();

        foreach (Interlockable interlockingObject in interlockingObjects)
        {
            interlockingObject.deactivatePhysics();
        }
    }
}
