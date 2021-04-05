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

    void Start()
    {
        this.layerMask = LayerMask.GetMask("Player","CurrentlyEdited");
        //Ignore player and current moving object
        this.layerMask = ~this.layerMask;
        Cam = Camera.main;

        CommunicationEvents.positionCogwheelEvent.AddListener(Activate);
        CommunicationEvents.openUIEvent.AddListener(Cancel);
        CommunicationEvents.closeUIEvent.AddListener(Cancel);
    }

    void Activate(GameObject obj) {
        this.movingObject = obj;
        this.movingActive = true;
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
                if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Cogwheel") && Hit.collider.gameObject.GetComponentInChildren<Cogwheel>().getModule().Equals(this.movingObject.GetComponentInChildren<Cogwheel>().getModule()))
                {
                    Vector3 currentPosition = Hit.point;
                    GameObject otherCogwheel = Hit.collider.gameObject;
                    Vector3 otherPosition = otherCogwheel.transform.position;
                    float otherPitchDiameter = otherCogwheel.GetComponentInChildren<Cogwheel>().getPitchDiameter();
                    List<Vector3> otherRelativeVectors = otherCogwheel.GetComponentInChildren<Cogwheel>().getRelativeCogInputVectors();
                    otherRelativeVectors.Sort((x,y) => Math.Abs(Vector3.Distance(currentPosition, otherPosition + (otherCogwheel.transform.rotation * x))).CompareTo(Math.Abs(Vector3.Distance(currentPosition, otherPosition + (otherCogwheel.transform.rotation * y)))));

                    float movingObjectPitchDiameter = movingObject.GetComponentInChildren<Cogwheel>().getPitchDiameter();

                    movingObject.transform.position = otherPosition + ((1 + movingObjectPitchDiameter/otherPitchDiameter) * (otherCogwheel.transform.rotation * otherRelativeVectors[0]));
                    movingObject.transform.LookAt(otherPosition, Hit.normal);
                }
                //Else: Follow cursor
                else
                {
                    float height = movingObject.transform.GetComponentInChildren<Cogwheel>().getHeight();
                    Vector3 tempPoint = Hit.point;
                    movingObject.transform.up = Hit.normal;
                    movingObject.transform.position = tempPoint;
                }

                CheckMouseButtons();
            }
        }
    }
    
    //Check if left Mouse-Button was pressed and handle it
    void CheckMouseButtons()
    {
        if (Input.GetMouseButtonDown(0))
        {
            movingObject.gameObject.layer = LayerMask.NameToLayer("Cogwheel");
            Stop();
        }
    }
}
