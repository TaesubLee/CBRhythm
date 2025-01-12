using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnimation : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float duration = 1.0f; // ����Ʈ�� ������� �ð�
    private float elapsedTime = 0.0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / duration); // Alpha �� ���
        Color color = spriteRenderer.color;
        color.a = alpha; // Alpha �� ����
        spriteRenderer.color = color;

        if (elapsedTime >= duration)
        {
            Destroy(gameObject); // ����Ʈ ����
        }
    }
}
