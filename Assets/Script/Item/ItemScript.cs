using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public GameObject GravityItemEffect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        // GravityItem 태그를 가진 오브젝트와 충돌 시
        if (other.CompareTag("Player"))
        {
            GravityItemEffect.SetActive(true);
        }
    }
}
