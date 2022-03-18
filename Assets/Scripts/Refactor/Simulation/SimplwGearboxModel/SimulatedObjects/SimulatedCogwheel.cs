using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedCogwheel : SimulatedObject
{
    public SimulatedCogwheel(int id) : base(id)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
        valuesOfInterest.Add(new ValueOfInterest("angularVelocity"));
    }

    public override void applyVluesOfInterest(Dictionary<ValueOfInterest, float> input)
    {

    }
}

