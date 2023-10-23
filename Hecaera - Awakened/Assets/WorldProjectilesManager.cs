using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldProjectilesManager : MonoBehaviour
{
    public static WorldProjectilesManager Instance;

    public List<Projectile> projectiles = new List<Projectile>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnProjectile(int type)
    {
        ProjectileType fixedType = (ProjectileType)type;

        if (projectiles.Count == 0) return;

        var projectileToSpawn = projectiles.Where(a => a.projectileType == fixedType).FirstOrDefault();

        if (projectileToSpawn != null)
        {
            var spawnedProjectile = Instantiate(projectileToSpawn.gameObject, Vector3.zero, Quaternion.identity);
        }
    }
}
