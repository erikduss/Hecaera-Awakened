using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaterialManagement : MonoBehaviour
{
    public static PlayerMaterialManagement Instance;
    public List<Material> additionalPlayerMaterials = new List<Material>();
    public int lastSelectedMaterialIndex = -1;

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
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetMaterial(PlayerManager player, int materialIndex, Color materialColor, bool useSetColor = false)
    {
        Debug.Log(player.IsOwnedByServer + " _ " + materialIndex + " _ " + materialColor);

        if (player.IsOwnedByServer && materialIndex == -1) return;

        Debug.Log("Setting color!");
        GameObject bodyChild = null;

        for (int i = 0; i< player.transform.childCount; i++)
        {
            Transform child = player.transform.GetChild(i);

            if (child.tag == "PlayerBody")
            {
                //The get child is only the first chain of children. Meaning, it only finds 2 childs.
                //The body gameobject is under the first child we find, as the only (first) gameobject under it.
                bodyChild = child.gameObject.transform.GetChild(0).gameObject; 
                i = player.transform.childCount; //Quit the loop
            }
        }

        if(bodyChild != null)
        {
            SkinnedMeshRenderer materialRenderer = bodyChild.GetComponent<SkinnedMeshRenderer>();

            if (useSetColor == false && additionalPlayerMaterials.Count >= materialIndex)
            {
                materialRenderer.material = additionalPlayerMaterials[materialIndex];
                Debug.Log("Set the material at index: " + materialIndex);
            }
            else //if we set to a specific color, or if we ran out of preset materials. Create a new one.
            {
                Debug.Log("Setting the color: " + materialColor);
                Material newAdditionalMaterial = new Material(additionalPlayerMaterials[0]);
                newAdditionalMaterial.color = materialColor;

                materialRenderer.material = newAdditionalMaterial;
            }
        }
    }
}
