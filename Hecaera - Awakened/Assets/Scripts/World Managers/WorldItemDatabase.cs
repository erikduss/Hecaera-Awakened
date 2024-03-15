using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Erikduss
{
    public class WorldItemDatabase : MonoBehaviour
    {
        public static WorldItemDatabase Instance;

        public WeaponItem unarmedWeapon;

        [Header("Weapon")]
        [SerializeField] List<WeaponItem> weapons = new List<WeaponItem>();

        //List of every item in the game with unique IDs
        private List<Item> items = new List<Item>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);

            //Add all our weapons to item list
            foreach (var weapon in weapons)
            {
                items.Add(weapon);
            }

            //Assign unique id to all items
            for (int i = 0; i < items.Count; i++)
            {
                items[i].itemID = i;
            }
        }

        public WeaponItem GetWeaponByID(int ID)
        {
            return weapons.FirstOrDefault(weapon => weapon.itemID == ID);
        }
    }
}
