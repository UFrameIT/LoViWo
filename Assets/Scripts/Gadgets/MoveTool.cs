using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTool : MonoBehaviour
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
                // Debug.Log(Hit.transform.tag);
                /*if (Hit.collider.transform.CompareTag("SnapZone"))
                {
                    if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Ray"))
                    {

                        int id = Hit.collider.gameObject.GetComponent<FactObject>().Id;
                        RayFact lineFact = CommunicationEvents.Facts.Find((x => x.Id == id)) as RayFact;
                        PointFact p1 = CommunicationEvents.Facts.Find((x => x.Id == lineFact.Pid1)) as PointFact;
                        PointFact p2 = CommunicationEvents.Facts.Find((x => x.Id == lineFact.Pid2)) as PointFact;

                        Vector3 lineDir = p2.Point - p1.Point;
                        Plane plane = new Plane(lineDir, Hit.point);

                        Ray intersectionRay = new Ray(p1.Point, lineDir);

                        if (plane.Raycast(intersectionRay, out float enter))
                        {

                            Hit.point = p1.Point + lineDir.normalized * enter;
                        }
                        else Debug.LogError("something wrong with linesnapzone");
                        CheckMouseButtons(true, true);



                    }
                    else
                    {
                        Hit.point = Hit.collider.transform.position;
                        Hit.normal = Vector3.up;
                        CheckMouseButtons(true);
                    }


                    transform.position = Hit.point;
                    transform.up = Hit.normal;

                }
                else */
                {
                    float height = movingObject.transform.GetComponentInChildren<CogwheelGenerator>().getHeight();
                    Vector3 tempPoint = Hit.point;
                    movingObject.transform.up = Hit.normal;
                    movingObject.transform.position = tempPoint + ((height/2) * Hit.normal);
                    CheckMouseButtons();
                }

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
