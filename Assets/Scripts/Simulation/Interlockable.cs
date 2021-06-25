using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interlockable
{
    void addInterlockingPart(Interlockable part);
    List<Interlockable> getInterlockingParts();
    Transform getRootTransform();
    void activatePhysics();
    void deactivatePhysics();
}
