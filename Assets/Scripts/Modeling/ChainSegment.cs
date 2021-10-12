using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ChainSegment
{
    void move(float distance);
    void stop();

    void createSegment(List<Vector3> chainShape, float offset, float pathLength);
}

