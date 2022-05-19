using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * class that represents Interactions between simulated objects
 * interactions have their own id
 * interactions have a associated Fact representing them on the MMT side
 */
public class Interaction
{
    protected int id;
    protected Fact interactionFact;

    public Interaction(int id)
    {
        this.id = id;
    }

    public int getId()
    {
        return id;
    }

    public Fact getInteractionFact()
    {
        return interactionFact;
    }
}
