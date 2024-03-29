using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager instance;
    public GameObject pathRendererParent;
    LineRenderer pathRenderer;
    public delegate void NodeResetDelegate(object sender, EventArgs args);
    public event NodeResetDelegate ResetEvent;

    private void Awake() 
    {
        if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);    
		}
		DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        pathRenderer = pathRendererParent.GetComponent<LineRenderer>();
    }

    public List<Node> FindPath(Node startNode, Node targetNode, bool isPlayerPath, int actionPoints = 0)
    {   
        ResetEvent(null, null);
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        openSet[0].open = true;

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost() < currentNode.fCost() || openSet[i].fCost() == currentNode.fCost() && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            currentNode.open = false;
            closedSet.Add(currentNode);
            currentNode.closed = true;

            if (currentNode == targetNode) 
            {
                List<Node> nodes;
                
                nodes = RetracePath(startNode, targetNode);
                if(isPlayerPath) 
                {
                    DrawPath(nodes, actionPoints);
                }

                return nodes;
            }

            foreach (Node neighbour in GameManager.instance.GetViableNodeNeighbours(currentNode, targetNode))
            {
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if(!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        neighbour.open = true;
                    }     
                }
            }
        }

        return null;
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(currentNode);

        path.Reverse();

        return path;

        // for (int i = 0; i < path.Count; i++)
        // {
        //     Debug.Log("Step " + i.ToString() + ": " + path[i].position.ToString());
        // }
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs((int)nodeA.position.x - (int)nodeB.position.x);
        int dstY = Mathf.Abs((int)nodeA.position.z - (int)nodeB.position.z);

        if (dstX > dstY)
            return 14*dstY + 10*(dstX-dstY);
        return 14*dstX + 10*(dstY-dstX);
    }

    public void DrawPath(List<Node> unitPath, int actionPoints)
    {   
        if(pathRenderer.gameObject.activeInHierarchy == false)
            pathRenderer.gameObject.SetActive(true);
        
        pathRenderer.positionCount = unitPath.Count;

        for(int i = 0; i < unitPath.Count; i++)
        {
            Vector3 vector = new Vector3(unitPath[i].position.x, unitPath[i].position.y + 0.1f, unitPath[i].position.z);
            pathRenderer.SetPosition(i, vector);
        }

        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        float alpha = 1.0f;

        if(unitPath.Count-1 > actionPoints)
        {
            float redPosition = scale(0f, unitPath.Count-1.5f, 0f, 1f, actionPoints);
            
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.green, redPosition), new GradientColorKey(Color.red, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha, 1f) }
            );

            UIManager.instance.SelectTile(unitPath[unitPath.Count-1].position, false);
        }
        else
        {
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f)},
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f)}
            );

            UIManager.instance.SelectTile(unitPath[unitPath.Count-1].position, true);
        }

        pathRenderer.colorGradient = gradient;
    }

    public float scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {
     
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
     
        return(NewValue);
    }


    public void HidePath()
    {

    }
}
