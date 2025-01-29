using Assets.Scripts.Core;
using Assets.Scripts.Items;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.MainMenu.Shop
{
    public class ShopUIScript : MonoBehaviour
    {
        private static ItemManager ItemManager => GameManager.ItemManager;
        
        [SerializeField] private Renderer _katana;
        [SerializeField] private TMP_Text _coinsTMP;

        private void Start()
        {
            ItemManager.OnCoinsChanged += UpdateCoins;
            ItemManager.OnItemChanged  += UpdateKatana;
            UpdateCoins();
            UpdateKatana();
        }

        private void UpdateCoins() => _coinsTMP.text = $@"₦{ItemManager.Coins}";

        private void UpdateKatana() => _katana.sharedMaterial = ItemManager.SelectedItem.Material;
    }
}
