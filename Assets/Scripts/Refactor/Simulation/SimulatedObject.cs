using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimulatedObject
{
    protected int id;
    protected Fact factRepresentation;
    protected GameObject objectRepresentation;
    protected List<ValueOfInterest> valuesOfInterest;

    public SimulatedObject(int id)
    {
        this.id = id;
    }

    public int getId()
    {
        return id;
    }

    public void addFactRepresentation(Fact factRepresentation)
    {
        this.factRepresentation = factRepresentation;
    }

    public void addObjectRepresentation(GameObject objectRepresentation)
    {
        this.objectRepresentation = objectRepresentation;
    }

    public Fact getFactRepresentation()
    {
        return factRepresentation;
    }

    public GameObject getObjectRepresentation()
    {
        return objectRepresentation;
    }

    public List<ValueOfInterest> getValuesOfInterest()
    {
        return valuesOfInterest;
    }

    public abstract void applyVluesOfInterest(Dictionary<ValueOfInterest, float> input);
}
