using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float rollSpeed = 2.0f; // �÷��̾ �� ĭ ������ �ӵ�
    private bool isRolling = false; // �÷��̾ ���� ������ �ִ��� Ȯ��

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
        if (isRolling) return; // ������ ���� ���� �Է� ����

        // QEZC Ű �Է� ���� �� �̵�
        if (Input.GetKeyDown(KeyCode.Q)) Roll(Vector3.forward); // Z�� + ����
        else if (Input.GetKeyDown(KeyCode.E)) Roll(Vector3.right); // X�� + ����
        else if (Input.GetKeyDown(KeyCode.Z)) Roll(Vector3.left); // X�� - ����
        else if (Input.GetKeyDown(KeyCode.C)) Roll(Vector3.back); // Z�� - ����

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
            float angleStep = rollSpeed * Time.deltaTime * 90; // ������ �� ȸ����
            rollAngle += angleStep;
            if (rollAngle > 90) angleStep -= (rollAngle - 90);

            transform.RotateAround(pivotPoint, axis, angleStep);

            yield return null;
        }

        // �̵� �Ϸ� �� ��ġ �� ȸ�� ����
        AlignToGrid();
        //����Ʈ ����
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
            gravity > 0 ? 0.51f : 3.51f,               // Y ��ǥ�� �ٴ� ���̷� ���� (0.5f)
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
    private void OnTriggerEnter(Collider other)
    {
        // GravityItem �±׸� ���� ������Ʈ�� �浹 ��
        if (other.CompareTag("GravityItem"))
        {
            gravity = -gravity;
        }
    }
}
