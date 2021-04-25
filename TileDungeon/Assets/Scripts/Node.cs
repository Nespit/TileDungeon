using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{ 
    public bool walkable;
    public bool open, closed;
    public int gCost;
    public int hCost;
    public int fCost()
    {
        return gCost + hCost;
    }
    public Node parent;

    public Vector3 position;

    public Node(int x, float y, int z, bool _walkable)
    {
        position = new Vector3(x, y, z);
        walkable = _walkable;
        open = false;
        closed = false;
    }
}
