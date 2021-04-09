using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CogwheelMoveTool : MonoBehaviour
{
    public RaycastHit Hit;
    private Camera Cam;
    private int layerMask;

    private GameObject movingObject;
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
        this.layerMask = LayerMask.GetMask("Player","CurrentlyEdited");
        //Ignore player and current moving object
        this.layerMask = ~this.layerMask;
        Cam = Camera.main;

        CommunicationEvents.positionCogwheelEvent.AddListener(Activate);
        CommunicationEvents.openUIEvent.AddListener(Cancel);
        CommunicationEvents.closeUIEvent.AddListener(Cancel);

        if (debug && debugMaterial != null && lineRenderer != null) {
            this.lineRenderer.enabled = true;
            this.lineRenderer.material = debugMaterial;
            this.lineRenderer.startWidth = 0.095f;
            this.lineRenderer.endWidth = 0.095f;
            this.debuggingActive = true;
        }
    }

    void Activate(GameObject obj) {
        this.movingObject = obj;
        this.movingActive = true;

        if (debuggingActive) {
            this.lineRenderer.positionCount = 4;

            Vector3 nullVector = new Vector3(0, 0, 0);
            for (int i = 0; i < this.lineRenderer.positionCount; i++) {
                this.linePositions.Add(nullVector);
            }
        }
    }

    //Stop Moving AND destroy moving GameObject
    void Cancel() {
        Destroy(movingObject);
        Stop();
    }

    //Stop Moving without destroying moving GameObject
    void Stop() {
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

        if (movingActive) {

            if (Physics.Raycast(ray, out tempHit, float.MaxValue, this.layerMask))
            {
                this.Hit = tempHit;

                //If Collision with other cogwheel, that has the same module: Snapzone positioning
                if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Cogwheel"))
                {
                    this.lastCollidedObject = null;

                    if (Hit.collider.gameObject.GetComponentInChildren<Cogwheel>().getModule().Equals(this.movingObject.GetComponentInChildren<Cogwheel>().getModule()))
                    {
                        Vector3 currentPosition = Hit.point;
                        GameObject otherCogwheel = Hit.collider.gameObject;

                        Vector3 otherPosition = otherCogwheel.transform.position;
                        float otherPitchDiameter = otherCogwheel.GetComponentInChildren<Cogwheel>().getPitchDiameter();
                        List<Vector3> otherRelativeVectors = otherCogwheel.GetComponentInChildren<Cogwheel>().getRelativeCogInputVectors();
                        otherRelativeVectors.Sort((x, y) => Math.Abs(Vector3.Distance(currentPosition, otherPosition + (otherCogwheel.transform.rotation * x))).CompareTo(Math.Abs(Vector3.Distance(currentPosition, otherPosition + (otherCogwheel.transform.rotation * y)))));

                        float movingObjectPitchDiameter = movingObject.GetComponentInChildren<Cogwheel>().getPitchDiameter();

                        movingObject.transform.position = otherPosition + ((1 + movingObjectPitchDiameter / otherPitchDiameter) * (otherCogwheel.transform.rotation * otherRelativeVectors[0]));

                        //The Right-Vector of movingObject should look at the otherObject, so Vector3.Cross(up, right) gives us the resulting forward-vector
                        Vector3 right = (otherPosition - movingObject.transform.position).normalized;
                        Vector3 up = Hit.normal;
                        movingObject.transform.rotation = Quaternion.LookRotation(Vector3.Cross(up, right), up);

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
                else if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Shaft")) {
                    GameObject shaft = Hit.collider.gameObject;
                    this.lastCollidedObject = shaft;

                    Vector3 projectedPoint = Vector3.Project((Hit.point - shaft.transform.position), shaft.transform.up);

                    movingObject.transform.up = shaft.transform.up;
                    movingObject.transform.position = shaft.transform.position + projectedPoint;
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
            if (lastCollidedObject != null && lastCollidedObject.GetComponentInChildren<Connectable>() != null)
            {
                lastCollidedObject.GetComponentInChildren<Connectable>().addConnectedPart(this.movingObject.GetComponentInChildren<Rotatable>());
            }

            movingObject.gameObject.layer = LayerMask.NameToLayer("Cogwheel");
            Stop();
        }
    }
}
