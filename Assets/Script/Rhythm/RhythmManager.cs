using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager instance; // �̱��� �ν��Ͻ�

    private bool canInput = false; // �Է� ���� ����
    private bool hasInputOccurred = false; // �Է��� �߻��ߴ��� ����
    private float defaultCoolTime = 1.3f; // �Է� ���� ���°� �Ǳ������ �⺻ ��� �ð�
    private float jumpCoolTime = 0.3f; // ���� ���� ������ �پ�� ��Ÿ��
    private float inputWindow = 0.7f; // �Է��� ������ �ð�

    private int inputCount = 0; // ���� �Է� Ƚ��
    private int requiredInputs = 3; // �䱸�Ǵ� ���� �Է� Ƚ��

    public Text rhythmStatusText; // ���� ���¸� ǥ���� UI �ؽ�Ʈ ������Ʈ
    public Text inputCountText; // �Է� Ƚ���� ǥ���� �ؽ�Ʈ

    private float coolTime; // ���� ��Ÿ��, �������� ����
    private float timer; // ���� Ÿ�̸�

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        coolTime = defaultCoolTime; // �ʱ� ��Ÿ�� ����
        timer = coolTime + inputWindow; // �ʱ� Ÿ�̸� ����
        UpdateInputCountText();
    }

    void FixedUpdate()
    {
        if (PlayerScript.instance.IsJumping && PlayerScript.instance.jumpCount < 1)
        {
            coolTime = jumpCoolTime; // ���� ���� �� ��Ÿ�� ����
            if (!hasInputOccurred)
            {
                PlayerScript.instance.FallDown();
            }
        }
        else if (PlayerScript.instance.IsGravityTiming)
        {
            coolTime = Mathf.Infinity; // �߷� ���� �� ��Ÿ�� ����
        }
        else
        {
            coolTime = defaultCoolTime; // �⺻ ��Ÿ������ �缳��
        }

        if (coolTime < Mathf.Infinity)
        {
            if (timer > inputWindow)
            {
                timer -= Time.fixedDeltaTime;
                rhythmStatusText.text = Mathf.RoundToInt(timer).ToString();
            }
            else if (timer <= inputWindow && timer > 0)
            {
                canInput = true;
                timer -= Time.fixedDeltaTime;
                rhythmStatusText.text = Mathf.RoundToInt(timer).ToString();
            }
            else
            {
                if (!hasInputOccurred)
                {
                    inputCount++; // ����ڰ� �Է����� ������ �Է� Ƚ�� ����
                    if (inputCount >= requiredInputs)
                    {
                        PlayerScript.instance.FallDown();
                        inputCount = 0; // �Է� Ƚ�� �ʱ�ȭ
                    }
                }
                canInput = false;
                timer = coolTime + inputWindow; // Ÿ�̸� �缳��
                hasInputOccurred = false; // �Է� ���� �ʱ�ȭ
            }
        }

        UpdateInputCountText(); // �Է� Ƚ�� ������Ʈ
    }

    // �Է� ���� ���θ� ��ȯ
    public bool CanInput()
    {
        return canInput && !hasInputOccurred;
    }

    // �Է� �߻��� ���
    public void RecordInput()
    {
        if (canInput)
        {
            hasInputOccurred = true;
            inputCount = 0; // �Է� �߻� �� �Է� Ƚ�� ����
        }
    }

    private void UpdateInputCountText()
    {
        inputCountText.text = inputCount.ToString();
    }
}
