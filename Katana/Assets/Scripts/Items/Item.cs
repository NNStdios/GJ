using System;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "Item")]
    public class Item : ScriptableObject
    {
        public string Name;
        public int Price;
        public bool UnlockedByDefault;
        public Material Material;

        public bool Unlocked
        {
            get => PlayerPrefs.GetInt($"{Name}Unlocked", 0) == 1 || UnlockedByDefault;
            set
            {
                PlayerPrefs.SetInt($"{Name}Unlocked", value ? 1 : 0);
                OnUnlocked?.Invoke();
            }
        }

        public event Action OnUnlocked;
    }
}