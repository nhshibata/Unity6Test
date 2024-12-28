using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomSelecterModel
{
    [SerializeField]
    private int maxNumber = 10;
    [SerializeField]
    private int minNumber = 1;

    [SerializeField]
    private List<int> list = new List<int>();

    private int selectNumber = 0;
    private bool isUse = false;
    private Dictionary<int, int> map = new Dictionary<int, int>();

    public int MaxNumber { get => maxNumber; set => maxNumber = value; }
    public int MinNumber { get => minNumber; set => minNumber = value; }
    public int SelectNumber { get => selectNumber; set => selectNumber = value; }
    public bool IsUse { get => isUse; set => isUse = value; }


    public void Init()
    {
        foreach (var item in map.Keys)
        {
            map[item] = list[item];
        }

    }

    public bool IsSelect(int index)
    {
        if(map.ContainsKey(index))
        {
            var num = map[index];

            if(num == 0)
                return false;
            else
            {
                if(isUse)
                    map[index]--;
                return true;
            }
        }

        int reg = list[index - 1] - 1;
        map.Add(index, reg);
        return true;
    }


}
