using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Storage;
using Firebase.Database;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirebaseManager : MonoBehaviour
{
    // Firebase Realtime Database 연결
    private FirebaseDatabase database;
    // Firevase Storage 연결
    FirebaseStorage storage;
    StorageReference storageRef;
    DatabaseReference refer_isMadeSpace;
    DatabaseReference refer_modeSpace;

    // Scene 관리
    //public GameObject ViewerSceneButton;
    private bool isDownloaded = false;
    private bool onViewerButton = false;

    // 싱글톤 -> 다른 씬에서도 공간 생성 가능
    public static FirebaseManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        //// Firebase 초기화 ////
        // Firebase Realtime Database 초기화
        database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
        Debug.Log("1.파이어베이스 초기화!");
        // 데이터베이스 접근
        refer_isMadeSpace = database.GetReference("isMadeSpace");
        refer_modeSpace = database.GetReference("modeSpace");
        //reference_train = database.GetReference("isTrain");
        //reference_made = database.GetReference("isMade");
        //reference_mode = database.GetReference("mode");

        // Firebase Storage 초기화
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // FirebaseStorage 인스턴스 가져오기
                storage = FirebaseStorage.DefaultInstance;
                // 스토리지 레퍼런스 설정
                storageRef = storage.GetReferenceFromUrl("gs://fir-***.appspot.com");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
        });

        // 다른 씬에서도 확인하기 위해 데이터베이스의 'isMadeSpace' 플래그 값 변경 리스너 등록
        refer_isMadeSpace.ValueChanged += HandleIsMadeSpaceChanged;
    }

    private void HandleIsMadeSpaceChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        // 'isMadeSpace' 값을 bool 형으로 가져오기
        bool isMade = args.Snapshot.Value != null && (bool)args.Snapshot.Value;

        if (isMade && !isDownloaded)
        {
            Debug.Log("3.포인트 클라우드 파일 다운로드 중!");
            isDownloaded = true;
            Download();
        }
    }


    private void Update()
    {
        //if (onViewerButton)
        //{
            //ViewerSceneButton.SetActive(true); // 다음 씬으로 넘어가는 버튼 활성화
        //}
    }

    //private void initializationState()
    //{
    //    refer_isMadeSpace.SetValueAsync(false);
    //    refer_modeSpace.SetValueAsync(false);
    //    isDownloaded = false;
    //}


    //////////// 1. 동영상 업로드 ////////////
    public async void DBUpdate()
    {
        // 이미지 파일들을 스토리지에 업로드 
        //UploadVideoInFolder("Assets/VideoSpace", "Video_Space/");                     ////////// Firebase 결제 이슈로 현재는 이미 업로드된 것에서 사용 //////////

        // Space 학습 모드 ON
        await refer_modeSpace.SetValueAsync(true);
        Debug.Log("Send Space Making Request");
    }

    void UploadVideoInFolder(string folderPath, string storagePath)         // (유니티 내의 경로, DB 내의 경로)
    {
        // 폴더 내의 모든 동영상 파일을 가져오기
        string[] VideoFiles = Directory.GetFiles(folderPath, "*.mp4");

        // 각 이미지를 업로드
        foreach (string VideoFile in VideoFiles)
        {
            // 파일 이름만 추출
            string fileName = Path.GetFileName(VideoFile);
            // 파일 업로드
            UploadFile(VideoFile, Path.Combine(storagePath, fileName));
        }
    }

    // 파일 업로드 함수
    void UploadFile(string localFilePath, string storageFilePath)
    {
        // 로컬 파일 경로 설정
        string localPath = localFilePath;

        // 스토리지 레퍼런스 생성
        StorageReference fileRef = storageRef.Child(storageFilePath);

        // 파일 메타데이터 설정
        var metadata = new MetadataChange
        {
            ContentType = "video/mp4"  // 여기서 메타데이터 타입을 설정
        };

        // 파일 업로드
        fileRef.PutFileAsync(localPath, metadata).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to upload file: " + task.Exception.ToString());
            }
            else
            {
                //Debug.Log("File uploaded successfully: " + storageFilePath);
                Debug.Log("2.파일 업데이트 완료!");
            }
        });
    }

    // 특정 폴더 내의 모든 이미지 파일을 업로드하는 함수
    //void UploadAllImagesInFolder(string folderPath, string storagePath)
    //{
    //    // 폴더 내의 모든 이미지 파일을 가져오기
    //    string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

    //    // 각 이미지를 업로드
    //    foreach (string imageFile in imageFiles)
    //    {
    //        // 파일 이름만 추출
    //        string fileName = Path.GetFileName(imageFile);
    //        Debug.Log(fileName);
    //        // 파일 업로드
    //        UploadFile(imageFile, Path.Combine(storagePath, fileName));
    //    }
    //}

    //// 파일 업로드 함수
    //void UploadFile(string localFilePath, string storageFilePath)
    //{
    //    // 로컬 파일 경로 설정
    //    string localPath = localFilePath;

    //    // 스토리지 레퍼런스 생성
    //    StorageReference fileRef = storageRef.Child(storageFilePath);

    //    // 파일 업로드
    //    fileRef.PutFileAsync(localPath).ContinueWith(task =>
    //    {
    //        if (task.IsFaulted || task.IsCanceled)
    //        {
    //            Debug.LogError("Failed to upload file: " + task.Exception.ToString());
    //        }
    //        else
    //        {
    //            //Debug.Log("File uploaded successfully: " + storageFilePath);
    //            Debug.Log("2.파일 업데이트 완료!");
    //        }
    //    });
    //}
    ////////////////////////////////////////////////
   

    //////////// 2. 포인트 클라우드 다운 ////////////
    // 포인트 클라우드 파일 다운로드 함수
    public async void DownloadFile(string remoteFilePath, string localFilePath)
    {
        // 파일 다운로드
        StorageReference textRef = storageRef.Child(remoteFilePath);
        byte[] textBytes = await textRef.GetBytesAsync(5000000000);   // 최대 5GB

        // 로컬에 파일 저장
        File.WriteAllBytes(localFilePath, textBytes);

        Debug.Log("4.파일 다운로드 완료!");
        onViewerButton = true;
    }


    // 다운로드 버튼 클릭 이벤트에 연결된 메소드
    public void Download()
    {
        string remoteFilePath = "point_cloud.ply"; // Firebase Storage에 있는 파일 경로
        string localFilePath = "./point_cloud.ply"; // 로컬에 저장할 파일 경로
        //string localFilePath = Application.persistentDataPath + "/test_down.txt"; // 로컬에 저장할 파일 경로
        //string localFilePath = "./Assets/Database/point_cloud.ply"; // 로컬에 저장할 파일 경로
        //Debug.Log(localFilePath);
        DownloadFile(remoteFilePath, localFilePath);
    }
    ////////////////////////////////////////////////
}
