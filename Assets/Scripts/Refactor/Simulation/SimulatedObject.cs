using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * simulated objects are all the objects that are either affecting the knowledge based simulation
 * or objects that are to be 'simulated' by the knowledge based simulation
 * They have 
 * an id
 * a Fact representing the Object on the MMT side
 * a GameObject representing the Object in the GameWorld
 * a List of 'values of interest' representong the aspects of the object
 * that the kowledge based simulation is sopposed to determine
 */
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

    /*
     * communicate the results of the knowledge based simulation to the object
     * and have the object reflect these results in the game world
     */
    public abstract void applyValuesOfInterest(Dictionary<ValueOfInterest, float> input);

    public abstract void unapplyValuesOfInterest();
}
