using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // 플레이어 Transform
    public Vector3 offset = new Vector3(0, 2, 0); // CameraPivot의 초기 상대적 위치
    public float rotationSpeed = 2f; // 회전 전환 속도
    public float offsetTransitionSpeed = 2f; // offset 전환 속도
    public float transitionSpeed = 2f; // Lerp 전환 속도

    private float targetRotationX = 30f; // 목표 X축 회전값
    private float targetOffsetY = 2f; // 목표 offset Y값
    private PlayerScript playerScript; // PlayerScript 참조

    private Coroutine transitionCoroutine; // 카메라 전환 코루틴 참조
    void Start()
    {
        // 플레이어 스크립트 참조 가져오기
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerScript>();
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // 중력 상태에 따라 목표 값 설정
            if (playerScript != null)
            {
                targetRotationX = playerScript.Gravity > 0 ? 30f : -30f;
                targetOffsetY = playerScript.Gravity > 0 ? 2f : -2f;
            }

            // CameraPivot 위치 업데이트
            Vector3 currentOffset = offset;
            currentOffset.y = Mathf.Lerp(offset.y, targetOffsetY, Time.deltaTime * offsetTransitionSpeed);
            offset = currentOffset;

            transform.position = player.position + offset;

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