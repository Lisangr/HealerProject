using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleMultiplier : MonoBehaviour
{
    // ����� ��� ���������� �������� �������
    public void MultiplyScale(float multiplier)
    {
        transform.localScale *= multiplier;
    }
}
