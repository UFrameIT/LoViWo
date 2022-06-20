using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaftCogwheelForcesInteraction : ShaftCogwheelInteraction, ForcesInteraction
{
    private List<ValueOfInterest> valuesOfInterest;

    public ShaftCogwheelForcesInteraction(int id, SimulatedCogwheelF cogwheel, SimulatedShaft shaft) : base(id, cogwheel, shaft)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();

        valuesOfInterest.Add(new ValueOfInterest("cog" + cogwheel.getId().ToString()
                                               + "_shaft" + shaft.getId().ToString() + "_trq"));
    }

    List<ValueOfInterest> ForcesInteraction.getValuesOfInterest()
    {
        return this.valuesOfInterest;
    }
}
