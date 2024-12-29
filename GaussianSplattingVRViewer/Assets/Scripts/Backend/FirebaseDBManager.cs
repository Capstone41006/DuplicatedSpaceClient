using UnityEngine;
using Firebase.Database;

public class FirebaseDBManager : MonoBehaviour
{
    // Firebase Realtime Database ����
    private FirebaseDatabase database;

    void Start()
    {
        // Firebase Realtime Database SDK �ʱ�ȭ
        database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
    }

    public void DBUpdate()
    {
        // �����ͺ��̽� ����
        DatabaseReference reference = database.GetReference("isTrain");

        // 'istrain' Ű ���� true�� ����
        reference.SetValueAsync(true);
    }
}
