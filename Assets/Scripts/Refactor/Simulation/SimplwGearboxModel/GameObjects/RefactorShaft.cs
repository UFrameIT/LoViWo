using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefactorShaft : MonoBehaviour, Rotatable, Connectable
{
    private List<Rotatable> connectedRotatables = new List<Rotatable>();
    private List<GameObject> connectedObjects = new List<GameObject>();
    private Fact associatedFact;

    private float angularVelocity;
    private bool rotationActive = false;

    // Update is called once per frame
    void Update()
    {
        if (rotationActive)
        {
            this.transform.RotateAround(this.transform.position, this.transform.up, this.angularVelocity * Time.deltaTime);
        }
    }

    public void addConnectedPart(Rotatable part)
    {
        connectedRotatables.Add(part);
    }

    public List<Rotatable> getConnectedParts()
    {
        return this.connectedRotatables;
    }

    public void addConnectedObject(GameObject part)
    {
        connectedObjects.Add(part);
    }

    public List<GameObject> getConnectedObjects()
    {
        return this.connectedObjects;
    }

    public Transform getRootTransform()
    {
        return this.transform.root;
    }

    public void addAssociatedFact(Fact fact)
    {
        this.associatedFact = fact;
    }
    public Fact getAssociatedFact()
    {
        return this.associatedFact;
    }


    public void rotate(float angularVelocity, bool knowledgeBased)
    {
        this.angularVelocity = angularVelocity;
        this.rotationActive = true;

        foreach (Rotatable connectedObject in connectedRotatables)
        {
            connectedObject.rotate(this.angularVelocity, knowledgeBased);
        }
    }

    public void stopRotation()
    {
        this.angularVelocity = 0.0f;
        this.rotationActive = false;

        foreach (Rotatable connectedObject in connectedRotatables)
        {
            connectedObject.stopRotation();
        }
    }
}
