using UnityEngine;
using Gameplay;
namespace GameWorld
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private bool collectAutomatically = true;
        [SerializeField] private int[] itemIDs;
        [SerializeField] private int[] count;

        private bool collected = false;

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent<Player>(out _) && collectAutomatically)
            {
                Collect();
            }
        }

        private void Collect()
        {
            if (collected) { return; }
            collected = true;
            for (int i = 0; i < itemIDs.Length; i++)
            {
                Inventory.AddItem(itemIDs[i], count[i]);
            }
            Destroy(gameObject);
        }
    }
}