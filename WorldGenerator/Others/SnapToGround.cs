#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SnapToGround : MonoBehaviour
{
    public GameObject ground;
    public LayerMask groundLayer = ~0; // Добавляем маску слоев
    public float maxDistance = 20f; // Увеличиваем дистанцию

    public void SnapAllChildren()
    {
        if (ground == null)
        {
            Debug.LogError("Ground object is not assigned!");
            return;
        }

        foreach (Transform child in transform)
        {
            SnapToGroundSurface(child);
        }
    }

    private void SnapToGroundSurface(Transform child)
    {
        Vector3 position = child.position;
        RaycastHit hit;

        // 1. Попытка сверху
        if (CastWithDebug(new Ray(position + Vector3.up * maxDistance, Vector3.down), out hit, maxDistance * 2))
        {
            if (IsValidHit(hit))
            {
                ApplySnap(child, hit.point, hit.normal);
                return;
            }
        }

        // 2. Попытка снизу
        if (CastWithDebug(new Ray(position + Vector3.down * maxDistance, Vector3.up), out hit, maxDistance * 2))
        {
            if (IsValidHit(hit))
            {
                ApplySnap(child, hit.point, hit.normal);
                return;
            }
        }

        // 3. Попытка локального луча
        if (Physics.Raycast(position, Vector3.down, out hit, maxDistance, groundLayer))
        {
            if (IsValidHit(hit))
            {
                ApplySnap(child, hit.point, hit.normal);
                return;
            }
        }

        Debug.LogWarning($"FAILED: {child.name} | Pos: {position} | Ground: {ground.name}");
    }

    private bool CastWithDebug(Ray ray, out RaycastHit hit, float distance)
    {
        bool result = Physics.Raycast(ray, out hit, distance, groundLayer);
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, result ? Color.green : Color.red, 2f);
        return result;
    }

    private bool IsValidHit(RaycastHit hit)
    {
        return hit.collider != null &&
             (hit.collider.gameObject == ground ||
              hit.collider.transform.IsChildOf(ground.transform));
    }

    private void ApplySnap(Transform child, Vector3 position, Vector3 normal)
    {
        Undo.RecordObject(child, "Snap to Ground");
        child.position = position;
        // Поворачиваем объект так, чтобы его локальная ось "up" совпадала с нормалью поверхности
        child.rotation = Quaternion.FromToRotation(child.up, normal) * child.rotation;
    }
}

[CustomEditor(typeof(SnapToGround))]
public class SnapToGroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SnapToGround script = (SnapToGround)target;
        if (GUILayout.Button("Зафиксировать детей на земле"))
        {
            script.SnapAllChildren();
        }
    }
}
#endif
