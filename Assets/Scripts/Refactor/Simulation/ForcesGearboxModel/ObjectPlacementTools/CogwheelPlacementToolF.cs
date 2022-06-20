using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static JSONManager;

public class CogwheelPlacementToolF : MonoBehaviour
{
    public RaycastHit Hit;
    private Camera Cam;
    private int layerMask;

    private GameObject movingObject;
    private float cogwheelFric;
    private bool movingActive;

    private GameObject lastCollidedObject;

    //Variables for debugging
    public Boolean debug = false;
    private Boolean debuggingActive = false;
    public Material debugMaterial;
    public LineRenderer lineRenderer;
    private List<Vector3> linePositions = new List<Vector3>();

    void Start()
    {
        this.layerMask = LayerMask.GetMask("Player", "CurrentlyEdited", "SimulatedObjects");
        //Ignore player and current moving object
        this.layerMask = ~this.layerMask;
        Cam = Camera.main;

        CommunicationEvents.positionCogwheelFEvent.AddListener(Activate);
        CommunicationEvents.openUIEvent.AddListener(Cancel);
        CommunicationEvents.closeUIEvent.AddListener(Cancel);

        if (debug && debugMaterial != null && lineRenderer != null)
        {
            this.lineRenderer.enabled = true;
            this.lineRenderer.material = debugMaterial;
            this.lineRenderer.startWidth = 0.095f;
            this.lineRenderer.endWidth = 0.095f;
            this.debuggingActive = true;
        }
    }

