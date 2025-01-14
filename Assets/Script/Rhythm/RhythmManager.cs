using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager instance; // 싱글톤 인스턴스

    private bool canInput = false; // 입력 가능 여부
    private bool hasInputOccurred = false; // 입력이 발생했는지 여부
    private float defaultCoolTime = 1.3f; // 입력 가능 상태가 되기까지의 기본 대기 시간
    private float jumpCoolTime = 0.3f; // 점프 중인 동안의 줄어든 쿨타임
    private float inputWindow = 0.7f; // 입력이 가능한 시간

    private int inputCount = 0; // 현재 입력 횟수
    private int requiredInputs = 3; // 요구되는 연속 입력 횟수

    public Text rhythmStatusText; // 리듬 상태를 표시할 UI 텍스트 컴포넌트
    public Text inputCountText; // 입력 횟수를 표시할 텍스트

    private float coolTime; // 현재 쿨타임, 동적으로 설정
    private float timer; // 현재 타이머

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        coolTime = defaultCoolTime; // 초기 쿨타임 설정
        timer = coolTime + inputWindow; // 초기 타이머 설정
        UpdateInputCountText();
    }

    void FixedUpdate()
    {
        if (PlayerScript.instance.IsJumping && PlayerScript.instance.jumpCount < 1)
        {
            coolTime = jumpCoolTime; // 점프 중일 때 쿨타임 감소
            if (!hasInputOccurred)
            {
                PlayerScript.instance.FallDown();
            }
        }
        else if (PlayerScript.instance.IsGravityTiming)
        {
            coolTime = Mathf.Infinity; // 중력 변경 중 쿨타임 정지
        }
        else
        {
            coolTime = defaultCoolTime; // 기본 쿨타임으로 재설정
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
                    inputCount++; // 사용자가 입력하지 않으면 입력 횟수 증가
                    if (inputCount >= requiredInputs)
                    {
                        PlayerScript.instance.FallDown();
                        inputCount = 0; // 입력 횟수 초기화
                    }
                }
                canInput = false;
                timer = coolTime + inputWindow; // 타이머 재설정
                hasInputOccurred = false; // 입력 상태 초기화
            }
        }

        UpdateInputCountText(); // 입력 횟수 업데이트
    }

    // 입력 가능 여부를 반환
    public bool CanInput()
    {
        return canInput && !hasInputOccurred;
    }

    // 입력 발생을 기록
    public void RecordInput()
    {
        if (canInput)
        {
            hasInputOccurred = true;
            inputCount = 0; // 입력 발생 시 입력 횟수 리셋
        }
    }

    private void UpdateInputCountText()
    {
        inputCountText.text = inputCount.ToString();
    }
}
