using System;
using UnityEngine;

public class SpawnUI : MonoBehaviour
{
    
    public GameObject uiPrefab;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(uiPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
