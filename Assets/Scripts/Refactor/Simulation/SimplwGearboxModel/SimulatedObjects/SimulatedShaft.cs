using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedShaft : SimulatedObject
{
    public SimulatedShaft(int id) : base(id)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
        valuesOfInterest.Add(new ValueOfInterest("Shaft_" + id.ToString() + "_av"));
    }

    public override void applyValuesOfInterest(Dictionary<ValueOfInterest, float> input)
    {
        float av = input[this.valuesOfInterest[0]];

        this.getObjectRepresentation().GetComponentInChildren<RefactorShaft>().rotate(av, true);
    }

    public override void unapplyValuesOfInterest()
    {
        this.getObjectRepresentation().GetComponentInChildren<RefactorShaft>().stopRotation();
    }
}
