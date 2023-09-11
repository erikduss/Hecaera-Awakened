using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SoulsLike
{
    public class SoulCountBar : MonoBehaviour
    {
        public TextMeshProUGUI soulCountText;

        public void SetSoulCountText(int soulCount)
        {
            soulCountText.text = soulCount.ToString();
        }
    }
}
