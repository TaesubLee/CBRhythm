using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager instance; // 싱글톤 인스턴스

    private bool canInput = false; // 입력 가능 여부
    private bool hasInputOccurred = false; // 입력 발생 여부
    private float coolTime = 0.5f; // 쿨타임
    private float inputWindow = 0.5f; // 입력 가능 시간

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(RhythmCycle());
    }

    // 리듬 주기 관리
    IEnumerator RhythmCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(coolTime);
            canInput = true; // 입력 가능 상태 활성화
            hasInputOccurred = false; // 입력 상태 초기화

            yield return new WaitForSeconds(inputWindow);
            canInput = false; // 입력 가능 상태 비활성화
        }
    }

    // 입력 가능 여부 확인
    public bool CanInput()
    {
        // isJumping이 true면 바로 입력 가능, 아니면 일반 로직
        if ((PlayerScript.instance.IsJumping && PlayerScript.instance.jumpCount < 1) || PlayerScript.instance.IsGravityTiming || !PlayerScript.instance.IsOnGround())
        {
            return true;
        }

        return canInput && !hasInputOccurred;
    }

    // 입력 발생 기록
    public void RecordInput()
    {
        hasInputOccurred = true;
    }
}
