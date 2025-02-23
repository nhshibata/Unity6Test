using UnityEngine;

public class ParameterUpdater : MonoBehaviour
{
    [SerializeField]
    private Parameter param;

    private bool isGot = false;
    private int type = 0;


    void OnEnable()
    {
        type = param.type;
        isGot = Parameter.Get(ref param);
    }

    void OnDisable()
    {
        isGot = false;
    }

    void Update()
    {
        if (param.type != type)
        {
            isGot = false;
        }

        // タイプ変更後はパラメタを取得
        if (!isGot)
        {
            isGot = Parameter.Get(ref param);
            if (isGot)
            {
                type = param.type;
            }
        }
        // パラメタ取得済みの場合はエディタで指定したもので書き換え
        else
        {
            Parameter.Set(param);
        }
    }
}