using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // 플레이어 Transform
    private Vector3 offset = new Vector3(0, 2, 0); // CameraPivot의 초기 상대적 위치
    private float rotationSpeed = 2f; // 회전 전환 속도
    private float offsetTransitionSpeed = 2f; // offset 전환 속도
    private float transitionSpeed = 2f; // Lerp 전환 속도

    private float targetRotationX = 30f; // 목표 X축 회전값
    private float targetOffsetY = 2f; // 목표 offset Y값

    private Coroutine transitionCoroutine; // 카메라 전환 코루틴 참조
    private Vector3 lastGroundedPosition; // 플레이어가 마지막으로 땅에 있었던 위치

    void LateUpdate()
    {
        if (player != null)
        {
            if (PlayerScript.instance != null && PlayerScript.instance.IsOnGround())
            {
                lastGroundedPosition = player.position; // 땅에 있을 때마다 마지막 위치 업데이트
            }

            // 중력 상태에 따라 목표 값 설정
            targetRotationX = PlayerScript.instance.Gravity > 0 ? 30f : -30f;
            targetOffsetY = PlayerScript.instance.Gravity > 0 ? 2f : -2f;

            // CameraPivot 위치 업데이트
            Vector3 currentOffset = offset;
            currentOffset.y = Mathf.Lerp(offset.y, targetOffsetY, Time.deltaTime * offsetTransitionSpeed);
            offset = currentOffset;

            // 플레이어가 땅에 있지 않은 경우 마지막 땅 위치를 사용
            Vector3 effectivePosition = (PlayerScript.instance.IsOnGround() || PlayerScript.instance.IsGravityTiming) ? player.position : lastGroundedPosition;
            transform.position = effectivePosition + offset;

            // CameraPivot의 X축 회전을 부드럽게 전환
            float currentRotationX = transform.eulerAngles.x;
            float newRotationX = Mathf.LerpAngle(currentRotationX, targetRotationX, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(newRotationX, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }
    public void SetCameraView(Vector3 targetRotation, float targetSize)
    {
        if(transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(CameraTransition(targetRotation, targetSize));
    }
    private IEnumerator CameraTransition(Vector3 targetRotation, float targetSize)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);

        float startSize = mainCamera.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;

            // Lerp for rotation and size
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsedTime);

            yield return null;
        }

        // Ensure final values are set
        transform.rotation = endRotation;
        mainCamera.orthographicSize = targetSize;
    }
}