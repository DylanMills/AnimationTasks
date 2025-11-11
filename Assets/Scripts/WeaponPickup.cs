// WeaponPickup.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public GameObject weaponPrefab; // the actual weapon prefab to give the player
    public string playerTag = "Player"; // tag used to identify the player
    public AudioClip pickupSound;
    public string pickupPrompt = "Press F to pick up";

    bool playerInRange = false;
    Transform playerTransform;
    PlayerInventory playerInventory;

    void Reset()
    {
        // make sure collider is a trigger by default
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        playerTransform = other.transform;
        playerInventory = other.GetComponent<PlayerInventory>();

        // show UI prompt if you have a UI manager in your project (optional)
        UIPrompt.Show(pickupPrompt);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        playerTransform = null;
        playerInventory = null;
        UIPrompt.Hide();
    }

    void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        // hide prompt first
        UIPrompt.Hide();
      

        if (playerInventory != null)
        {
            

            // try to add to inventory (preferred)
            bool added = playerInventory.AddWeapon(weaponPrefab);
            if (added)
            {
          

                PlayPickupSound();
                Invoke("Destruct", 0.1f);// remove pickup from world
                return;
            }
        }

        // fallback: just instantiate and parent to a default hand transform on the player
        if (playerTransform != null && weaponPrefab != null)
        {
            Transform hand = FindAttachPoint(playerTransform, "Hand");
            GameObject inst = Instantiate(weaponPrefab);

            if (hand != null)
            {
                inst.transform.SetParent(hand, false);
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
            }
            PlayPickupSound();

            Destroy(gameObject);
        }
    }

    void Destruct()
    {
        Destroy(gameObject);
    }

    Transform FindAttachPoint(Transform root, string name)
    {
        // looks for a child with a given name (depth-first)
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            if (t.name == name) return t;
        }
        return null;
    }

    void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}





/*
Usage notes:
1) Create a pickup prefab (a world object). Add a Collider set to "Is Trigger" and attach WeaponPickup.cs to it.
   - Assign 'weaponPrefab' to the weapon (usually a separate prefab with visuals and optionally scripts).
   - (Optional) assign a pickup sound.
   - Ensure the player GameObject is tagged with the tag defined in playerTag (default: "Player").

2) On the Player, add the PlayerInventory component and assign a child Transform to 'handAttachPoint' (name it e.g. "Hand").

3) When the player enters the trigger, a prompt will be shown in console (replace UIPrompt with your UI). Press E to pick up.

4) Customize to your project's input system (this script uses Input.GetKeyDown(KeyCode.E)).

This example aims to be simple and clear; adapt to support networking, multiple weapon slots, or the new Unity Input System as needed.
*/
