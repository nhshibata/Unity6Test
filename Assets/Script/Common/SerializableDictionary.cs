using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [Serializable]
    public class Pair
    {
        public TKey key = default;
        public TValue value = default;

        public Pair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [SerializeField]
    private List<Pair> pairs = new List<Pair>();

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        Clear();

        foreach (var pair in pairs)
        {
            if (ContainsKey(pair.key))
                continue;

            Add(pair.key, pair.value);
        }
    }
}
