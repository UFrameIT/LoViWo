using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeCogwheelDebug : MonoBehaviour
{
    public ConeCogwheel coneCogwheel;
    public LineRenderer lineRenderer;
    public Material debugMaterial;
    public float lineWidth = 0f;

    public bool showRelativeVectors = true;
    public bool showRightVector = false;

    public bool frontOnly = false;

    private bool debuggingActive;

    // Start is called before the first frame update
    void Start()
    {
        if (elementsNotNull() && (showRelativeVectors || showRightVector))
        {
            this.lineRenderer.enabled = true;
            this.lineRenderer.material = debugMaterial;
            this.lineRenderer.startWidth = lineWidth;
            this.lineRenderer.endWidth = lineWidth;
            this.debuggingActive = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> linePositions = new List<Vector3>();

        if (elementsNotNull() && (showRelativeVectors || showRightVector) && debuggingActive) {
            //Update material/width so we can change it during game-play
            this.lineRenderer.material = debugMaterial;
            this.lineRenderer.startWidth = lineWidth;
            this.lineRenderer.endWidth = lineWidth;

            float height = this.coneCogwheel.getHeight();
            List<Vector3> relativeCogInputVectors = this.coneCogwheel.getRelativeCogInputVectors();

            Vector3 positionBackside = this.transform.position;
            Vector3 positionFront = this.transform.position + height * (this.transform.up);

            this.lineRenderer.positionCount = 0;

            if (showRelativeVectors)
            {
                //For each side
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < relativeCogInputVectors.Count; j++)
                    {
                        if (i == 0 && !frontOnly)
                        {
                            linePositions.Add(positionBackside);
                            Vector3 newPos = positionBackside + (this.transform.rotation * relativeCogInputVectors[j]);
                            linePositions.Add(newPos);
                        }
                        else
                        {
                            linePositions.Add(positionFront);
                            Vector3 newPos = positionFront + (this.transform.rotation * relativeCogInputVectors[j]);
                            linePositions.Add(newPos);
                        }
                    }
                }

                this.lineRenderer.positionCount = linePositions.Count;
                for (int i = 0; i < this.lineRenderer.positionCount; i++)
                    this.lineRenderer.SetPosition(i, linePositions[i]);
            }
            if (showRightVector) {
                int oldPositionCount = this.lineRenderer.positionCount;

                if (!frontOnly) {
                    this.lineRenderer.positionCount += 2;
                    this.lineRenderer.SetPosition(oldPositionCount, positionBackside);
                    this.lineRenderer.SetPosition(oldPositionCount + 1, positionBackside + this.transform.right * (this.coneCogwheel.getPitchDiameter() / 2));
                }

                oldPositionCount = this.lineRenderer.positionCount;
                this.lineRenderer.positionCount += 2;
                this.lineRenderer.SetPosition(oldPositionCount, positionFront);
                this.lineRenderer.SetPosition(oldPositionCount+1, positionFront + this.transform.right * (this.coneCogwheel.getPitchDiameter()/2));
            }
        }
    }

    private bool elementsNotNull() {
        return coneCogwheel && debugMaterial != null && lineRenderer != null;
    }
}
