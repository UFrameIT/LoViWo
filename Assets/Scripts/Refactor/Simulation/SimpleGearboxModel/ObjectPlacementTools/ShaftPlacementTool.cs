using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static JSONManager;

public class ShaftPlacementTool : MonoBehaviour
{
    public RaycastHit Hit;
    private Camera Cam;
    private int layerMask;

    private GameObject movingObject;
    private bool movingActive;

    private GameObject lastCollidedObject;

    void Start()
    {
        this.layerMask = LayerMask.GetMask("Player", "CurrentlyEdited", "SimulatedObjects");
        //Ignore player and current moving object
        this.layerMask = ~this.layerMask;
        Cam = Camera.main;

        CommunicationEvents.positionShaftEvent.AddListener(Activate);
        CommunicationEvents.openUIEvent.AddListener(Cancel);
        CommunicationEvents.closeUIEvent.AddListener(Cancel);
    }

    void Activate(GameObject obj)
    {
        this.movingObject = obj;
        this.movingActive = true;
    }

    //Stop Moving AND destroy moving GameObject
    void Cancel()
    {
        Destroy(movingObject);
        Stop();
    }

    //Stop Moving without destroying moving GameObject
    void Stop()
    {
        movingObject = null;
        movingActive = false;
    }

    void Update()
    {
        Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit tempHit;

        if (movingActive)
        {

            if (Physics.Raycast(ray, out tempHit, float.MaxValue, this.layerMask))
            {
                this.Hit = tempHit;

                //If Collision with Generator
                if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Generator"))
                {
                    GameObject generator = Hit.collider.gameObject;
                    this.lastCollidedObject = generator;

                    movingObject.transform.position = generator.transform.position + Hit.normal * (movingObject.transform.localScale.y + (generator.transform.localScale.y / 2));
                    movingObject.transform.up = Hit.normal;
                }
                //If Collision with Cogwheel
                else if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Cogwheel"))
                {
                    GameObject otherCogwheel = Hit.collider.gameObject;
                    this.lastCollidedObject = otherCogwheel;

                    movingObject.transform.position = otherCogwheel.transform.position;
                    movingObject.transform.up = otherCogwheel.transform.up;
                }
                //If Collision with ShaftHolder
                else if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("ShaftHolder"))
                {
                    GameObject shaftHolder = Hit.collider.gameObject;
                    this.lastCollidedObject = shaftHolder;

                    movingObject.transform.position = shaftHolder.transform.position;
                    movingObject.transform.up = shaftHolder.transform.up;
                }
                else
                {
                    this.lastCollidedObject = Hit.collider.gameObject;

                    movingObject.transform.position = Hit.point + Hit.normal * (movingObject.transform.localScale.x / 2);
                    movingObject.transform.forward = Hit.normal;
                }

                CheckMouseButtons();
            }
            else
            {
                lastCollidedObject = null;
            }
        }
    }

    //Check if left Mouse-Button was pressed and handle it
    void CheckMouseButtons()
    {

        if (Input.GetMouseButtonDown(0))
        {
            SimulatedShaft simShaft = createSimulatedShaft(movingObject);

            if (lastCollidedObject != null && getUpperParent(lastCollidedObject).GetComponentInChildren<Connectable>() != null)
            {
                getUpperParent(lastCollidedObject).GetComponentInChildren<Connectable>().addConnectedPart(this.movingObject.GetComponentInChildren<Rotatable>());
            }
            if (lastCollidedObject != null && getUpperParent(lastCollidedObject).GetComponentInChildren<RefactorMotor>() != null)
            {
                RefactorMotor motor = getUpperParent(lastCollidedObject).GetComponentInChildren<RefactorMotor>();
                motor.addConnecedShaft(this.movingObject);
                createMotorInteraction((SimulatedMotor)motor.getSimulatedObject(), simShaft);
                Debug.Log("added shaft to generator");
            }
            if (lastCollidedObject != null && getUpperParent(lastCollidedObject).GetComponentInChildren<RefactorCogwheel>() != null)
            {
                RefactorCogwheel cogwheel = getUpperParent(lastCollidedObject).GetComponentInChildren<RefactorCogwheel>();
                createShaftInteraction(simShaft, (SimulatedCogwheel)cogwheel.getSimulatedObject());
                Debug.Log("added shaft cogwheel Interaction");
            }

            string tagLayerName = "Shaft";
            movingObject.gameObject.layer = LayerMask.NameToLayer(tagLayerName);
            movingObject.gameObject.tag = tagLayerName;
            Stop();
        }
    }

    GameObject getUpperParent(GameObject go)
    {
        if (go.transform.parent != null)
            return getUpperParent(go.transform.parent.gameObject);
        else
            return go;
    }


    private SimulatedShaft createSimulatedShaft(GameObject movingObject)
    {
        int id = GameState.simulationHandler.getNextId();
        SimulatedShaft simShaft = new SimulatedShaft(id);
        simShaft.addObjectRepresentation(movingObject);
        GameState.simulationHandler.activeSimAddSimObject(simShaft);
        movingObject.GetComponentInChildren<RefactorShaft>().addSimulatedObject(simShaft);

        ShaftFact shaftFact = new ShaftFact(id);

        simShaft.addFactRepresentation(shaftFact);
        simShaft.getValuesOfInterest().First().setRelevantFact((Fact)shaftFact);
        simShaft.getObjectRepresentation().GetComponentInChildren<RefactorShaft>().addAssociatedFact(shaftFact);

        return simShaft;
    }

    private void createShaftInteraction(SimulatedShaft simShaft, SimulatedCogwheel simCogwheel)
    {
        int id = GameState.simulationHandler.getNextId();
        ShaftCogwheelInteraction interaction = new ShaftCogwheelInteraction(id, simCogwheel, simShaft);
        GameState.simulationHandler.activeSimAddInteraction(interaction);
    }

    private void createMotorInteraction(SimulatedMotor simMotor, SimulatedShaft simShaft)
    {
        int id = GameState.simulationHandler.getNextId();
        MotorShaftInteraction interaction = new MotorShaftInteraction(id, simMotor, simShaft);
        GameState.simulationHandler.activeSimAddInteraction(interaction);
    }
}
