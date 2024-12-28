using System.Collections;
using UnityEngine;

public class RandomSelecter : MonoBehaviour
{
    [SerializeField]
    private RandomSelecterModel model;
    [SerializeField]
    private RandomSelecterView view;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        view.AddMaxListener((value) =>
        {
            value = value < 2 ? 2 : value;
            model.MaxNumber = value;
        });
        
        view.AddMinListener((value) =>
        {
            value = value < 1 ? 1 : value;
            model.MinNumber = value;
        });

        view.AddToggleListener((value) =>
        {
            model.IsUse = value;
        });

        view.OnClickStart += ((value) =>
        {
            if(value)
            {
                StartCoroutine("SelectNumber");
            }
            else
            {
                StopCoroutine("SelectNumber");
                view.SetNumber(model.SelectNumber);
            }
        });

    }

    private void OnEnable()
    {
        model.Init();
    }

    private IEnumerator SelectNumber()
    {
        while (true)
        {
            model.SelectNumber = Random.Range(model.MinNumber, model.MaxNumber);
            yield return null;
        }
    }

}
