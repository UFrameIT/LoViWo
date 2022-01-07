using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainSegmentObject : MonoBehaviour, ChainSegment
{
    private bool moving;
    private float speed;
    private float pathPos;
    private List<Vector3> chainShape;
    private float pathLength;

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            pathPos += speed * Time.deltaTime;
            if (pathPos >= pathLength)
            {
                pathPos -= pathLength;
            }
            moveToPathPos(pathPos);
        }
    }

    public void stop()
    {
        moving = false;
    }

    public void createSegment(List<Vector3> chainShape, float offset, float pathLength)
    {
        this.chainShape = chainShape;
        this.pathPos = offset;
        this.pathLength = pathLength;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = this.transform;
        moveToPathPos(offset);

    }

    public void move(float speed)
    {
        moving = true;
        this.speed = speed;
    }

    private void moveToPathPos(float distance)
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

        transform.position = Vector3.MoveTowards(chainShape[currentPoint], chainShape[nextPoint], remaining_dist);
        Vector3 dir = chainShape[nextPoint] - chainShape[currentPoint];
        transform.rotation = Quaternion.LookRotation(dir);
    }

}
