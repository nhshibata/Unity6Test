using UnityEngine;

[CreateAssetMenu(fileName = "MapTileConfig", menuName = "Dungeon/Base MapTileConfig")]
public class MapTileConfig : ScriptableObject
{
    [SerializeField] 
    private GameObject wallPrefab;
    [SerializeField] 
    private GameObject pathPrefab;
    [SerializeField] 
    private GameObject roomOuterPrefab;
    [SerializeField] 
    private GameObject roomInnerPrefab;

    public virtual GameObject GetGameObjectForTile(char tile)
    {
        return tile switch
        {
            '#' => wallPrefab,
            '.' => pathPrefab,
            '@' => roomOuterPrefab,
            '&' => roomInnerPrefab,
            _ => null,
        };
    }
}
