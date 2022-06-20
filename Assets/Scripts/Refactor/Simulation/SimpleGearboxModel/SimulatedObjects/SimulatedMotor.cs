using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedMotor : SimulatedObject
{
    public SimulatedMotor(int id) : base(id)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
    }

    public override void applyValuesOfInterest(Dictionary<ValueOfInterest, float> input)
    {

    }

    public override void unapplyValuesOfInterest()
    {

    }
}
