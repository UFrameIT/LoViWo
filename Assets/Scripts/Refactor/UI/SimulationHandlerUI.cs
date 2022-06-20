using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationHandlerUI : MonoBehaviour
{
    public Dropdown simulationsDropdown;
    public Dropdown newSimulationType;

    public SimulationHandler simHandler;

    private List<Simulation> simulations;

    // Start is called before the first frame update
    void Start()
    {
        simulations = new List<Simulation>();

        simulationsDropdown.options.Clear();
        newSimulationType.options.Clear();
        newSimulationType.options.Add(new Dropdown.OptionData() { text = "simple Cogwheel Simulation"});
        newSimulationType.options.Add(new Dropdown.OptionData() { text = "Cogwheel forces Simulation" });

        simulationsDropdown.onValueChanged.AddListener(delegate { activeSimulationSelected(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createNewSimulation()
    {
        if (newSimulationType.value == 0)       // simple Cogwheel Simulation selected
        {
            Simulation sim = new GearboxSimulation("simpleGearboxSimulation" + this.simulationsDropdown.options.Count);
            simHandler.addSimulation(sim);
            this.simulations.Add(sim);
            simHandler.setActiveSimulation(sim);
            simulationsDropdown.options.Add(new Dropdown.OptionData() { text = sim.getSimulationName()});
        }
        else if (newSimulationType.value == 1) // Cogwheel forces Simulation selected
        {
            Simulation sim = new GearboxForcesSimulation("GearboxForcesSimulation" + this.simulationsDropdown.options.Count);
            simHandler.addSimulation(sim);
            this.simulations.Add(sim);
            simHandler.setActiveSimulation(sim);
            simulationsDropdown.options.Add(new Dropdown.OptionData() { text = sim.getSimulationName() });
        }

        activeSimulationSelected();

    }

    public void activeSimulationSelected()
    {
        this.simHandler.setActiveSimulation(simulations[simulationsDropdown.value]);
        CommunicationEvents.showSimulationObjectsPanelEvent.Invoke(simulations[simulationsDropdown.value]);
    }
}
