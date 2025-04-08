using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleMultiplier : MonoBehaviour
{
    // Метод для увеличения масштаба объекта
    public void MultiplyScale(float multiplier)
    {
        transform.localScale *= multiplier;
    }
}
