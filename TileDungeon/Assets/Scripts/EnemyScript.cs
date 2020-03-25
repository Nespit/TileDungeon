using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public CharacterStats characterStats;
    public CharacterCanvasController canvasController;
    
    // Start is called before the first frame update
    void Start()
    {
        characterStats = new CharacterStats(100, 5, 5);
        canvasController = GetComponent<CharacterCanvasController>();
    }
}
