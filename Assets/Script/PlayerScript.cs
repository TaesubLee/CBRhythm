using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float rollSpeed = 2.0f; // 플레이어가 한 칸 구르는 속도
    private bool isRolling = false; // 플레이어가 현재 구르고 있는지 확인

    private Vector3 targetPosition; // 목표 위치
    private Quaternion targetRotation; // 목표 회전

    public LayerMask groundLayer; // 바닥 레이어 지정 (Inspector에서 설정)
    private float gravity = 9.8f; // 중력 값

    void Start()
    {
        AlignToGrid(); // 초기 위치 정렬
    }

    void Update()
    {
        if (isRolling) return; // 구르는 중일 때는 입력 차단

        // QEZC 키 입력 감지 및 이동
        if (Input.GetKeyDown(KeyCode.Q)) Roll(Vector3.forward); // Z축 + 방향
        else if (Input.GetKeyDown(KeyCode.E)) Roll(Vector3.right); // X축 + 방향
        else if (Input.GetKeyDown(KeyCode.Z)) Roll(Vector3.left); // X축 - 방향
        else if (Input.GetKeyDown(KeyCode.C)) Roll(Vector3.back); // Z축 - 방향

        // 중력 처리
        if (!IsOnGround())
        {
            ApplyGravity();
        }
    }

    void Roll(Vector3 direction)
    {
        isRolling = true;

        // 목표 위치 및 회전 계산
        targetPosition = transform.position + direction;
        Vector3 axis = Vector3.Cross(gravity > 0 ? Vector3.up : Vector3.down, direction); // 회전 축 계산
        Vector3 pivotPoint = CalculatePivotPoint(direction); // 회전 중심 계산

        StartCoroutine(RollCoroutine(pivotPoint, axis));
    }

    Vector3 CalculatePivotPoint(Vector3 direction)
    {
        return transform.position + new Vector3(
            direction.x * 0.5f,
            gravity > 0 ? -0.5f : 0.5f,
            direction.z * 0.5f
        );
    }

    System.Collections.IEnumerator RollCoroutine(Vector3 pivotPoint, Vector3 axis)
    {
        float rollAngle = 0;

        while (rollAngle < 90)
        {
            float angleStep = rollSpeed * Time.deltaTime * 90; // 프레임 당 회전각
            rollAngle += angleStep;
            if (rollAngle > 90) angleStep -= (rollAngle - 90);

            transform.RotateAround(pivotPoint, axis, angleStep);

            yield return null;
        }

        // 이동 완료 후 위치 및 회전 보정
        AlignToGrid();
        isRolling = false;
    }
    void AlignToGrid()
    {
        // 위치를 0.5 단위로 정렬
        transform.position = new Vector3(
            RoundToHalf(transform.position.x),
            Mathf.Round(transform.position.y), // Y축은 정수로 유지
            RoundToHalf(transform.position.z)
        );

        // 회전을 초기 상태로 강제
        transform.rotation = Quaternion.identity;
    }

    // 0.5 단위로 위치를 정렬하는 함수
    private float RoundToHalf(float value)
    {
        // 값이 0.5 단위로 보정되도록 변환
        return Mathf.Round(value * 2) / 2.0f;
    }

    private bool IsOnGround()
    {
        // 바닥 감지 (Raycast 활용)
        return Physics.Raycast(transform.position, gravity > 0 ? Vector3.down : Vector3.up, 1f, groundLayer);
    }

    void ApplyGravity()
    {
        // 중력 적용 (수동으로 Y축 감소)
        transform.position += Vector3.down * gravity * Time.deltaTime;

        // 낙하 후 위치 정렬
        if (IsOnGround())
        {
            AlignToGrid();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // GravityItem 태그를 가진 오브젝트와 충돌 시
        if (other.CompareTag("GravityItem"))
        {
            gravity = -gravity;
        }
    }
}
