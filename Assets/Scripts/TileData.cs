using UnityEngine;
using System.Collections.Generic;

public class TileData
{
    public int x;
    public int y;

    public float maxHealth;
    public float currentHealth;

    public int value;

    public bool isDestroyed;
    public int workersAssigned = 0;
    public int maxWorkers = 5; // 👈 tweak later

    public Color baseColor;

    public List<Worker> assignedWorkers = new List<Worker>();
}