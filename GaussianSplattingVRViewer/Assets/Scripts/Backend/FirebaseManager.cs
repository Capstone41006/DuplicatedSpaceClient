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
    // Firebase Realtime Database ����
    private FirebaseDatabase database;
    // Firevase Storage ����
    FirebaseStorage storage;
    StorageReference storageRef;
    DatabaseReference refer_isMadeSpace;
    DatabaseReference refer_modeSpace;

    // Scene ����
    //public GameObject ViewerSceneButton;
    private bool isDownloaded = false;
    private bool onViewerButton = false;

    // �̱��� -> �ٸ� �������� ���� ���� ����
    public static FirebaseManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        //// Firebase �ʱ�ȭ ////
        // Firebase Realtime Database �ʱ�ȭ
        database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
        Debug.Log("1.���̾�̽� �ʱ�ȭ!");
        // �����ͺ��̽� ����
        refer_isMadeSpace = database.GetReference("isMadeSpace");
        refer_modeSpace = database.GetReference("modeSpace");
        //reference_train = database.GetReference("isTrain");
        //reference_made = database.GetReference("isMade");
        //reference_mode = database.GetReference("mode");

        // Firebase Storage �ʱ�ȭ
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // FirebaseStorage �ν��Ͻ� ��������
                storage = FirebaseStorage.DefaultInstance;
                // ���丮�� ���۷��� ����
                storageRef = storage.GetReferenceFromUrl("gs://fir-***.appspot.com");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
        });

        // �ٸ� �������� Ȯ���ϱ� ���� �����ͺ��̽��� 'isMadeSpace' �÷��� �� ���� ������ ���
        refer_isMadeSpace.ValueChanged += HandleIsMadeSpaceChanged;
    }

    private void HandleIsMadeSpaceChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }

        // 'isMadeSpace' ���� bool ������ ��������
        bool isMade = args.Snapshot.Value != null && (bool)args.Snapshot.Value;

        if (isMade && !isDownloaded)
        {
            Debug.Log("3.����Ʈ Ŭ���� ���� �ٿ�ε� ��!");
            isDownloaded = true;
            Download();
        }
    }


    private void Update()
    {
        //if (onViewerButton)
        //{
            //ViewerSceneButton.SetActive(true); // ���� ������ �Ѿ�� ��ư Ȱ��ȭ
        //}
    }

    //private void initializationState()
    //{
    //    refer_isMadeSpace.SetValueAsync(false);
    //    refer_modeSpace.SetValueAsync(false);
    //    isDownloaded = false;
    //}


    //////////// 1. ������ ���ε� ////////////
    public async void DBUpdate()
    {
        // �̹��� ���ϵ��� ���丮���� ���ε� 
        //UploadVideoInFolder("Assets/VideoSpace", "Video_Space/");                     ////////// Firebase ���� �̽��� ����� �̹� ���ε�� �Ϳ��� ��� //////////

        // Space �н� ��� ON
        await refer_modeSpace.SetValueAsync(true);
        Debug.Log("Send Space Making Request");
    }

    void UploadVideoInFolder(string folderPath, string storagePath)         // (����Ƽ ���� ���, DB ���� ���)
    {
        // ���� ���� ��� ������ ������ ��������
        string[] VideoFiles = Directory.GetFiles(folderPath, "*.mp4");

        // �� �̹����� ���ε�
        foreach (string VideoFile in VideoFiles)
        {
            // ���� �̸��� ����
            string fileName = Path.GetFileName(VideoFile);
            // ���� ���ε�
            UploadFile(VideoFile, Path.Combine(storagePath, fileName));
        }
    }

    // ���� ���ε� �Լ�
    void UploadFile(string localFilePath, string storageFilePath)
    {
        // ���� ���� ��� ����
        string localPath = localFilePath;

        // ���丮�� ���۷��� ����
        StorageReference fileRef = storageRef.Child(storageFilePath);

        // ���� ��Ÿ������ ����
        var metadata = new MetadataChange
        {
            ContentType = "video/mp4"  // ���⼭ ��Ÿ������ Ÿ���� ����
        };

        // ���� ���ε�
        fileRef.PutFileAsync(localPath, metadata).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to upload file: " + task.Exception.ToString());
            }
            else
            {
                //Debug.Log("File uploaded successfully: " + storageFilePath);
                Debug.Log("2.���� ������Ʈ �Ϸ�!");
            }
        });
    }

    // Ư�� ���� ���� ��� �̹��� ������ ���ε��ϴ� �Լ�
    //void UploadAllImagesInFolder(string folderPath, string storagePath)
    //{
    //    // ���� ���� ��� �̹��� ������ ��������
    //    string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

    //    // �� �̹����� ���ε�
    //    foreach (string imageFile in imageFiles)
    //    {
    //        // ���� �̸��� ����
    //        string fileName = Path.GetFileName(imageFile);
    //        Debug.Log(fileName);
    //        // ���� ���ε�
    //        UploadFile(imageFile, Path.Combine(storagePath, fileName));
    //    }
    //}

    //// ���� ���ε� �Լ�
    //void UploadFile(string localFilePath, string storageFilePath)
    //{
    //    // ���� ���� ��� ����
    //    string localPath = localFilePath;

    //    // ���丮�� ���۷��� ����
    //    StorageReference fileRef = storageRef.Child(storageFilePath);

    //    // ���� ���ε�
    //    fileRef.PutFileAsync(localPath).ContinueWith(task =>
    //    {
    //        if (task.IsFaulted || task.IsCanceled)
    //        {
    //            Debug.LogError("Failed to upload file: " + task.Exception.ToString());
    //        }
    //        else
    //        {
    //            //Debug.Log("File uploaded successfully: " + storageFilePath);
    //            Debug.Log("2.���� ������Ʈ �Ϸ�!");
    //        }
    //    });
    //}
    ////////////////////////////////////////////////
   

    //////////// 2. ����Ʈ Ŭ���� �ٿ� ////////////
    // ����Ʈ Ŭ���� ���� �ٿ�ε� �Լ�
    public async void DownloadFile(string remoteFilePath, string localFilePath)
    {
        // ���� �ٿ�ε�
        StorageReference textRef = storageRef.Child(remoteFilePath);
        byte[] textBytes = await textRef.GetBytesAsync(5000000000);   // �ִ� 5GB

        // ���ÿ� ���� ����
        File.WriteAllBytes(localFilePath, textBytes);

        Debug.Log("4.���� �ٿ�ε� �Ϸ�!");
        onViewerButton = true;
    }


    // �ٿ�ε� ��ư Ŭ�� �̺�Ʈ�� ����� �޼ҵ�
    public void Download()
    {
        string remoteFilePath = "point_cloud.ply"; // Firebase Storage�� �ִ� ���� ���
        string localFilePath = "./point_cloud.ply"; // ���ÿ� ������ ���� ���
        //string localFilePath = Application.persistentDataPath + "/test_down.txt"; // ���ÿ� ������ ���� ���
        //string localFilePath = "./Assets/Database/point_cloud.ply"; // ���ÿ� ������ ���� ���
        //Debug.Log(localFilePath);
        DownloadFile(remoteFilePath, localFilePath);
    }
    ////////////////////////////////////////////////
}
