using System.Collections.Generic;
using UnityEngine;

public class AmmoManager : MonoBehaviour
{
    public GameObject ammoPrefab; // Prefab for ammo
    private List<GameObject> activeAmmo = new List<GameObject>();

    public void RegisterAmmo(GameObject ammo)
    {
        activeAmmo.Add(ammo);
    }

    public List<GameObject> GetActiveAmmo()
    {
        return activeAmmo; // Return the list of active ammo
    }

    public void ClearAmmo()
    {
        foreach (GameObject ammo in activeAmmo)
        {
            if (ammo != null)
            {
                Destroy(ammo); // Destroy all active ammo
            }
        }
        activeAmmo.Clear(); // Clear the list
    }

    void Update()
    {
        CleanupAmmo();
    }

    public void CleanupAmmo()
    {
        for (int i = activeAmmo.Count - 1; i >= 0; i--)
        {
            if (activeAmmo[i] == null)
            {
                activeAmmo.RemoveAt(i); // Ensure null ammo is removed
            }
        }
    }
}
