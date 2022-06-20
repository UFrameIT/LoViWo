using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogwheelChainInteraction : Interaction
{
    private SimulatedCogwheel cogwheel;
    private SimulatedChain chain;
    private bool orientation;

    public CogwheelChainInteraction(int id, SimulatedCogwheel cogwheel, SimulatedChain chain, bool orientation)
        : base(id)
    {
        this.cogwheel = cogwheel;
        this.chain = chain;
        this.orientation = orientation;
        this.interactionFact = new CogChainInteractionFact(id, this);
    }

    public SimulatedCogwheel getCogwheel()
    {
        return this.cogwheel;
    }

    public SimulatedChain getChain()
    {
        return this.chain;
    }

    public bool getOrientation()
    {
        return this.orientation;
    }

}
