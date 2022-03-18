using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Functionallity for handling Simulations that are used in the GameWorld
 * 
 * 
 * 
 */
public class SimulationHandler : MonoBehaviour
{
    // The Simulation currently being handled
    private Simulation activeSimulation;

    private int nextId;


    public int getNextId()
    {
        return nextId;
    }

    public void activeSimAddSimObject(SimulatedObject simObject)
    {
        activeSimulation.addSimulatedObject(simObject);
        this.nextId++;
    }

    public void activeSimulationStartSimulation()
    {
        activeSimulation.startSimulation();
    }

    public void activeSimulationStopSimulation()
    {
        activeSimulation.stopSimulation();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.activeSimulation = new GearboxSimulation();
        this.nextId = 0;
        GameState.simulationHandler = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
