using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface Chain
{
    void move(float distance);

    void createChain(List<Tuple<GameObject, bool>> cogwheels, List<Vector3> chainShape);
}


