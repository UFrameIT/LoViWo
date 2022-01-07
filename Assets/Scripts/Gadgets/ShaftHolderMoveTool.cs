using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShaftHolderMoveTool : MonoBehaviour
{
    public RaycastHit Hit;
    private Camera Cam;
    private int layerMask;

    private GameObject movingObject;
    private bool movingActive;

    private Transform initialStandTransform;
    private Transform initialBottomTransform;
    private float shaftHolderBorderWidth = 1.0f;
    private float radius;

    void Start()
    {
        this.layerMask = LayerMask.GetMask("Player","CurrentlyEdited");
        //Ignore player and current moving object
        this.layerMask = ~this.layerMask;
        Cam = Camera.main;

        CommunicationEvents.positionShaftHolderEvent.AddListener(Activate);
        CommunicationEvents.openUIEvent.AddListener(Cancel);
        CommunicationEvents.closeUIEvent.AddListener(Cancel);
    }

    void Activate(GameObject obj) {
        this.movingObject = obj;
        this.initialStandTransform = this.movingObject.transform.GetChild(0);
        this.initialBottomTransform = this.movingObject.transform.GetChild(1);
        this.radius = initialStandTransform.localScale.y / 8;
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
                    this.movingObject.transform.GetChild(0).localScale = initialStandTransform.localScale;
                    this.movingObject.transform.GetChild(0).localPosition = initialStandTransform.localPosition;
                    this.movingObject.transform.GetChild(1).localScale = initialBottomTransform.localScale;
                    this.movingObject.transform.GetChild(1).localPosition = initialBottomTransform.localPosition;

                    movingObject.transform.position = Hit.point + Hit.normal * (initialStandTransform.localScale.y + radius + shaftHolderBorderWidth);
                    movingObject.transform.right = Hit.normal;
                    movingObject.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.back);

                    float height = movingObject.GetComponentInChildren<ShaftHolder>().height;
                    this.movingObject.transform.GetChild(0).localScale = new Vector3(initialStandTransform.localScale.x, height - radius - shaftHolderBorderWidth, initialStandTransform.localScale.z);
                    this.movingObject.transform.GetChild(0).localPosition = new Vector3(-(this.movingObject.transform.GetChild(0).localScale.y / 2 + radius + shaftHolderBorderWidth), initialStandTransform.localPosition.y, initialStandTransform.localPosition.z);
                    this.movingObject.transform.GetChild(1).localPosition = new Vector3((-(shaftHolderBorderWidth + radius + this.movingObject.transform.GetChild(0).localScale.y - this.movingObject.transform.GetChild(1).localScale.x / 2)), initialBottomTransform.localPosition.y, initialBottomTransform.localPosition.z);

                }
                //If Collision with Shaft
                else if(Hit.collider.gameObject.layer == LayerMask.NameToLayer("Shaft"))
                {
                    GameObject shaft = Hit.collider.gameObject;
                    Vector3 projectedPoint = Vector3.Project((Hit.point - shaft.transform.position), shaft.transform.up);

                    movingObject.transform.rotation = Quaternion.LookRotation(-shaft.transform.right, shaft.transform.up);
                    movingObject.transform.position = shaft.transform.position + projectedPoint;

                    float height = movingObject.transform.position.y;
                    this.movingObject.transform.GetChild(0).localScale = new Vector3(initialStandTransform.localScale.x, height - radius - shaftHolderBorderWidth, initialStandTransform.localScale.z);
                    this.movingObject.transform.GetChild(0).localPosition = new Vector3(-(this.movingObject.transform.GetChild(0).localScale.y/2 + radius + shaftHolderBorderWidth), initialStandTransform.localPosition.y, initialStandTransform.localPosition.z);
                    this.movingObject.transform.GetChild(1).localPosition = new Vector3((-(shaftHolderBorderWidth + radius + this.movingObject.transform.GetChild(0).localScale.y - this.movingObject.transform.GetChild(1).localScale.x / 2)), initialBottomTransform.localPosition.y, initialBottomTransform.localPosition.z);
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
            string tagLayerName = "ShaftHolder";
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
