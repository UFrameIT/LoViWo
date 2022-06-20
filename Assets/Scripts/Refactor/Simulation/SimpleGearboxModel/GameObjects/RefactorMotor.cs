using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefactorMotor : MonoBehaviour
{
    private GameObject connectedShaft;
    private Fact associatedFact;
    private SimulatedObject simulatedObject;
    private float drive;

    public void addConnecedShaft(GameObject shaft)
    {
        this.connectedShaft = shaft;
    }

    public void addSimulatedObject(SimulatedObject simulatedObject)
    {
        this.simulatedObject = simulatedObject;
    }

    public GameObject getConnecedShaft()
    {
        return this.connectedShaft;
    }

    public SimulatedObject getSimulatedObject()
    {
        return this.simulatedObject;
    }

    public void addAssociatedFact(Fact associatedFact)
    {
        this.associatedFact = associatedFact;
    }

    public Fact getAssociatedFact()
    {
        return this.associatedFact;
    }

    public void setDrive(float drive)
    {
        this.drive = drive;
    }

    public float getDrive()
    {
        return this.drive;
    }

}
