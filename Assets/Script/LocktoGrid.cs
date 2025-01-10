using UnityEngine;

public class LocktoGrid : MonoBehaviour
{
    public Grid grid; // 유니티 씬에서 연결할 Grid 객체

    // 월드 포지션을 그리드 포지션으로 변환
    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return grid.WorldToCell(worldPosition);
    }

    // 그리드 포지션을 월드 포지션으로 변환합니다.
    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        return grid.CellToWorld(gridPosition);
    }

    // 객체를 그리드 위치에 정렬합니다.
    public void AlignObjectToGrid(GameObject obj)
    {
        if (obj == null) return;

        // 현재 객체의 월드 포지션
        Vector3 worldPosition = obj.transform.position;

        // 그리드 포지션 계산
        Vector3Int gridPosition = WorldToGridPosition(worldPosition);

        // 그리드 포지션을 월드 포지션으로 변환하여 객체 위치 설정
        obj.transform.position = GridToWorldPosition(gridPosition);
    }

    void Update()
    {
        // 테스트 용도로, 스크립트가 붙은 객체를 항상 그리드에 정렬
        AlignObjectToGrid(gameObject);
    }
}
