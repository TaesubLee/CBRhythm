using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // �÷��̾� Transform
    public Vector3 offset = new Vector3(0, 2, 0); // CameraPivot�� �ʱ� ����� ��ġ
    public float rotationSpeed = 2f; // ȸ�� ��ȯ �ӵ�
    public float offsetTransitionSpeed = 2f; // offset ��ȯ �ӵ�
    public float transitionSpeed = 2f; // Lerp ��ȯ �ӵ�

    private float targetRotationX = 30f; // ��ǥ X�� ȸ����
    private float targetOffsetY = 2f; // ��ǥ offset Y��
    private PlayerScript playerScript; // PlayerScript ����

    private Coroutine transitionCoroutine; // ī�޶� ��ȯ �ڷ�ƾ ����
    void Start()
    {
        // �÷��̾� ��ũ��Ʈ ���� ��������
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerScript>();
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // �߷� ���¿� ���� ��ǥ �� ����
            if (playerScript != null)
            {
                targetRotationX = playerScript.Gravity > 0 ? 30f : -30f;
                targetOffsetY = playerScript.Gravity > 0 ? 2f : -2f;
            }

            // CameraPivot ��ġ ������Ʈ
            Vector3 currentOffset = offset;
            currentOffset.y = Mathf.Lerp(offset.y, targetOffsetY, Time.deltaTime * offsetTransitionSpeed);
            offset = currentOffset;

            transform.position = player.position + offset;

            // CameraPivot�� X�� ȸ���� �ε巴�� ��ȯ
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