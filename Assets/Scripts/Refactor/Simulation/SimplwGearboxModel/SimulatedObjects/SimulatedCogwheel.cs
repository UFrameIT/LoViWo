using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedCogwheel : SimulatedObject
{
    public SimulatedCogwheel(int id) : base(id)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
        valuesOfInterest.Add(new ValueOfInterest("Cogwheel_" + id.ToString() + "_av"));
    }

    public override void applyValuesOfInterest(Dictionary<ValueOfInterest, float> input)
    {
        float av = input[this.valuesOfInterest[0]];

        this.getObjectRepresentation().GetComponentInChildren<RotatableCogwheel>().rotate(av, true);
    }
    public override void unapplyValuesOfInterest()
    {
        this.getObjectRepresentation().GetComponentInChildren<RotatableCogwheel>().stopRotation();
    }

}

