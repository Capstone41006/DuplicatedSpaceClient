using UnityEngine;
using Firebase.Database;

public class FirebaseDBManager : MonoBehaviour
{
    // Firebase Realtime Database 연결
    private FirebaseDatabase database;

    void Start()
    {
        // Firebase Realtime Database SDK 초기화
        database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
    }

    public void DBUpdate()
    {
        // 데이터베이스 접근
        DatabaseReference reference = database.GetReference("isTrain");

        // 'istrain' 키 값을 true로 변경
        reference.SetValueAsync(true);
    }
}
