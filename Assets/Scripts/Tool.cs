using UnityEngine;

[CreateAssetMenu(fileName = "Tool", menuName = "Game/Tool")]
public class Tool : ScriptableObject
{
    public string toolName;
    public float digPower;
    public int baseCost;
    public int maxCost;
    public float costMultiplier = 1.15f; 
    public int flatIncrease = 10; 
    public int tier;

    public Sprite workerSprite;
    public RuntimeAnimatorController animator;

    public ParticleSystem digParticles;
    public AudioClip workerLoopClip;
}