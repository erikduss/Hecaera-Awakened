using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntitySpawnInformation
{
    public Vector3 SpawnPosition = Vector3.zero;
    public Quaternion SpawnRotation = Quaternion.identity;
    public EnemySpawnType EntityType = EnemySpawnType.BASIC_DUMMY;
    public GameObject OverrideSpawnGameObject = null;
}
