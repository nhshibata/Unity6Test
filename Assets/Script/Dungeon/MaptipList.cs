using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Maptip : MaptipBase
{
    public enum TipType
    {
        Player,
        Enemy,
        Item,
        Goal,
        Gimick,
    }

    [SerializeField]
    private TipType type;        // 配置するオブジェクトの種類
    [SerializeField]
    private int count = 1;       // 配置する個数
    [SerializeField]
    private int minCount = 1;    // 最低配置数

    public override int GetType()
    {
        return (int)type;
    }

    public override int GetCount()
    {
        return count;
    }

    public override int GetMinCount()
    {
        return minCount;
    }
}

[CreateAssetMenu(fileName = "NewMaptipList", menuName = "Dungeon/MaptipList")]
public class MaptipList : MaptipListBase
{
    [SerializeField]
    private List<Maptip> tipList = new List<Maptip>();

    public override List<MaptipBase> GetList()
    {
        List<MaptipBase> ret = new List<MaptipBase>();
        foreach (var maptip in tipList)
        {
            ret.Add(maptip as MaptipBase);
        }
        return ret;
    }
}