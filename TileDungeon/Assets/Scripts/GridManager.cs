using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    [SerializeField]
    public GameObject tilePrefab;
    [SerializeField]
    public Vector2 maxGridSize;

    Grid grid;

    void Awake()
    {
        if (transform.childCount > 0)
        {
            grid = ConvertSceneToGrid();
        }
    }

    void Start()
    {
        GridOutline();
    }
    
    private void GridOutline()
    {
        //Vertices & UVs
        Vector3[] vertices = 
        {
            new Vector3 ((float)-0.5, 0, (float)-0.5),
            new Vector3 ((float)(maxGridSize.x-0.5), 0, (float)-0.5),
            new Vector3 ((float)(maxGridSize.x-0.5), (float)0.01, (float)-0.5),
            new Vector3 ((float)-0.5, (float)0.01, (float)-0.5),
            new Vector3 ((float)-0.5, (float)0.01, (float)(maxGridSize.y-0.5)),
            new Vector3 ((float)(maxGridSize.x-0.5), (float)0.01, (float)(maxGridSize.y-0.5)),
            new Vector3 ((float)(maxGridSize.x-0.5), 0, (float)(maxGridSize.y-0.5)),
            new Vector3 ((float)-0.5, 0, (float)(maxGridSize.y-0.5))
        };

        Vector2[] uv = 
        {
            new Vector2 (0, 0),
            new Vector2 (1, 0),
            new Vector2 (1, 1),
            new Vector2 (0, 1),
            new Vector2 (0, 0),
            new Vector2 (1, 0),
            new Vector2 (1, 1),
            new Vector2 (0, 1)
        };

        //Triangles
        int[] triangleIndices = 
        {
            0, 2, 1, //face front
            0, 3, 2,
            2, 3, 4, //face top
            2, 4, 5,
            1, 2, 5, //face right
            1, 5, 6,
            0, 7, 4, //face left
            0, 4, 3,
            5, 4, 7, //face back
            5, 7, 6,
            0, 6, 7, //face bottom
            0, 1, 6
        };

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangleIndices
        };

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = mesh;
    }

    Grid ConvertSceneToGrid()
    {
        int tileCount = transform.childCount;
        Grid grid;
        grid.tiles = new Tile[tileCount, tileCount];
        
        foreach(Transform t in transform)
        {
             grid.tiles[Mathf.RoundToInt(t.position.x), Mathf.RoundToInt(t.position.z)].gameObject = t.gameObject;
        }

        return  grid;
    }

    void Update()
    {

    }
}

public struct Tile
{
    public GameObject gameObject;
}

public struct Grid
{
    public Tile[,] tiles;
}