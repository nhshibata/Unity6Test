using UnityEngine;
using static Maptip;

[CreateAssetMenu(fileName = "CustomMapTileConfig", menuName = "Dungeon/Custom MapTileConfig")]
public class CustomMapTileConfig : MapTileConfig
{
    [SerializeField] 
    private SerializableDictionary<TipType, GameObject> customTilePrefabs = new SerializableDictionary<TipType, GameObject>();

    public override GameObject GetGameObjectForTile(char tile)
    {
        if (int.TryParse(tile.ToString(), out int tileKey))
        {
            var key = (TipType)tileKey;
            if(customTilePrefabs.ContainsKey(key))
            {
                return customTilePrefabs[key];
            }
        }

        return base.GetGameObjectForTile(tile);
    }
}
