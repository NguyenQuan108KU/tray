using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class PackManager : MonoBehaviour
{
    public static PackManager instance;

    [Header("Pack Slots")]
    public Transform packRoot;

    [Header("Pack Prefabs (1 prefab / type)")]
    public List<PackTarget> packPrefabs;

    public List<PackTarget> activePacks = new List<PackTarget>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        activePacks = packRoot.GetComponentsInChildren<PackTarget>().ToList();

        for (int i = 0; i < activePacks.Count; i++)
        {
            activePacks[i].slotIndex = i;
        }
    }

    public PackTarget GetPackInScene(ItemType type)
    {
        return activePacks.FirstOrDefault(p =>
            p.packType == type && !p.isFull
        );
    }

    /// Được gọi khi pack hoàn thành
    public void OnPackFilled(PackTarget pack)
    {
        int slotIndex = pack.slotIndex;
        Vector3 slotPos = pack.transform.position;
        ItemType type = pack.packType;

        pack.FlyUp(() =>
        {
            activePacks.Remove(pack);
            Destroy(pack.gameObject);

            SpawnSameTypePack(type, slotIndex, slotPos);
        });
    }

    void SpawnSameTypePack(ItemType type, int slotIndex, Vector3 slotPos)
    {
        PackTarget prefab = packPrefabs
            .FirstOrDefault(p => p.packType == type);
        PackTarget newPack = Instantiate(
            prefab,
            slotPos + Vector3.up * 6f,
            Quaternion.identity,
            packRoot
        );

        newPack.slotIndex = slotIndex;
        activePacks.Add(newPack);

        newPack.transform.DOMoveY(slotPos.y, 0.6f)
            .SetEase(Ease.OutQuad);
    }
}
