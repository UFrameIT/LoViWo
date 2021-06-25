using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GeneratorMoveTool : MonoBehaviour
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

        CommunicationEvents.positionGeneratorEvent.AddListener(Activate);
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
                
                //If Collision with Ground
                if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    movingObject.transform.position = Hit.point;
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
            //Set Layer for gameobject and all its children
            string tagLayerName = "Generator";
            SetLayerRecursively(movingObject, LayerMask.NameToLayer(tagLayerName));
            movingObject.gameObject.tag = tagLayerName;
            Stop();
        }
    }

    void SetLayerRecursively(GameObject obj, int layer) {
        obj.layer = layer;

        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
