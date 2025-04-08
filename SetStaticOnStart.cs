using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetStaticAndBatch : MonoBehaviour
{
    void OnEnable()
    {
        // ������������� �� ������� �������� �����
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ���������� ����� �������� ����� �����
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ��������� ��������, ����� ������� ���������, ���� ������� ��������� ����������������
        StartCoroutine(DelayedCombine());
    }

    private IEnumerator DelayedCombine()
    {
        // ���� ���� ���� (����� ��������� ��������, ���� �����)
        yield return new WaitForEndOfFrame();
        // ��������� �������������� ��������� ��������
        StaticBatchingUtility.Combine(gameObject);
    }
}
