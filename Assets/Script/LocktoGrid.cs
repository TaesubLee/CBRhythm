using UnityEngine;

public class LocktoGrid : MonoBehaviour
{
    public Grid grid; // ����Ƽ ������ ������ Grid ��ü

    // ���� �������� �׸��� ���������� ��ȯ
    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return grid.WorldToCell(worldPosition);
    }

    // �׸��� �������� ���� ���������� ��ȯ�մϴ�.
    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        return grid.CellToWorld(gridPosition);
    }

    // ��ü�� �׸��� ��ġ�� �����մϴ�.
    public void AlignObjectToGrid(GameObject obj)
    {
        if (obj == null) return;

        // ���� ��ü�� ���� ������
        Vector3 worldPosition = obj.transform.position;

        // �׸��� ������ ���
        Vector3Int gridPosition = WorldToGridPosition(worldPosition);

        // �׸��� �������� ���� ���������� ��ȯ�Ͽ� ��ü ��ġ ����
        obj.transform.position = GridToWorldPosition(gridPosition);
    }

    void Update()
    {
        // �׽�Ʈ �뵵��, ��ũ��Ʈ�� ���� ��ü�� �׻� �׸��忡 ����
        AlignObjectToGrid(gameObject);
    }
}
