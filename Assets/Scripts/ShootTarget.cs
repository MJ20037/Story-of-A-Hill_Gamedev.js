using UnityEngine;

public class ShootTarget : MonoBehaviour
{
    public AudioClip hitClip;
    public AudioSource audioSource;

    public void OnHit(PlayerTool tool)
    {
        if(audioSource.isPlaying) return;
        audioSource.PlayOneShot(hitClip);
    }
}