using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaftCogwheelInteraction : Interaction
{
    private SimulatedCogwheel cogwheel;
    private SimulatedShaft shaft;

    public ShaftCogwheelInteraction(int id, SimulatedCogwheel cogwheel, SimulatedShaft shaft)
        : base(id)
    {
        this.cogwheel = cogwheel;
        this.shaft = shaft;
        this.interactionFact = new ShaftCogInteractionFact(id, this);
    }

    public SimulatedCogwheel getCogwheel()
    {
        return this.cogwheel;
    }

    public SimulatedShaft getShaft()
    {
        return this.shaft;
    }

}
