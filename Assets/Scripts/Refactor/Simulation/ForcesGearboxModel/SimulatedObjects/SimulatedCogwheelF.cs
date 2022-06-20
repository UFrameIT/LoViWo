using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedCogwheelF : SimulatedCogwheel
{
    private float friction;

    public SimulatedCogwheelF(int id, float friction) : base(id)
    {
        this.friction = friction;
    }

    public float getFriction()
    {
        return this.friction;
    }
}
