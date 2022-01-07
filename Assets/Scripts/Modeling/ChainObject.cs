using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChainObject : MonoBehaviour, Chain, Interlockable
{
    private List<Interlockable> interlockingObjects = new List<Interlockable>();

    private List<Tuple<GameObject, bool>> cogwheels;
    private List<Vector3> chainShape;
    private float pathLength;
    private float chainSpeed;


    public GameObject chainSegmentPrefab;

    private List<GameObject> segments;

    private float segmentSpacing;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void createSegments(float spacing)
    {
        for (int i = 0; i < (pathLength / spacing); i++)
        {
            placeSegment(i * spacing);
        }
    }

    private void placeSegment(float distance)
    {
        GameObject segment = Instantiate(chainSegmentPrefab);
        segment.GetComponent<ChainSegment>().createSegment(chainShape, distance, pathLength);

        segments.Add(segment);
    }

    private Vector3 pointOnPath(float distance)
    {
        float remaining_dist = distance;

        int currentPoint = 0;
        int nextPoint = 1;

        while (remaining_dist > Vector3.Distance(chainShape[currentPoint], chainShape[nextPoint]))
        {
            remaining_dist = remaining_dist - Vector3.Distance(chainShape[currentPoint], chainShape[nextPoint]);

            currentPoint = nextPoint;
            nextPoint += 1;

            if (nextPoint >= chainShape.Count)
            {
                nextPoint = 0;
            }
        }

        return Vector3.MoveTowards(chainShape[currentPoint], chainShape[nextPoint], remaining_dist);
    }

    private float getPathLength()
    {
        float dist = 0;
        for (int i = 0; i < chainShape.Count; i++)
        {
            if (i < chainShape.Count - 1)
            {
                dist += Vector3.Distance(chainShape[i], chainShape[i + 1]);
            }
            else
            {
                dist += Vector3.Distance(chainShape[i], chainShape[0]);
            }
        }
        return dist;
    }

    public void createChain(List<Tuple<GameObject, bool>> in_cogwheels, List<Vector3> in_chainShape)
    {
        segments = new List<GameObject>();

        cogwheels = in_cogwheels;
        chainShape = in_chainShape;
        pathLength = getPathLength();

        createSegments(2.5f);

        foreach (Tuple<GameObject, bool> in_cog in in_cogwheels)
        {
            RotatableCogwheel cog = in_cog.Item1.GetComponentInChildren<RotatableCogwheel>();
            if (cog != null)
            {
                this.addInterlockingPart(cog);
                cog.addInterlockingPart(this);
            }
            else
            {
                UnityEngine.Debug.Log("chain unsuccessfully added interlocking");
            }
        }

    }

    public void move(float distance)
    {
        foreach (GameObject segment in this.segments)
        {
            segment.GetComponent<ChainSegment>().move(distance);
        }
    }

    public void stop_moving()
    {
        foreach (GameObject segment in this.segments)
        {
            segment.GetComponent<ChainSegment>().stop();
        }
    }

    public void addInterlockingPart(Interlockable part)
    {
        interlockingObjects.Add(part);
    }

    public List<Interlockable> getInterlockingParts()
    {
        return this.interlockingObjects;
    }

    public Transform getRootTransform()
    {
        return this.transform.root;
    }

    public void activatePhysics()
    {
       
    }

    public void deactivatePhysics()
    {
       
    }
}
