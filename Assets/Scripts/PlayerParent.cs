using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlayerParent
{
    public void getUltimateAndBombNumber(ref int ultimate, ref int bombNumber);
    public void setUltimateAndBombNumber(int ultimate, int bombNumber);
    public void setPos(Vector3 target, int[] posIndex, bool fast = false, Map map = null);
    public int[] getPosIndex();
    public void resetPoint();
    public void addPoint(int value);
    public int getPoint();

    public Transform getTransform();
}
