using UnityEngine;
using System;

public class ProjectileMoveScript : MonoBehaviour
{
    [Header("��������� ��������")]
    [Tooltip("�������� �������� �������")]
    public float speed = 20f;

    [Tooltip("����� ����� ������� (� ��������)")]
    public float lifeTime = 5f;

    [Tooltip("�������� �������� (��� �������� ���������)")]
    public float rotateSpeed = 10f;

    [Tooltip("����, � ������� ������������ ������ (���� �� ������, �������� �� ����������� transform.forward)")]
    public Transform target;

    [Tooltip("���� ��������, ������ ����� ������ �������������� � ������� ����")]
    public bool homing = false;

    // ������� ��� �������� ������� � ���
    public Action<ProjectileMoveScript> ReturnToPoolCallback;

    // ����� ��������� �������
    private float spawnTime;

    /// <summary>
    /// �����, ������� ���������� ��� ��������� �������. ���������� ����� ������.
    /// </summary>
    private void OnEnable()
    {
        spawnTime = Time.time;
    }

    /// <summary>
    /// �������� ����� ����������. ������� ������ � ����������� ���� ��� �� transform.forward.
    /// ��� ��������� ������� ����� ������ ������������ � ���.
    /// </summary>
    private void Update()
    {
        // ���� ����� ����� ������� � ���������� ������ � ���.
        if (Time.time > spawnTime + lifeTime)
        {
            ReturnToPool();
            return;
        }

        Vector3 moveDirection;

        if (target != null)
        {
            // ��������� ����������� � ����
            Vector3 targetDirection = (target.position - transform.position).normalized;

            if (homing)
            {
                // ������� ������� ������� � ������� ����
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, rotateSpeed * Time.deltaTime, 0f);
                moveDirection = newDir.normalized;
                transform.rotation = Quaternion.LookRotation(newDir);
            }
            else
            {
                // ���������� ����������� � ������� ����
                moveDirection = targetDirection;
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
        else
        {
            // ���� ���� ���, �������� �� ����������� transform.forward
            moveDirection = transform.forward;
        }

        // ���������� ������
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    /// <summary>
    /// ����� ��������� ������������.
    /// ����� ����� �������� �������������� �������� (��������, ��������� ������������ � �������).
    /// ����� ������������ ������ ������������ � ���.
    /// </summary>
    /// <param name="collision">���������� � ������������</param>
    private void OnCollisionEnter(Collision collision)
    {
        ReturnToPool();
    }

    /// <summary>
    /// ����� ���������� ������ � ���.
    /// ���� ������� ReturnToPoolCallback �����, �� ���������� ��� �������� ������� � ���,
    /// ����� ������ ������ ��������������.
    /// </summary>
    public void ReturnToPool()
    {
        // ���������� ����, ����� �������� ����������� ���������
        target = null;

        if (ReturnToPoolCallback != null)
        {
            ReturnToPoolCallback(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// ������������� ���� ��� �������.
    /// ����� ��������� ������ Enemy (��� ����� ������ ������, ���������� Transform)
    /// � ��������� ��� � �������� ���� ��� ���������.
    /// </summary>
    /// <param name="enemy">����, � ������� ������ ������ ������</param>
    public void SetTarget(Enemy enemy)
    {
        if (enemy != null)
        {
            target = enemy.transform;
        }
        else
        {
            target = null;
        }
    }
}
