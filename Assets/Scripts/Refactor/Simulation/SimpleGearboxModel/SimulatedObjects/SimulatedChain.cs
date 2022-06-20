using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedChain : SimulatedObject
{
    public SimulatedChain(int id) : base(id)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
        valuesOfInterest.Add(new ValueOfInterest("Chain_" + id.ToString() + "_cv"));
    }

    public override void applyValuesOfInterest(Dictionary<ValueOfInterest, float> input)
    {
        if (input.ContainsKey(this.valuesOfInterest[0]))
        {
            float cv = input[this.valuesOfInterest[0]];

            this.getObjectRepresentation().GetComponentInChildren<ChainObject>().move(cv * ((2.0f * 3.14f) / 360.0f));
        }
    }

    public override void unapplyValuesOfInterest()
    {
        this.getObjectRepresentation().GetComponentInChildren<ChainObject>().stop_moving();
    }
}
