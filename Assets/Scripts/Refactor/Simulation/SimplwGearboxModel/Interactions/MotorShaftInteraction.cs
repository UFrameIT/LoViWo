using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorShaftInteraction : Interaction
{
    private SimulatedMotor motor;
    private SimulatedShaft shaft;

    public MotorShaftInteraction(int id, SimulatedMotor motor, SimulatedShaft shaft)
        : base(id)
    {
        this.motor = motor;
        this.shaft = shaft;
        this.interactionFact = new MotorShaftInteractionFact(id, this);
    }

    public SimulatedMotor getMotor()
    {
        return this.motor;
    }

    public SimulatedShaft getShaft()
    {
        return this.shaft;
    }

}
