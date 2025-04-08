using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetStaticAndBatch : MonoBehaviour
{
    void OnEnable()
    {
        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Вызывается после загрузки любой сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Запускаем корутину, чтобы немного подождать, пока объекты полностью инициализируются
        StartCoroutine(DelayedCombine());
    }

    private IEnumerator DelayedCombine()
    {
        // Ждем один кадр (можно увеличить задержку, если нужно)
        yield return new WaitForEndOfFrame();
        // Выполняем комбинирование статичных объектов
        StaticBatchingUtility.Combine(gameObject);
    }
}
