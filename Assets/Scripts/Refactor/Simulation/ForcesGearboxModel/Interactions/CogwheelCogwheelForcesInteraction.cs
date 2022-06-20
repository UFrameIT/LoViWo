using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogwheelCogwheelForcesInteraction : CogwheelCogwheelInteraction, ForcesInteraction
{
    private List<ValueOfInterest> valuesOfInterest;

    public CogwheelCogwheelForcesInteraction(int id, SimulatedCogwheelF cog1, SimulatedCogwheelF cog2) : base(id, cog1, cog2)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();

        valuesOfInterest.Add(new ValueOfInterest("cog" + cog1.getId().ToString()
                                               + "_cog" + cog2.getId().ToString() + "_frc"));
    }

    List<ValueOfInterest> ForcesInteraction.getValuesOfInterest()
    {
        return this.valuesOfInterest;
    }
}
