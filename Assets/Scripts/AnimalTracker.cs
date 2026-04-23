using System.Collections.Generic;
using UnityEngine;

public class AnimalTracker : MonoBehaviour
{
    public static AnimalTracker Instance;

    private HashSet<Animal> activeAnimals = new HashSet<Animal>();

    void Awake()
    {
        Instance = this;
    }

    public int ActiveCount => activeAnimals.Count;

    public void Register(Animal animal)
    {
        if (animal == null) return;

        activeAnimals.Add(animal);
        UpdateFlash();
    }

    public void Unregister(Animal animal)
    {
        if (animal == null) return;

        activeAnimals.Remove(animal);
        UpdateFlash();
    }

    public void OnAnimalNeutralized()
    {
        UpdateFlash();
    }

    public void RetreatWave()
    {
        foreach (var a in activeAnimals)
        {
            if (a != null)
                a.RetreatToLeftAndWait();
        }

        UpdateFlash();
    }

    public void ResumeWave()
    {
        foreach (var a in activeAnimals)
        {
            if (a != null)
                a.ResumeAttack();
        }

        UpdateFlash();
    }

    void UpdateFlash()
    {
        bool anyDanger = false;

        foreach (var a in activeAnimals)
        {
            if (a != null && a.IsDangerous())
            {
                anyDanger = true;
                break;
            }
        }

        if (anyDanger)
            AttackFlashUI.Instance?.StartFlashing();
        else
            AttackFlashUI.Instance?.StopFlashing();
    }
}