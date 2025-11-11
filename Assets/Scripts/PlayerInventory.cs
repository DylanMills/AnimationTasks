// PlayerInventory.cs
using UnityEngine;
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public Transform handAttachPoint; // where weapons get parented

    public ThirdPersonController controller;
    // up to 3 weapons
    public GameObject[] weaponInstances = new GameObject[3];
    public GameObject[] weaponPrefabs = new GameObject[3];
    int activeSlot = -1; // no weapon active at start





    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchSlot(2);
    }
    public bool AddWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null) return false;


        // find first empty slot
        for (int i = 0; i < 3; i++)
        {
            if (weaponPrefabs[i] == null)
            {
                weaponPrefabs[i] = weaponPrefab;
                controller.WeaponSwitchInput(i+1);
                SwitchSlot(i);
                return true;
            }
        }
        // inventory full – could replace active
        // here we'll replace active
        if (activeSlot >= 0)
        {
            DropSlot(activeSlot);
            weaponPrefabs[activeSlot] = weaponPrefab;
            EquipSlot(activeSlot);
            return true;
        }
        return false;
    }

    void SwitchSlot(int slot)
    {
        Debug.Log("slot:"+slot);
        Debug.Log("activeslot:"+activeSlot);
        if (slot == activeSlot) return;
      //  if (weaponPrefabs[slot] == null);


        // unequip current
        if (activeSlot >= 0) UnequipSlot(activeSlot);


        // equip new
        EquipSlot(slot);
        activeSlot = slot;
    }


    void EquipSlot(int slot)
    {
        if (weaponPrefabs[slot] == null) return;
        Transform parent = handAttachPoint != null ? handAttachPoint : transform;
        weaponInstances[slot] = Instantiate(weaponPrefabs[slot], parent);
        weaponInstances[slot].transform.localPosition = Vector3.zero;
        //weaponInstances[slot].transform.localRotation = Quaternion.identity;


        Collider col = weaponInstances[slot].GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody rb = weaponInstances[slot].GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }


    void UnequipSlot(int slot)
    {
        if (weaponInstances[slot] != null)
        {
            Destroy(weaponInstances[slot]);
            weaponInstances[slot] = null;
        }
    }


    public void DropWeapon()
    {
        if (activeSlot < 0) return;
        DropSlot(activeSlot);
        activeSlot = -1;
    }


    void DropSlot(int slot)
    {
        if (weaponInstances[slot] == null) return;


        weaponInstances[slot].transform.SetParent(null);
        Collider col = weaponInstances[slot].GetComponent<Collider>();
        if (col != null) col.enabled = true;
        Rigidbody rb = weaponInstances[slot].GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;


        if (rb != null) rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.VelocityChange);


        weaponInstances[slot] = null;
        weaponPrefabs[slot] = null;
    }
}