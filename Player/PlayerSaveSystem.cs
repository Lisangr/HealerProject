using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveSystem : MonoBehaviour
{
    // ����������� ������ �������, ������� ����� �������� �� ���� ������
    public static List<PlayerSnapshot> StaticSnapshots = new List<PlayerSnapshot>();

    private void Awake()
    {
        // Очищаем снапшоты при загрузке новой сцены
        StaticSnapshots.Clear();
    }

    //    ������� ���������� (������ 60 ������)
    public void RecordSnapshot(PlayerSnapshot snapshot)
    {
        StaticSnapshots.Add(snapshot);
        StaticSnapshots.RemoveAll(s => Time.time - s.timeStamp > 60f);
    }

    // ���������� ������, ��������� �������� "secondsBack" ������ �����
    public PlayerSnapshot GetSnapshotForRewind(float secondsBack)
    {
        float targetTime = Time.time - secondsBack;
        for (int i = StaticSnapshots.Count - 1; i >= 0; i--)
        {
            if (StaticSnapshots[i].timeStamp <= targetTime)
                return StaticSnapshots[i];
        }
        if (StaticSnapshots.Count > 0)
            return StaticSnapshots[0];
        return null;
    }

    // ����������� ����� ��� �������� ������� ����������� �������
    public static bool HasSnapshots()
    {
        return StaticSnapshots.Count > 0;
    }

    // ����������� ����� ��� ������� ����������� �������
    public static void ClearSnapshots()
    {
        StaticSnapshots.Clear();
    }
    public PlayerSnapshot GetLatestSnapshot()
    {
        if (StaticSnapshots.Count > 0)
            return StaticSnapshots[StaticSnapshots.Count - 1];
        return null;
    }
}
