using UnityEngine;

public class ShipParticleSystemController : MonoBehaviour
{
    public ParticleSystem shipParticleSystem;

    private Ship m_linkedShip;

    public void Initialize(Ship ship)
    {
        m_linkedShip = ship;
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
