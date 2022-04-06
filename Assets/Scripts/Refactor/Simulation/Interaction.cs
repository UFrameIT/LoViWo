using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
