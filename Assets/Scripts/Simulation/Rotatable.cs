using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Rotatable
{
    void rotate(float angularVelocity);
    void stopRotation();
}
