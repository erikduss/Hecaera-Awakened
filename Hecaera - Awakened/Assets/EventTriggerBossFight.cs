using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerBossFight : MonoBehaviour
{
    [SerializeField] int bossID;
    private bool executedTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (executedTrigger) return;

        AIBossCharacterManager boss = WorldAIManager.Instance.GetBossCharacterByID(bossID);

        if(boss != null)
        {
            executedTrigger = true;
            boss.WakeBoss();
        }
    }
}
