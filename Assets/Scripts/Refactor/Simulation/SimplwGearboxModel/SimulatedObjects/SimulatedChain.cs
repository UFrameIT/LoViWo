using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedChain : SimulatedObject
{
    public SimulatedChain(int id) : base(id)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
        valuesOfInterest.Add(new ValueOfInterest("linearVelocity"));
    }

    public override void applyVluesOfInterest(Dictionary<ValueOfInterest, float> input)
    {

    }
}
