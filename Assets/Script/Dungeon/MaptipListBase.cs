using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class MaptipBase
{
    public new abstract int GetType();
    public abstract int GetCount();
    public abstract int GetMinCount();
}

public class MaptipListBase : ScriptableObject
{
    [SerializeField]
    private List<MaptipBase> list;

    public virtual List<MaptipBase> GetList()
    {
        return list;
    }
}
