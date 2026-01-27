using UnityEngine;
using System.Collections.Generic;

public class MultiFX : MonoBehaviour
{
    // 1. Define a class to hold the Pair of SFX and VFX
    [System.Serializable]
    public class EffectVariant
    {
        public string name; // Just for organization in Inspector
        public GameObject vfxPrefab;
        public AudioClip sfxClip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool destroyVfxAfterTime = true;
        public float vfxLifetime = 2f;
    }

    // 2. Define a class to link a specific Tag to a list of Variants
    [System.Serializable]
    public class TagEffectGroup
    {
        public string tagToDetect; // e.g., "Enemy", "Ground"
        public List<EffectVariant> variants; // The list to randomize
    }

    [Header("Player Ref")]
    public ShipMovement player;

    [Header("Configuration")]
    [Tooltip("Add different groups for different collision tags here.")]
    public List<TagEffectGroup> effectGroups;

    [Header("Defaults (If no tag matches)")]
    public bool useDefaultEffects = false;
    public List<EffectVariant> defaultVariants;

    // Optional: Reference to AudioSource if you don't want to use PlayClipAtPoint
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        player = GetComponent<ShipMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (player.IsDashing)
            HandleImpact(collision.gameObject.tag, collision.contacts[0].point);
    }

    // Optional: Support Trigger collisions as well
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (player.IsDashing)
            HandleImpact(other.tag, other.transform.position);
    }

    private void HandleImpact(string tag, Vector2 spawnPosition)
    {
        EffectVariant selectedVariant = null;

        // 1. Try to find a group that matches the tag we hit
        foreach (var group in effectGroups)
        {
            if (group.tagToDetect == tag)
            {
                if (group.variants.Count > 0)
                {
                    selectedVariant = GetRandomVariant(group.variants);
                }
                break;
            }
        }

        // 2. If no tag matched, check if we should use defaults
        if (selectedVariant == null && useDefaultEffects && defaultVariants.Count > 0)
        {
            selectedVariant = GetRandomVariant(defaultVariants);
        }

        // 3. Play the effects
        if (selectedVariant != null)
        {
            PlayEffect(selectedVariant, spawnPosition);
        }
    }

    private EffectVariant GetRandomVariant(List<EffectVariant> list)
    {
        int index = Random.Range(0, list.Count);
        return list[index];
    }

    private void PlayEffect(EffectVariant effect, Vector2 position)
    {
        // --- Visual Effects (VFX) ---
        if (effect.vfxPrefab != null)
        {
            // Spawn the particle/prefab at the impact point with default rotation
            GameObject instance = Instantiate(effect.vfxPrefab, position, Quaternion.identity);

            if (effect.destroyVfxAfterTime)
            {
                Destroy(instance, effect.vfxLifetime);
            }
        }

        // --- Sound Effects (SFX) ---
        if (effect.sfxClip != null)
        {
            // If we have an AudioSource on this object, use it (better for moving objects)
            if (audioSource != null && audioSource.enabled)
            {
                audioSource.PlayOneShot(effect.sfxClip, effect.volume);
            }
            // Otherwise, spawn a temporary 2D sound at the location
            else
            {
                AudioSource.PlayClipAtPoint(effect.sfxClip, position, effect.volume);
            }
        }
    }
}