using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Cogwheel
{
    void generateMesh(float height, int cogCount, float radius);

    float getHeight();

    float getModule();

    float getCogAngle();

    float getPitchDiameter();

    float getRadius();

    float getInsideRadius();

    float getOutsideRadius();

    List<Vector3> getRelativeCogInputVectors();
}
