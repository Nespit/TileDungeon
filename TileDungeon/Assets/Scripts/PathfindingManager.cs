using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager instance;
    public GameObject pathRendererParent;
    LineRenderer pathRenderer;
    
    List<Node> playerPath = new List<Node>();

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

    public void FindPath(Node startNode, Node targetNode, bool isPlayerPath)
    {   
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
                RetracePath(startNode, targetNode, isPlayerPath);
                DrawPath();
                return;
            }

            foreach (Node neighbour in GameManager.instance.GetViableNodeNeighbours(currentNode))
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
    }

    void RetracePath(Node startNode, Node endNode, bool isPlayerPath)
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

        if(isPlayerPath)
        {
            playerPath = path;
        }
            

        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log("Step " + i.ToString() + ": " + path[i].position.ToString());
        }
    }

    void ClearNodes()
    {
        
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs((int)nodeA.position.x - (int)nodeB.position.x);
        int dstY = Mathf.Abs((int)nodeA.position.z - (int)nodeB.position.z);

        if (dstX > dstY)
            return 14*dstY + 10*(dstX-dstY);
        return 14*dstX + 10*(dstY-dstX);
    }

    public void DrawPath()
    {
        pathRenderer.gameObject.SetActive(true);
        
        pathRenderer.positionCount = playerPath.Count;

        Debug.Log(playerPath.Count);

        for(int i = 0; i < playerPath.Count; i++)
        {
            Vector3 vector = new Vector3(playerPath[i].position.x, playerPath[i].position.y + 0.1f, playerPath[i].position.z);
            pathRenderer.SetPosition(i, vector);
        }
    }

    public void HidePath()
    {

    }
}
