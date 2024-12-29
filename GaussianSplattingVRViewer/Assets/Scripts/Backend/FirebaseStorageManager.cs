using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Storage;
using UnityEditor.SceneManagement;
using UnityEngine;

public class FirebaseStorageManager : MonoBehaviour
{
    private FirebaseStorage _storage;
    private StorageReference _storageRef;

    private void Awake()
    {
        // Firebase Storage 초기화
        _storage = FirebaseStorage.DefaultInstance;
        _storageRef = _storage.RootReference;

        Debug.Log("파이어베이스 초기화!");
    }

    public async void UploadFile(string filePath, string fileName)
    {
        // 업로드할 파일 경로 및 이름 설정
        var file = File.ReadAllBytes(filePath);
        var fileRef = _storageRef.Child(fileName);

        // 파일 업로드
        await fileRef.PutBytesAsync(file);

        // 업로드 성공 시 메시지 출력
        Debug.Log("파일 업로드 성공!");
    }

    public void Upload()
    {
        string path = ".";
        string name = "test.txt";

        UploadFile(path, name);
    }


    // 텍스트 파일 다운로드 메소드
    public async void DownloadFile(string remoteFilePath, string localFilePath)
    {
        // 파일 다운로드
        StorageReference textRef = _storageRef.Child(remoteFilePath);
        byte[] textBytes = await textRef.GetBytesAsync(5000000000);

        // 로컬에 파일 저장
        File.WriteAllBytes(localFilePath, textBytes);

        Debug.Log("파일 다운로드 성공!");
    }

    // 다운로드 버튼 클릭 이벤트에 연결된 메소드
    public void Download()
    {
        string remoteFilePath = "point_cloud2.ply"; // Firebase Storage에 있는 파일 경로
        //string localFilePath = Application.persistentDataPath + "/test_down.txt"; // 로컬에 저장할 파일 경로
        //string localFilePath = "./Assets/Database/point_cloud.ply"; // 로컬에 저장할 파일 경로
        string localFilePath = "./point_cloud.ply"; // 로컬에 저장할 파일 경로

        Debug.Log(localFilePath);
        DownloadFile(remoteFilePath, localFilePath);
    }
}
