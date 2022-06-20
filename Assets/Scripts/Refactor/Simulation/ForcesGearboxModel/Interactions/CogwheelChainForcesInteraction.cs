using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogwheelChainForcesInteraction : CogwheelChainInteraction, ForcesInteraction
{
    private List<ValueOfInterest> valuesOfInterest;

    public CogwheelChainForcesInteraction(int id, SimulatedCogwheelF cogwheel, SimulatedChain chain, bool orientation) 
                                         : base(id, cogwheel, chain, orientation)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();

        valuesOfInterest.Add(new ValueOfInterest("cog" + cogwheel.getId().ToString()
                                               + "_chain" + chain.getId().ToString() + "_frc"));
    }

    List<ValueOfInterest> ForcesInteraction.getValuesOfInterest()
    {
        return this.valuesOfInterest;
    }
}