    void Activate(GameObject obj, float cogFriction)
    {
        this.movingObject = obj;
        this.movingActive = true;
        this.cogwheelFric = cogFriction;

        if (debuggingActive)
        {
            this.lineRenderer.positionCount = 4;

            Vector3 nullVector = new Vector3(0, 0, 0);
            for (int i = 0; i < this.lineRenderer.positionCount; i++)
            {
                this.linePositions.Add(nullVector);
            }
        }
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

        if (debuggingActive)
        {
            this.lineRenderer.positionCount = 0;
            this.linePositions = new List<Vector3>();
        }
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

                //If Collision with other cogwheel, that has the same module: Snapzone positioning
                if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Cogwheel"))
                {
                    this.lastCollidedObject = null;

                    if ((Hit.collider.gameObject.transform.parent != null
                            && Hit.collider.gameObject.transform.parent.GetComponentInChildren<Cogwheel>() != null
                            && Math.Abs(Hit.collider.gameObject.transform.parent.GetComponentInChildren<Cogwheel>().getModule() - this.movingObject.GetComponentInChildren<Cogwheel>().getModule()) < 0.001f)
                        ||
                        (Hit.collider.gameObject.GetComponentInChildren<Cogwheel>() != null
                        && Math.Abs(Hit.collider.gameObject.GetComponentInChildren<Cogwheel>().getModule() - this.movingObject.GetComponentInChildren<Cogwheel>().getModule()) < 0.001f))
                    {
                        Vector3 currentPosition = Hit.point;
                        GameObject otherCogwheel;
                        if (Hit.collider.gameObject.transform.parent != null)
                            otherCogwheel = Hit.collider.gameObject.transform.parent.gameObject;
                        else
                            otherCogwheel = Hit.collider.gameObject;

                        this.lastCollidedObject = otherCogwheel;

                        Vector3 otherPosition = otherCogwheel.transform.position;
                        float otherPitchDiameter = otherCogwheel.GetComponentInChildren<Cogwheel>().getPitchDiameter();
                        List<Vector3> otherRelativeVectors = otherCogwheel.GetComponentInChildren<Cogwheel>().getRelativeCogInputVectors();
                        otherRelativeVectors.Sort((x, y) => Math.Abs(Vector3.Distance(currentPosition, otherPosition + (otherCogwheel.transform.rotation * x))).CompareTo(Math.Abs(Vector3.Distance(currentPosition, otherPosition + (otherCogwheel.transform.rotation * y)))));

                        float movingObjectPitchDiameter = movingObject.GetComponentInChildren<Cogwheel>().getPitchDiameter();

                        movingObject.transform.position = otherPosition + ((1 + movingObjectPitchDiameter / otherPitchDiameter) * (otherCogwheel.transform.rotation * otherRelativeVectors[0]));

                        //In our created Cogwheels the right-vector and forward-vector are switched. So the right-vector is actually
                        //the vector that points to the initial gap at 0°
                        //If we want the Right-Vector of movingObject to look at the otherObject's position,
                        //the forward-vector must be Vector3.Cross(up, left)

                        //If we would use Vector3.Cross(up, right) here, the forward vector would result in a rotation,
                        //where rotation.right would be -1 * (*right-vector-we-actually-want*)
                        //This can cause problems for cogwheels with odd cogCount, because here, the right-vector
                        //points to the middle of a cog-gap, whereas -1 * right-vector (left-vector) points to the middle of a cog
                        Vector3 left = -1 * (otherPosition - movingObject.transform.position).normalized;
                        Vector3 up = otherCogwheel.transform.up;
                        movingObject.transform.rotation = Quaternion.LookRotation(Vector3.Cross(up, left), up);

                        if (debuggingActive)
                        {
                            this.linePositions[0] = otherPosition;
                            this.linePositions[1] = otherPosition + (otherCogwheel.transform.rotation * otherRelativeVectors[0]);
                            this.linePositions[2] = movingObject.transform.position + (movingObjectPitchDiameter / 2 * movingObject.transform.right);
                            this.linePositions[3] = movingObject.transform.position;

                            for (int i = 0; i < this.lineRenderer.positionCount; i++)
                                this.lineRenderer.SetPosition(i, this.linePositions[i]);
                        }
                    }
                }
                //If Collision with Shaft
                else if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Shaft"))
                {
                    GameObject shaft = Hit.collider.gameObject;
                    this.lastCollidedObject = shaft;

                    Vector3 projectedPoint = Vector3.Project((Hit.point - shaft.transform.position), shaft.transform.up);

                    movingObject.transform.up = shaft.transform.up;
                    movingObject.transform.position = shaft.transform.position + projectedPoint;

                    //temporary modification to make placing cogwheels in the same plane across different shafts easier

                    if (shaft.transform.up == Vector3.back)
                    {
                        Vector3 pos = movingObject.transform.position;
                        pos.z = Mathf.Round(pos.z);
                        movingObject.transform.position = pos;
                    }

                }
                //Else: Follow cursor
                else
                {
                    this.lastCollidedObject = Hit.collider.gameObject;

                    float height = movingObject.transform.GetComponentInChildren<Cogwheel>().getHeight();
                    movingObject.transform.up = Hit.normal;
                    movingObject.transform.position = Hit.point;
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
            Tuple<List<RefactorCogwheel>, List<RefactorCogwheel>> IntersectingAndInterlockingCogwheels 
                = movingObject.GetComponentInChildren<RefactorCogwheel>().getIntersectingAndInterlockingCogwheels();
            if (IntersectingAndInterlockingCogwheels.Item1.Count > 0)
            {
                return;
            }
            //Create new Cogwheel Simulated Object
            SimulatedCogwheelF simCogwheel = createSimulatedCogwheelF(movingObject, this.cogwheelFric);
            createCogwheelForcesInteractions(simCogwheel, IntersectingAndInterlockingCogwheels.Item2);

            if (lastCollidedObject != null && lastCollidedObject.GetComponentInChildren<RefactorShaft>() != null)
            {
                RefactorShaft shaft = lastCollidedObject.GetComponentInChildren<RefactorShaft>();
                shaft.addConnectedObject(this.movingObject);
                createShaftForcesInteraction((SimulatedShaft)shaft.getSimulatedObject(), simCogwheel);
            }
            if (lastCollidedObject != null && lastCollidedObject.GetComponentInChildren<Connectable>() != null)
            {
                lastCollidedObject.GetComponentInChildren<Connectable>().addConnectedPart(this.movingObject.GetComponentInChildren<Rotatable>());
            }
            if (lastCollidedObject != null && lastCollidedObject.GetComponentInChildren<Interlockable>() != null)
            {
                lastCollidedObject.GetComponentInChildren<Interlockable>().addInterlockingPart(this.movingObject.GetComponentInChildren<Interlockable>());
            }

            string tagLayerName = "Cogwheel";
            movingObject.gameObject.layer = LayerMask.NameToLayer(tagLayerName);
            //Set Layer for all children
            foreach (Transform t in movingObject.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = LayerMask.NameToLayer(tagLayerName);
            }
            if (movingObject.GetComponentInChildren<OutsideRadiusCollider>() != null && movingObject.GetComponentInChildren<InsideRadiusCollider>() != null)
            {
                movingObject.GetComponentInChildren<OutsideRadiusCollider>().gameObject.layer = LayerMask.NameToLayer("SimulatedObjects");
                movingObject.GetComponentInChildren<InsideRadiusCollider>().gameObject.layer = LayerMask.NameToLayer("SimulatedObjects");
            }
            movingObject.gameObject.tag = tagLayerName;
            
            Stop();
        }
    }

