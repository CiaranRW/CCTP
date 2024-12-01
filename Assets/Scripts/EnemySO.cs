using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "SO/Enemy")]
public class EnemySO : ScriptableObject
{
    public int level; // Maybe add elite enemies.
    public GameObject prefab;
    public float health;
    public float speed;
    public float damage;
    //public int team;
}
