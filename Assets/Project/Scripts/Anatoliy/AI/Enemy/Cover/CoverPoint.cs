using UnityEngine;

/// <summary>
/// Маркер-компонент на объектах-укрытиях.
/// Без него любой объект на coverLayer может быть укрытием.
/// С ним можно задать точные позиции для сложной геометрии.
/// </summary>
public class CoverPoint : MonoBehaviour
{
    [Tooltip("Точные мировые позиции для сложной геометрии. " +
             "Если пусто позиция определяется автоматически через NavMesh edge.")]
    public Vector3[] CustomPositions;

    [Tooltip("Переопределить высоту укрытия. -1 = брать из bounds.size.y объекта.")]
    public float CoverHeight = -1f;

    public Transform OccupiedBy { get; private set; }
    public bool IsOccupied => OccupiedBy != null;

    public void Occupy(Transform occupant)
    {
        OccupiedBy = occupant;
    }

    public void Release()
    {
        OccupiedBy = null;
    }

    public float GetHeight()
    {
        if (CoverHeight >= 0f) return CoverHeight;

        if (TryGetComponent(out Collider col))
            return col.bounds.size.y;

        return 1f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (CustomPositions == null || CustomPositions.Length == 0) return;

        Gizmos.color = Color.cyan;
        foreach (var pos in CustomPositions)
        {
            Vector3 world = transform.TransformPoint(pos);
            Gizmos.DrawWireSphere(world, 0.25f);
            Gizmos.DrawLine(transform.position, world);
        }
    }
#endif
}
