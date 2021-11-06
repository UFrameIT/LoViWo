using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableCogwheel : MonoBehaviour, Rotatable, Interlockable
{
    private List<Interlockable> interlockingObjects = new List<Interlockable>();

    private float angularVelocity;
    private bool rotationActive = false;
    private Vector3 positionBeforeSimulation;
    private Quaternion rotationBeforeSimulation;

    private bool knowledgeBasedSimulation;

    // Update is called once per frame
    void Update()
    {
        if (rotationActive)
        {
            this.transform.RotateAround(this.transform.position, this.transform.up, this.angularVelocity * Time.deltaTime);
        }
    }

    public void addInterlockingPart(Interlockable part)
    {
        interlockingObjects.Add(part);
    }

    public List<Interlockable> getInterlockingParts() {
        return this.interlockingObjects;
    }

    public Transform getRootTransform() {
        return this.transform.root;
    }

    public void activatePhysics()
    {
        saveTransform();
        if (this.transform.GetComponentInChildren<Rigidbody>() != null)
            this.transform.GetComponentInChildren<Rigidbody>().isKinematic = false;

        //Recursively activatePhysics of interlockingObjects
        foreach (Interlockable interlockingObject in interlockingObjects)
        {
            interlockingObject.activatePhysics();
        }
    }

    public void saveTransform()
    {
        this.positionBeforeSimulation = this.transform.localPosition;
        this.rotationBeforeSimulation = this.transform.localRotation;
    }

    public void deactivatePhysics()
    {
        if (this.transform.GetComponentInChildren<Rigidbody>() != null)
            this.transform.GetComponentInChildren<Rigidbody>().isKinematic = true;

        //Recursively deactivatePhysics of interlockingObjects
        foreach (Interlockable interlockingObject in interlockingObjects)
        {
            interlockingObject.deactivatePhysics();
        }

        restoreTransform();
    }

    public void restoreTransform()
    {
        this.transform.localPosition = this.positionBeforeSimulation;
        this.transform.localRotation = this.rotationBeforeSimulation;
    }

    public void rotate(float angularVelocity, bool knowledgeBased) {
        this.angularVelocity = angularVelocity;
        this.knowledgeBasedSimulation = knowledgeBased;
        this.rotationActive = true;
        saveTransform();

        if (!knowledgeBased) {
            activatePhysics();
        }
    }

    public void stopRotation() {
        this.angularVelocity = 0.0f;
        this.rotationActive = false;
        restoreTransform();

        if (!knowledgeBasedSimulation) {
            deactivatePhysics();
        }
    }
}
