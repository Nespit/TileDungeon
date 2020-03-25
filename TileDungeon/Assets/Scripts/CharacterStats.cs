using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CharacterStats
{
    public int health;
    public int attack;
    public int defense;

    public CharacterStats(int health, int attack, int defense)
    {
        this.health = health;
        this.attack = attack;
        this.defense = defense;
    }
}
