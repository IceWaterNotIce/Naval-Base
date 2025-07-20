using UnityEngine;

public class ShipParticleSystemController : MonoBehaviour
{
    public ParticleSystem shipParticleSystem;

    private Ship m_linkedShip;

    private void Awake()
    {
        // 自動從父物件獲取 Ship 組件
        m_linkedShip = GetComponentInParent<Ship>();
        if (m_linkedShip == null)
        {
            Debug.LogWarning("No Ship component found in parent hierarchy.");
        }
    }

    private void Update()
    {
        if (m_linkedShip != null && shipParticleSystem != null)
        {
            var mainModule = shipParticleSystem.main;
            mainModule.startLifetime = m_linkedShip.CurrentSpeed * 10;
        }
    }
}
