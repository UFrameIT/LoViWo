using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShaftMoveTool : MonoBehaviour
{
    public RaycastHit Hit;
    private Camera Cam;
    private int layerMask;

    private GameObject movingObject;
    private bool movingActive;

    private GameObject lastCollidedObject;

    void Start()
    {
        this.layerMask = LayerMask.GetMask("Player","CurrentlyEdited");
        //Ignore player and current moving object
        this.layerMask = ~this.layerMask;
        Cam = Camera.main;

        CommunicationEvents.positionShaftEvent.AddListener(Activate);
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
                else
                {
                    this.lastCollidedObject = Hit.collider.gameObject;

                    movingObject.transform.position = Hit.point + Hit.normal * (movingObject.transform.localScale.x / 2);
                    movingObject.transform.forward = Hit.normal;
                }

                CheckMouseButtons();
            }
            else {
                lastCollidedObject = null;
            }
        }
    }
    
    //Check if left Mouse-Button was pressed and handle it
    void CheckMouseButtons()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (lastCollidedObject != null && getUpperParent(lastCollidedObject).GetComponentInChildren<Connectable>() != null) {
                getUpperParent(lastCollidedObject).GetComponentInChildren<Connectable>().addConnectedPart(this.movingObject.GetComponentInChildren<Rotatable>());
            }

            movingObject.gameObject.layer = LayerMask.NameToLayer("Shaft");
            Stop();
        }
    }

    GameObject getUpperParent(GameObject go) {
        if (go.transform.parent != null)
            return getUpperParent(go.transform.parent.gameObject);
        else
            return go;
    }
}
