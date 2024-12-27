using System.Collections;
using UnityEngine;

public class deactivateui : MonoBehaviour
{
    public float time = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Deactivate());
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
