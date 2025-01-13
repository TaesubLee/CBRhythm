using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float rollSpeed = 2.0f; // �÷��̾ �� ĭ ������ �ӵ�
    private bool isRolling = false; // �÷��̾ ���� ������ �ִ��� Ȯ��
    private bool isJumping = false;
    bool isCamera = false;


    private Vector3 targetPosition; // ��ǥ ��ġ
    private Quaternion targetRotation; // ��ǥ ȸ��

    public LayerMask groundLayer; // �ٴ� ���̾� ���� (Inspector���� ����)
    private float gravity = 9.8f; // �߷� ��

    public GameObject effectPrefab; // ����Ʈ ������ (Inspector���� ����)

    void Start()
    {
        AlignToGrid(); // �ʱ� ��ġ ����
    }

    void Update()
    {
        if (isRolling && isJumping) return; // ������ ���� ���� �Է� ����
        if (isCamera)
        {
            // QEZC Ű �Է� ���� �� �̵�
            if (Input.GetKeyDown(KeyCode.A)) Roll(Vector3.forward); // Z�� + ����
            else if (Input.GetKeyDown(KeyCode.W)) Roll(Vector3.right); // X�� + ����
            else if (Input.GetKeyDown(KeyCode.X)) Roll(Vector3.left); // X�� - ����
            else if (Input.GetKeyDown(KeyCode.D)) Roll(Vector3.back); // Z�� - ����
        }
        else
        {
            // QEZC Ű �Է� ���� �� �̵�
            if (Input.GetKeyDown(KeyCode.Q)) Roll(Vector3.forward); // Z�� + ����
            else if (Input.GetKeyDown(KeyCode.E)) Roll(Vector3.right); // X�� + ����
            else if (Input.GetKeyDown(KeyCode.Z)) Roll(Vector3.left); // X�� - ����
            else if (Input.GetKeyDown(KeyCode.C)) Roll(Vector3.back); // Z�� - ����
        }

        // �߷� ó��
        if (!IsOnGround())
        {
            ApplyGravity();
        }
    }

    void Roll(Vector3 direction)
    {
        isRolling = true;

        // ��ǥ ��ġ �� ȸ�� ���
        targetPosition = transform.position + direction;
        Vector3 axis = Vector3.Cross(gravity > 0 ? Vector3.up : Vector3.down, direction); // ȸ�� �� ���
        Vector3 pivotPoint = CalculatePivotPoint(direction); // ȸ�� �߽� ���

        StartCoroutine(RollCoroutine(pivotPoint, axis));
    }

    Vector3 CalculatePivotPoint(Vector3 direction)
    {
        if (isJumping)
        {
            return transform.position + new Vector3(
                direction.x * 0.5f,
                gravity > 0 ? -0.5f : 0.5f,
                direction.z * 0.5f
            ) + direction * 0.5f;
        }
        else
        {
            return transform.position + new Vector3(
                direction.x * 0.5f,
                gravity > 0 ? -0.5f : 0.5f,
                direction.z * 0.5f
            );
        }
    }

    System.Collections.IEnumerator RollCoroutine(Vector3 pivotPoint, Vector3 axis)
    {
        float rollAngle = 0;
        float targetAngle = isJumping ? 135 : 90; // isJumping�̸� 180��, �ƴϸ� 90��

        while (rollAngle < targetAngle)
        {
            float angleStep = rollSpeed * Time.deltaTime * 90; // ������ �� ȸ����
            rollAngle += angleStep;
            if (rollAngle > targetAngle) angleStep -= (rollAngle - targetAngle);

            transform.RotateAround(pivotPoint, axis, angleStep);

            yield return null;
        }

        // �̵� �Ϸ� �� ��ġ �� ȸ�� ����
        AlignToGrid();
        // ����Ʈ ����
        SpawnEffect(transform.position);

        isRolling = false;
    }
    void AlignToGrid()
    {
        // ��ġ�� 0.5 ������ ����
        transform.position = new Vector3(
            RoundToHalf(transform.position.x),
            Mathf.Round(transform.position.y), // Y���� ������ ����
            RoundToHalf(transform.position.z)
        );

        // ȸ���� �ʱ� ���·� ����
        transform.rotation = Quaternion.identity;
    }
    void SpawnEffect(Vector3 position)
    {
        // ����Ʈ�� �ٴڿ� ��ġ�ϵ��� Y ��ǥ�� ����
        Vector3 effectPosition = new Vector3(
            position.x,         // X ��ǥ�� �÷��̾��� X ��ġ
            gravity > 0 ? 0.51f : 4.51f,               // Y ��ǥ�� �ٴ� ���̷� ���� (0.5f)
            position.z          // Z ��ǥ�� �÷��̾��� Z ��ġ
        );

        // �̵��� ��ġ�� ����Ʈ�� ����
        GameObject effect = Instantiate(effectPrefab, effectPosition, Quaternion.Euler(90, 0, 0));
        Destroy(effect, 1.0f); // ����Ʈ�� 1�� �� �ڵ����� �����ǵ��� ����
    }

    // 0.5 ������ ��ġ�� �����ϴ� �Լ�
    private float RoundToHalf(float value)
    {
        // ���� 0.5 ������ �����ǵ��� ��ȯ
        return Mathf.Round(value * 2) / 2.0f;
    }

    private bool IsOnGround()
    {
        // �ٴ� ���� (Raycast Ȱ��)
        return Physics.Raycast(transform.position, gravity > 0 ? Vector3.down : Vector3.up, 1f, groundLayer);
    }

    void ApplyGravity()
    {
        // �߷� ���� (�������� Y�� ����)
        transform.position += Vector3.down * gravity * Time.deltaTime;

        // ���� �� ��ġ ����
        if (IsOnGround())
        {
            AlignToGrid();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // floor ���̾�� �浹 �� jumpCount �ʱ�ȭ
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            gravity = gravity > 0 ? 9.8f : -9.8f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // CameraFollow ��ũ��Ʈ ȣ���Ͽ� ī�޶� ���� ����
        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
        // GravityItem �±׸� ���� ������Ʈ�� �浹 ��
        if (other.CompareTag("GravityItem"))
        {
            gravity = -gravity;
            gravity = gravity > 0 ? 1.0f : -1.0f;
        }
        if (other.CompareTag("JumpItem"))
        {
            isJumping = true;
            StartCoroutine(JumpReset());
        }
        if (cameraFollow != null)
        {
            if (other.CompareTag("CameraItem"))
            {
                if (!isCamera)
                {
                    // ī�޶� ���� ���� (45, 90, 0) / Size 3
                    cameraFollow.SetCameraView(new Vector3(45, 90, 0), 3f);
                    isCamera = true; // ���� ������Ʈ
                }
            }
            else if (other.CompareTag("CameraItemRe"))
            {
                if (isCamera)
                {
                    // ī�޶� ���� �ʱ�ȭ (30, 45, 0) / Size 5
                    cameraFollow.SetCameraView(new Vector3(30, 45, 0), 5f);
                    isCamera = false; // ���� ������Ʈ
                }
            }
        }
    }
    IEnumerator JumpReset()
    {
        yield return new WaitForSeconds(1f);
        isJumping = false;
    }
    // Gravity ���� �ܺο��� ���� �� �ֵ��� ����
    public float Gravity
    {
        get { return gravity; }
    }

}