    private SimulatedCogwheelF createSimulatedCogwheelF(GameObject movingObject, float fric)
    {
        int id = GameState.simulationHandler.getNextId();
        SimulatedCogwheelF simCogwheel = new SimulatedCogwheelF(id, fric);
        simCogwheel.addObjectRepresentation(movingObject);
        movingObject.GetComponentInChildren<RefactorCogwheel>().setSimulatedObject(simCogwheel);
        GameState.simulationHandler.activeSimAddSimObject(simCogwheel);

        float radius = simCogwheel.getObjectRepresentation().GetComponentInChildren<Cogwheel>().getRadius();
        float insideRadius = simCogwheel.getObjectRepresentation().GetComponentInChildren<Cogwheel>().getInsideRadius();
        float outsideRadius = simCogwheel.getObjectRepresentation().GetComponentInChildren<Cogwheel>().getOutsideRadius();
        Vector3 pos = simCogwheel.getObjectRepresentation().transform.position;
        Vector3 up = simCogwheel.getObjectRepresentation().transform.up;

        CogwheelFact factRepresentation = new CogwheelFact(id, pos, up, radius, insideRadius, outsideRadius, fric);
        simCogwheel.addFactRepresentation(factRepresentation);
        simCogwheel.getValuesOfInterest().First().setRelevantFact((Fact)factRepresentation);
        simCogwheel.getObjectRepresentation().GetComponentInChildren<RotatableCogwheel>().setAssociatedFact(factRepresentation);

        return simCogwheel;
    }

    private void createCogwheelForcesInteractions(SimulatedCogwheelF simCogwheel, List<RefactorCogwheel> InterlockingCogwheels)
    {
        List<CogwheelCogwheelForcesInteraction> interactions = new List<CogwheelCogwheelForcesInteraction>();
        foreach (RefactorCogwheel interlocking in InterlockingCogwheels)
        {
            int id = GameState.simulationHandler.getNextId();
            SimulatedCogwheelF simCog2 = (SimulatedCogwheelF)interlocking.getSimulatedObject();
            CogwheelCogwheelForcesInteraction interaction = new CogwheelCogwheelForcesInteraction(id, simCogwheel, simCog2);
            ((ForcesInteraction)interaction).getValuesOfInterest().First().setRelevantFact(interaction.getInteractionFact());
            GameState.simulationHandler.activeSimAddInteraction(interaction);
        }
    }
    private void createShaftForcesInteraction(SimulatedShaft simShaft, SimulatedCogwheelF simCogwheel)
    {
        int id = GameState.simulationHandler.getNextId();
        ShaftCogwheelForcesInteraction interaction = new ShaftCogwheelForcesInteraction(id, simCogwheel, simShaft);
        ((ForcesInteraction)interaction).getValuesOfInterest().First().setRelevantFact(interaction.getInteractionFact());
        GameState.simulationHandler.activeSimAddInteraction(interaction);
    }

}