using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnimation : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float duration = 1.0f; // 이펙트가 사라지는 시간
    private float elapsedTime = 0.0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / duration); // Alpha 값 계산
        Color color = spriteRenderer.color;
        color.a = alpha; // Alpha 값 적용
        spriteRenderer.color = color;

        if (elapsedTime >= duration)
        {
            Destroy(gameObject); // 이펙트 삭제
        }
    }
}
