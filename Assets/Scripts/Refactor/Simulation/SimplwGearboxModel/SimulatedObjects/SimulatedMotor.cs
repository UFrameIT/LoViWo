using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedMotor : SimulatedObject
{
    public SimulatedMotor(int id) : base(id)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
        valuesOfInterest.Add(new ValueOfInterest("angularVelocity"));
    }

    public override void applyVluesOfInterest(Dictionary<ValueOfInterest, float> input)
    {

    }
}
