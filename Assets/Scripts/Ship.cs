using UnityEngine;

public class Ship : MonoBehaviour
{
    public string ShipName;
    public float Speed;
    public float Health;

    public void Initialize(string name, float speed, float health)
    {
        ShipName = name;
        Speed = speed;
        Health = health;
    }

    public void Move(Vector3 direction)
    {
        transform.Translate(direction * Speed * Time.deltaTime);
    }
}
