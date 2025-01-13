using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager instance; // �̱��� �ν��Ͻ�

    private bool canInput = false; // �Է� ���� ����
    private bool hasInputOccurred = false; // �Է� �߻� ����
    private float coolTime = 0.5f; // ��Ÿ��
    private float inputWindow = 0.5f; // �Է� ���� �ð�

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(RhythmCycle());
    }

    // ���� �ֱ� ����
    IEnumerator RhythmCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(coolTime);
            canInput = true; // �Է� ���� ���� Ȱ��ȭ
            hasInputOccurred = false; // �Է� ���� �ʱ�ȭ

            yield return new WaitForSeconds(inputWindow);
            canInput = false; // �Է� ���� ���� ��Ȱ��ȭ
        }
    }

    // �Է� ���� ���� Ȯ��
    public bool CanInput()
    {
        // isJumping�� true�� �ٷ� �Է� ����, �ƴϸ� �Ϲ� ����
        if ((PlayerScript.instance.IsJumping && PlayerScript.instance.jumpCount < 1) || PlayerScript.instance.IsGravityTiming || !PlayerScript.instance.IsOnGround())
        {
            return true;
        }

        return canInput && !hasInputOccurred;
    }

    // �Է� �߻� ���
    public void RecordInput()
    {
        hasInputOccurred = true;
    }
}
