using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorShaftForcesInteraction : MotorShaftInteraction, ForcesInteraction
{
    private List<ValueOfInterest> valuesOfInterest;

    public MotorShaftForcesInteraction(int id, SimulatedMotor motor, SimulatedShaft shaft) : base(id, motor, shaft)
    {
        this.valuesOfInterest = new List<ValueOfInterest>();
        
        valuesOfInterest.Add(new ValueOfInterest("motot" + motor.getId().ToString() 
                                               + "_shaft" + shaft.getId().ToString() + "_trq"));

    }

    List<ValueOfInterest> ForcesInteraction.getValuesOfInterest()
    {
        return this.valuesOfInterest;
    }
}
