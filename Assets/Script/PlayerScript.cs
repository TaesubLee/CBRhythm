using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public static PlayerScript instance;

    public float rollSpeed = 2.0f; // 플레이어가 한 칸 구르는 속도
    private bool isRolling = false; // 플레이어가 현재 구르고 있는지 확인
    private bool isJumping = false;
    private bool isCamera = false;
    private bool isFalling = false;


    private Vector3 targetPosition; // 목표 위치
    private Quaternion targetRotation; // 목표 회전

    public LayerMask groundLayer; // 바닥 레이어 지정 (Inspector에서 설정)
    private float gravity = 9.8f; // 중력 값

    public GameObject effectPrefab; // 이펙트 프리팹 (Inspector에서 연결)

    private bool isGravityTiming = false; // 중력타이머가 동작 중인지 확인
    private float gravityStartTime;      // 시작 시간 기록
    private float gravityChangeTime;

    public int jumpCount = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 최대 프레임 레이트를 60으로 설정
        Application.targetFrameRate = 60;

        AlignToGrid(); // 초기 위치 정렬
    }

    void FixedUpdate()
    {
        // 중력 타이머가 활성화된 동안 ApplyGravity()를 계속 호출
        if (isGravityTiming || (!IsOnGround() && !isJumping))
        {
            ApplyGravity();
            return;
        }

        if (isGravityTiming || isRolling || !RhythmManager.instance.CanInput()) return;

        if (isCamera)
        {
            // 카메라 모드일 때의 키 입력
            HandleCameraInput();
        }
        else
        {
            // 일반 모드일 때의 키 입력
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
            // 점프 모드에서의 키 입력
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

        // 일반 이동 키 입력
        if (Input.GetKeyDown(KeyCode.Q)) Roll(Vector3.forward);
        else if (Input.GetKeyDown(KeyCode.E)) Roll(Vector3.right);
        else if (Input.GetKeyDown(KeyCode.Z)) Roll(Vector3.left);
        else if (Input.GetKeyDown(KeyCode.C)) Roll(Vector3.back);
    }
    void Roll(Vector3 direction)
    {
        isRolling = true;

        // 입력 발생 기록
        RhythmManager.instance.RecordInput();

        // 목표 위치 및 회전 계산
        targetPosition = transform.position + direction;
        Vector3 axis = Vector3.Cross(gravity > 0 ? Vector3.up : Vector3.down, direction); // 회전 축 계산
        Vector3 pivotPoint = CalculatePivotPoint(direction); // 회전 중심 계산

        StartCoroutine(RollCoroutine(pivotPoint, axis));
    }

    void Jump(Vector3 direction)
    {
        // 입력 발생 기록
        RhythmManager.instance.RecordInput();

        // 목표 위치 및 회전 계산
        targetPosition = transform.position + direction;
        Vector3 axis = Vector3.Cross(gravity > 0 ? Vector3.up : Vector3.down, direction); // 회전 축 계산
        Vector3 pivotPoint = CalculatePivotPoint(direction); // 회전 중심 계산

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
        float targetAngle = 90; // isJumping이면 180도, 아니면 90도

        while (rollAngle < targetAngle)
        {
            float angleStep = rollSpeed * Time.deltaTime * 90; // 프레임 당 회전각
            rollAngle += angleStep;
            if (rollAngle > targetAngle) angleStep -= (rollAngle - targetAngle);

            transform.RotateAround(pivotPoint, axis, angleStep);

            yield return null;
        }

        // 이동 완료 후 위치 및 회전 보정
        AlignToGrid();
        // 이펙트 생성
        SpawnEffect(transform.position);

        isRolling = false;
    }
    System.Collections.IEnumerator JumpCoroutine(Vector3 pivotPoint, Vector3 axis)
    {
        float rollAngle = 0;
        float targetAngle = 90; // isJumping이면 180도, 아니면 90도

        while (rollAngle < targetAngle)
        {
            float angleStep = rollSpeed * Time.deltaTime * 90; // 프레임 당 회전각
            rollAngle += angleStep;
            if (rollAngle > targetAngle) angleStep -= (rollAngle - targetAngle);

            transform.RotateAround(pivotPoint, axis, angleStep);

            yield return null;
        }

        // 이동 완료 후 위치 및 회전 보정
        AlignToGrid();
        // 이펙트 생성
        SpawnEffect(transform.position);
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
    void SpawnEffect(Vector3 position)
    {
        // 이펙트를 바닥에 위치하도록 Y 좌표를 고정
        Vector3 effectPosition = new Vector3(
            position.x,         // X 좌표는 플레이어의 X 위치
            gravity > 0 ? 0.51f : 4.51f,               // Y 좌표를 바닥 높이로 고정 (0.5f)
            position.z          // Z 좌표는 플레이어의 Z 위치
        );

        // 이동한 위치에 이펙트를 생성
        GameObject effect = Instantiate(effectPrefab, effectPosition, Quaternion.Euler(90, 0, 0));
        Destroy(effect, 1.0f); // 이펙트가 1초 후 자동으로 삭제되도록 설정
    }

    // 0.5 단위로 위치를 정렬하는 함수
    private float RoundToHalf(float value)
    {
        // 값이 0.5 단위로 보정되도록 변환
        return Mathf.Round(value * 2) / 2.0f;
    }

    public bool IsOnGround()
    {
        if (isFalling)
        {
            gravity = 9.8f;
            return false;
        }
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
    private void OnCollisionEnter(Collision collision)
    {
        // floor 레이어와 충돌 시 jumpCount 초기화
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            gravity = gravity > 0 ? 9.8f : -9.8f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // CameraFollow 스크립트 호출하여 카메라 설정 변경
        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();

        // GravityItem 태그를 가진 오브젝트와 충돌 시
        if (other.CompareTag("GravityItem") && !isGravityTiming)
        {
            isGravityTiming = true;         // 타이머 시작
            gravityStartTime = Time.time;   // 현재 시간을 기록
            gravity = -gravity;
            gravity = gravity > 0 ? 1.0f : -1.0f;
        }
        if (other.CompareTag("GraivtyArrive") && isGravityTiming)
        {
            isGravityTiming = false;        // 타이머 종료
            float endTime = Time.time;
            gravityChangeTime = endTime - gravityStartTime; // 경과 시간 계산
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
                    // 카메라 시점 변경 (45, 90, 0) / Size 3
                    cameraFollow.SetCameraView(new Vector3(45, 90, 0), 3f);
                    isCamera = true; // 상태 업데이트
                }
            }
            else if (other.CompareTag("CameraItemRe"))
            {
                if (isCamera)
                {
                    // 카메라 시점 초기화 (30, 45, 0) / Size 5
                    cameraFollow.SetCameraView(new Vector3(30, 45, 0), 5f);
                    isCamera = false; // 상태 업데이트
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
    // Gravity 값을 외부에서 읽을 수 있도록 제공
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