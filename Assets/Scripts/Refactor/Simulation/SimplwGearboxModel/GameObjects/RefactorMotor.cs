using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefactorMotor : MonoBehaviour
{
    private GameObject connectedShaft;
    private Fact associatedFact;

    public void addConnecedShaft(GameObject shaft)
    {
        this.connectedShaft = shaft;
    }

    public GameObject getConnecedShaft()
    {
        return this.connectedShaft;
    }

    public void addAssociatedFact(Fact associatedFact)
    {
        this.associatedFact = associatedFact;
    }

    public Fact getAssociatedFact()
    {
        return this.associatedFact;
    }

}
