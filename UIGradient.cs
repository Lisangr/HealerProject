using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradient : BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = Color.white;
    [SerializeField]
    private Color32 bottomColor = Color.white;
    [SerializeField]
    private bool useGradientAsOverlay = false;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        UIVertex vertex = default;
        float bottomY = -1f;
        float topY = -1f;

        // Находим верхнюю и нижнюю точки
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            if (bottomY == -1f || vertex.position.y < bottomY)
                bottomY = vertex.position.y;
            if (topY == -1f || vertex.position.y > topY)
                topY = vertex.position.y;
        }

        float uiElementHeight = topY - bottomY;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // Вычисляем процент позиции для градиента
            float gradientFactor = (vertex.position.y - bottomY) / uiElementHeight;
            vertex.color = Color32.Lerp(bottomColor, topColor, gradientFactor);

            if (useGradientAsOverlay)
            {
                // Умножаем цвет градиента на исходный цвет
                Color originalColor = vertex.color;
                vertex.color = new Color32(
                    (byte)((originalColor.r * vertex.color.r) / 255),
                    (byte)((originalColor.g * vertex.color.g) / 255),
                    (byte)((originalColor.b * vertex.color.b) / 255),
                    (byte)((originalColor.a * vertex.color.a) / 255)
                );
            }

            vh.SetUIVertex(vertex, i);
        }
    }

    public void SetColors(Color32 top, Color32 bottom)
    {
        topColor = top;
        bottomColor = bottom;
        if (graphic != null)
            graphic.SetVerticesDirty();
    }
} 