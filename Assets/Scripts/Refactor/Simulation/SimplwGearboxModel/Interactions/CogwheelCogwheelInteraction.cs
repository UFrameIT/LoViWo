using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogwheelCogwheelInteraction : Interaction
{
    private SimulatedCogwheel cogwheel1;
    private SimulatedCogwheel cogwheel2;

    public CogwheelCogwheelInteraction(int id, SimulatedCogwheel cogwheel1, SimulatedCogwheel cogwheel2) 
        : base(id)
    {
        this.cogwheel1 = cogwheel1;
        this.cogwheel2 = cogwheel2;
        this.interactionFact = new CogCogInteractionFact(id, this);
    }

    public SimulatedCogwheel getCogwheel1()
    {
        return this.cogwheel1;
    }

    public SimulatedCogwheel getCogwheel2()
    {
        return this.cogwheel2;
    }

}
