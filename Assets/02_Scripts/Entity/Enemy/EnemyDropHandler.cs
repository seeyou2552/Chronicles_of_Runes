using _02_Scripts.Inventory;
using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace _02_Scripts.Enetity.Enemy
{
    public class EnemyDropHandler : MonoBehaviour
    {
        public GameObject dropItemPrefab;
        public GameObject dropGoldPrefab;
        [SerializeField] private GameObject DefaultSkillPrefab;
        [SerializeField] private GameObject DefaultRunePrefab;
        [SerializeField] private GameObject DefaultPotionPrefab;

        public void SpawnDropItems(ItemData itemData)
        {
            if (dropItemPrefab == null || dropGoldPrefab == null)
            {
                return;
            }

            if (itemData == null)
            {
                return;
            }

            Logger.Log(transform.name + " spawned");

            Vector3 origin = transform.position;

            SpriteRenderer image = dropItemPrefab.GetComponentInChildren<SpriteRenderer>();
            if (image == null)
            {
                return;
            }

            image.sprite = itemData.icon;

            // 아이템 1개
            SpawnAndMoveToPlayer(dropItemPrefab, origin, itemData);
        }

        public void SpawnDropGold(int amount)
        {
            Vector3 origin = transform.position;
            for (int i = 0; i < amount; i++)
            {
                ItemData goldData = new ItemData { itemType = ItemType.Gold };
                SpawnAndMoveToPlayer(dropGoldPrefab, origin, goldData); // ← 잘못된 부분 수정됨!
            }
        }


        private void SpawnAndMoveToPlayer(GameObject prefab, Vector3 origin, ItemData itemData)
        {
            GameObject item = Instantiate(prefab, origin, Quaternion.identity);
            Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
            Vector3 jumpTarget = origin + (Vector3)randomOffset;

            // 살짝 튀어나오게 (0.3초)
            item.transform.DOMove(jumpTarget, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // 1초 대기 후 플레이어에게 빨려감
                DOVirtual.DelayedCall(1f, () =>
                {
                    MoveToPlayer(item, itemData);
                });
            });
        }
        
        private void MoveToPlayer(GameObject item, ItemData itemData)
        {
            Transform player = PlayerController.Instance.transform;

            if (GameManager.Instance.state != GameState.GameOver)
            {
                StartCoroutine(AggressiveTrackPlayer(item, player, itemData));
            }
        }
        
        private IEnumerator AggressiveTrackPlayer(GameObject item, Transform player, ItemData itemData)
        {
            float baseSpeed = 5f;          // 더 빠른 기본 속도
            float accelerationRate = 30f;   // 더 빠른 가속도
            float maxSpeed = 150f;          // 더 높은 최대 속도
            float currentSpeed = baseSpeed;
            float shrinkStartDistance = 5f;
            float pickupDistance      = 0.3f;
            
            while (item != null && player != null)
            {
                Vector3 originalScale = item.transform.localScale;
                float dist = Vector3.Distance(item.transform.position, player.position);

                // 0.3f 이하면 아이템 획득
                if (dist < 0.3f)
                {
                    HandlePickup(item, itemData);
                    yield break;
                }
                
                float t = Mathf.InverseLerp(pickupDistance, shrinkStartDistance, dist);
                item.transform.localScale = originalScale * t;
                
                // 거리가 멀수록 더 빠르게 이동
                float distanceBonus = Mathf.Min(dist * 0.5f, 5f);
                currentSpeed = Mathf.Min(currentSpeed + accelerationRate * Time.deltaTime, maxSpeed);
                float finalSpeed = currentSpeed + distanceBonus;
                
                // 플레이어 방향으로 이동
                Vector3 direction = (player.position - item.transform.position).normalized;
                item.transform.position += direction * finalSpeed * Time.deltaTime;
                yield return null;
            }
        }
        
        private void HandlePickup(GameObject item, ItemData itemData)
        {
            if (itemData == null)
            {
                Destroy(item);
                return;
            }

            var slot = InventoryManager.Instance.inventoryUIManager.FindFirstEmptySlot();
            if (slot == null)
            {
                Destroy(item);
                return;
            }

            GameObject instance = null;

            if (itemData.itemType == ItemType.Gold)
            {
                InventoryManager.Instance.addGold(1);
            }
            else
            {
                InventoryManager.Instance.AddItem(itemData.id);    
            }

            Destroy(item); // 나중에 풀링으로 교체 가능
        }
    }
}