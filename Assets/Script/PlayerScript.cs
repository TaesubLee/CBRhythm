using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public static PlayerScript instance;

    public float rollSpeed = 2.0f; // �÷��̾ �� ĭ ������ �ӵ�
    private bool isRolling = false; // �÷��̾ ���� ������ �ִ��� Ȯ��
    private bool isJumping = false;
    private bool isCamera = false;
    private bool isFalling = false;


    private Vector3 targetPosition; // ��ǥ ��ġ
    private Quaternion targetRotation; // ��ǥ ȸ��

    public LayerMask groundLayer; // �ٴ� ���̾� ���� (Inspector���� ����)
    private float gravity = 9.8f; // �߷� ��

    public GameObject effectPrefab; // ����Ʈ ������ (Inspector���� ����)

    private bool isGravityTiming = false; // �߷�Ÿ�̸Ӱ� ���� ������ Ȯ��
    private float gravityStartTime;      // ���� �ð� ���
    private float gravityChangeTime;

    public int jumpCount = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // �ִ� ������ ����Ʈ�� 60���� ����
        Application.targetFrameRate = 60;

        AlignToGrid(); // �ʱ� ��ġ ����
    }

    void FixedUpdate()
    {
        // �߷� Ÿ�̸Ӱ� Ȱ��ȭ�� ���� ApplyGravity()�� ��� ȣ��
        if (isGravityTiming || (!IsOnGround() && !isJumping))
        {
            ApplyGravity();
            return;
        }

        if (isGravityTiming || isRolling || !RhythmManager.instance.CanInput()) return;

        if (isCamera)
        {
            // ī�޶� ����� ���� Ű �Է�
            HandleCameraInput();
        }
        else
        {
            // �Ϲ� ����� ���� Ű �Է�
            HandleNormalInput();
        }
    }
    void HandleCameraInput()
    {
        if (Input.GetKeyDown(KeyCode.A)) Roll(Vector3.forward);
        else if (Input.GetKeyDown(KeyCode.W)) Roll(Vector3.right);
        else if (Input.GetKeyDown(KeyCode.X)) Roll(Vector3.left);
        else if (Input.GetKeyDown(KeyCode.D)) Roll(Vector3.back);
    }

    void HandleNormalInput()
    {
        if (isJumping)
        {
            // ���� ��忡���� Ű �Է�
            if (Input.GetKeyDown(KeyCode.Q))
            {
                jumpCount++;
                Jump(Vector3.forward);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                jumpCount++;
                Jump(Vector3.right);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                jumpCount++;
                Jump(Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                jumpCount++;
                Jump(Vector3.back);
            }
        }

        // �Ϲ� �̵� Ű �Է�
        if (Input.GetKeyDown(KeyCode.Q)) Roll(Vector3.forward);
        else if (Input.GetKeyDown(KeyCode.E)) Roll(Vector3.right);
        else if (Input.GetKeyDown(KeyCode.Z)) Roll(Vector3.left);
        else if (Input.GetKeyDown(KeyCode.C)) Roll(Vector3.back);
    }
    void Roll(Vector3 direction)
    {
        isRolling = true;

        // �Է� �߻� ���
        RhythmManager.instance.RecordInput();

        // ��ǥ ��ġ �� ȸ�� ���
        targetPosition = transform.position + direction;
        Vector3 axis = Vector3.Cross(gravity > 0 ? Vector3.up : Vector3.down, direction); // ȸ�� �� ���
        Vector3 pivotPoint = CalculatePivotPoint(direction); // ȸ�� �߽� ���

        StartCoroutine(RollCoroutine(pivotPoint, axis));
    }

    void Jump(Vector3 direction)
    {
        // �Է� �߻� ���
        RhythmManager.instance.RecordInput();

        // ��ǥ ��ġ �� ȸ�� ���
        targetPosition = transform.position + direction;
        Vector3 axis = Vector3.Cross(gravity > 0 ? Vector3.up : Vector3.down, direction); // ȸ�� �� ���
        Vector3 pivotPoint = CalculatePivotPoint(direction); // ȸ�� �߽� ���

        StartCoroutine(JumpCoroutine(pivotPoint, axis));
    }

    Vector3 CalculatePivotPoint(Vector3 direction)
    {
        if (isJumping)
        {
            return transform.position + new Vector3(
                direction.x * 0.5f,
                gravity > 0 ? -0.25f : 0.25f,
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
        float targetAngle = 90; // isJumping�̸� 180��, �ƴϸ� 90��

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
    System.Collections.IEnumerator JumpCoroutine(Vector3 pivotPoint, Vector3 axis)
    {
        float rollAngle = 0;
        float targetAngle = 90; // isJumping�̸� 180��, �ƴϸ� 90��

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

    public bool IsOnGround()
    {
        if (isFalling)
        {
            gravity = 9.8f;
            return false;
        }
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
        if (other.CompareTag("GravityItem") && !isGravityTiming)
        {
            isGravityTiming = true;         // Ÿ�̸� ����
            gravityStartTime = Time.time;   // ���� �ð��� ���
            gravity = -gravity;
            gravity = gravity > 0 ? 1.0f : -1.0f;
        }
        if (other.CompareTag("GraivtyArrive") && isGravityTiming)
        {
            isGravityTiming = false;        // Ÿ�̸� ����
            float endTime = Time.time;
            gravityChangeTime = endTime - gravityStartTime; // ��� �ð� ���
            Debug.Log($"Elapsed Time: {gravityChangeTime} seconds");
        }

        if (other.CompareTag("JumpItem") && !isJumping && jumpCount < 1)
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
        yield return new WaitForSeconds(2f);
        isJumping = false;
        jumpCount = 0;
    }
    public void FallDown()
    {
        isFalling = true;
    }
    // Gravity ���� �ܺο��� ���� �� �ֵ��� ����
    public float Gravity
    {
        get { return gravity; }
    }
    public bool IsJumping
    {
        get { return isJumping; }
    }
    public bool IsRolling
    {
        get { return isRolling; }
    }
    public bool IsGravityTiming
    {
        get { return isGravityTiming; }
    }
}