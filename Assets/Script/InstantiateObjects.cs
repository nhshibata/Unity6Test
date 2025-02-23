using UnityEngine;

public class InstantiateObjects : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private int createNum;
    [SerializeField]
    private float rangeFactor = 100.0f;

    private GameObject parentObject;

    public GameObject Prefab => prefab;


    private void Start()
    {
        GenerateObjects();
    }

    public void GenerateObjects()
    {
        if (prefab == null)
            return;

        if (parentObject != null)
            return;

        // 親オブジェクトを生成
        parentObject = new GameObject($"{prefab.name} Group");
        parentObject.transform.position = transform.position;

        var range = transform.lossyScale * rangeFactor;

        for (var i = 0; i < createNum; i++)
        {
            var randomPosition = new Vector3(
                Random.Range(-range.x, range.x),
                Random.Range(-range.y, range.y),
                Random.Range(-range.z, range.z)
            );

            var randomRotation = Random.rotation;
            Instantiate(prefab, this.transform.position + randomPosition, randomRotation, parentObject.transform);
        }
    }

    public void DeleteGeneratedObjects()
    {
        if (parentObject != null)
        {
            DestroyImmediate(parentObject);
            parentObject = null;
        }
    }
}
