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
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Firebase.Extensions;

public class FirebaseManagerOBJ : MonoBehaviour
{
    // Firebase Realtime Database ����
    private FirebaseDatabase database;
    // Firebase Storage ����
    FirebaseStorage storage;
    StorageReference storageRef;
    //DatabaseReference reference_x1;
    //DatabaseReference reference_y1;
    //DatabaseReference reference_x2;
    //DatabaseReference reference_y2;
    //DatabaseReference reference_time;
    //DatabaseReference reference_selected;
    DatabaseReference refer_isMadeObject;
    DatabaseReference refer_modeObject;

    DatabaseReference reference_inputs;

    // Scene ����
    public GameObject ViewerSceneButton;
    public TextMeshProUGUI InputLevelCounterUI;
    public bool isDownloaded = false;
    private bool onViewerButton = false;
    public bool isSelected = false;

    // SAM2 ó��
    public XRRayInteractor rayInteractor;
    public Camera mainCamera; // ���� ī�޶�
    public RectTransform canvasRectTransform; // Canvas�� RectTransform
    public GameObject plane; // Plane ������Ʈ
    public OculusFunctions OculusData;
    public VideoControlUI videoControlUI;
    private float saved_x1 = 0f;
    private float saved_y1 = 0f;
    private float saved_x2 = 0f;
    private float saved_y2 = 0f;
    private float saved_time = 0f;
    private int input_count = 1;


    private void Start()
    {
        InputLevelCounterUI.text = input_count.ToString();

        OculusData = GameObject.Find("Settings").GetComponent<OculusFunctions>();
        refer_isMadeObject = database.GetReference("isMadeObject");
        refer_modeObject = database.GetReference("modeObject");
        reference_inputs = FirebaseDatabase.DefaultInstance.GetReference("inputs");         // �Է°����� ���� �ֻ�� ������Ʈ

        refer_isMadeObject.ValueChanged += OnValueChanged;

        //reference_selected = database.GetReference("isSelected");
        //reference_x1 = database.GetReference("ref_x1");
        //reference_y1 = database.GetReference("ref_y1");
        //reference_x2 = database.GetReference("ref_x2");
        //reference_y2 = database.GetReference("ref_y2");
        //reference_time = database.GetReference("ref_t");
        //reference_mode.SetValueAsync(2);
        //StorageUpdate();
    }


    private void Awake()
    {
        //// Firebase �ʱ�ȭ ////
        // Firebase Realtime Database �ʱ�ȭ
        database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
        Debug.Log("1.���̾�̽� �ʱ�ȭ!");
        // �����ͺ��̽� ����

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
    }


    private void Update()
    {
        // XR Ray Interactor�κ��� RaycastHit ���� ���
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // �浹�� ������Ʈ�� Plane�� ���
            if (hit.collider.gameObject == plane)
            {
                // �浹 ������ ���� ��ǥ
                Vector3 worldPoint = hit.point;

                // ���� ��ǥ�� ȭ�� ��ǥ�� ��ȯ
                Vector2 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);

                // ȭ�� ��ǥ�� Canvas�� ���� ��ǥ�� ��ȯ
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    screenPoint,
                    mainCamera,
                    out localPoint
                );

                // Canvas RectTransform�� ũ�� ��������
                Vector2 size = canvasRectTransform.sizeDelta;

                //Debug.Log(localPoint);      // (width, height)=(1080,720)�� ����, (-540,-360)~(540,360)�� ��ȯ
                //Debug.Log(size);              // (width, height) ���� �״�� ��ȯ
                // ���� ��ǥ�� (0, 0) ~ (1, 1)�� ����ȭ
                float normalizedX = ( localPoint.x*2 + size.x) / (2*size.x);
                float normalizedY = (-localPoint.y*2 + size.y) / (2*size.y);        // localPoint.y�� ���� �ݴ�� �ϴ� ����: y�� ������ �ݴ밡 ��

                // ��� �α� ���
                //Debug.Log("Normalized Local Coordinates: (" + normalizedX + ", " + normalizedY + ")");
                if (OculusData.buttonA)
                {
                    saved_x1 = normalizedX;
                    saved_y1 = normalizedY;
                    Debug.Log("A Normalized Local Coordinates: (" + saved_x1 + ", " + saved_y1 + ")");
                    OculusData.buttonA = false;
                }
                if (OculusData.buttonB)
                {
                    saved_x2 = normalizedX;
                    saved_y2 = normalizedY;
                    Debug.Log("B Normalized Local Coordinates: (" + saved_x2 + ", " + saved_y2 + ")");
                    OculusData.buttonB = false;
                }
                saved_time = videoControlUI.last_time_value;
            }
        }

        // UI �̺�Ʈ�� �߻��ߴ��� Ȯ��
        //if (eventSystem.IsPointerOverGameObject())
        //{
        //    // Debug.Log("Pointer is over UI element");
        //}

        // isMade�� �۵��ϸ�,
        refer_isMadeObject.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to read value: " + task.Exception.ToString());
                return;
            }

            // �о�� ���� bool ������ ��ȯ�Ͽ� ������
            DataSnapshot snapshot = task.Result;
            object value = snapshot.Value; // ������ ���� Object �������� ��ȯ��

            // bool�� ����ȯ
            bool isMade = (bool)value;

            if (isMade == true && !isDownloaded)
            {
                Debug.Log("3.FBX ���� �ٿ�ε� ��!");
                isDownloaded = true;
                //Download();
                DownloadFBX();
                //refer_isMadeObject.SetValueAsync(false);
            }
        });

        //if (onViewerButton == true)
        //{
        //    ViewerSceneButton.SetActive(true);      // ���� ������ �Ѿ�� ��ư Ȱ��ȭ
        //    initializationState();
        //}
    }

    //private void initializationState()
    //{
    //    reference_x1.SetValueAsync(0);
    //    reference_y1.SetValueAsync(0);
    //    reference_x2.SetValueAsync(0);
    //    reference_y2.SetValueAsync(0);
    //    reference_time.SetValueAsync(0);
    //    reference_mode.SetValueAsync(0);
    //    isDownloaded = false;
    //    isSelected = false;
    //}


    public void DBUpdate()
    {
        // ������ ���ϵ��� ���丮���� ���ε� 
        //UploadVideoInFolder("Assets/VideoObject", "Video_Object/");                                       ////////// Firebase ���� �̽��� ����� �̹� ���ε�� �Ϳ��� ��� //////////

        // Space �н� ��� ON
        refer_modeObject.SetValueAsync(true);
        Debug.Log("Send Space Making Request");
    }

    //////////// (x,y) ������Ʈ ////////////
    public void DBXYUpdate()
    {
        //reference_x1.SetValueAsync(saved_x1);
        //reference_y1.SetValueAsync(saved_y1);
        //reference_x2.SetValueAsync(saved_x2);
        //reference_y2.SetValueAsync(saved_y2);
        //reference_time.SetValueAsync(saved_time);

        string inputKey = "input" + input_count.ToString();
        DatabaseReference inputReference = reference_inputs.Child(inputKey);
        inputReference.Child("ref_x1").SetValueAsync(saved_x1);
        inputReference.Child("ref_y1").SetValueAsync(saved_y1);
        inputReference.Child("ref_x2").SetValueAsync(saved_x2);
        inputReference.Child("ref_y2").SetValueAsync(saved_y2);
        inputReference.Child("ref_t").SetValueAsync(saved_time);

        input_count++;
        InputLevelCounterUI.text = input_count.ToString();
        Debug.Log(input_count);
    }

    //public void DBTrainStart()
    //{
    //    isSelected = true;
    //    reference_selected.SetValueAsync(isSelected);
    //    Debug.Log("Points are selected!!!");

    //    // 'istrain' Ű ���� true�� ����
    //    refer_modeObject.SetValueAsync(true);
    //}


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
    //        // ���� ���ε�
    //        UploadFile(imageFile, Path.Combine(storagePath, fileName));
    //    }
    //}

    void UploadVideoInFolder(string folderPath, string storagePath)
    {
        // ���� ���� ��� �̹��� ������ ��������
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
            ContentType = "video/mp4"  // ���⼭ MIME Ÿ���� ����
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
    ////////////////////////////////////////////////


    //////////// 2. FBX, PNG �ٿ� ////////////
    //// FBX, PNG ���� �ٿ�ε� �̺�Ʈ �Լ�
    private void OnValueChanged(object sender, ValueChangedEventArgs args)
    {
        // �о�� ���� bool�� ��ȯ
        if (args.Snapshot.Value is bool isMade)
        {
            Debug.Log("refer_isMadeObject �� ���� ����: " + isMade);

            if (isMade && !isDownloaded) // true�� �ǰ� ���� �ٿ�ε���� ���� ���
            {
                Debug.Log("3. FBX ���� �ٿ�ε� ����!");
                isDownloaded = true; // �ߺ� ���� ����
                DownloadFBX();

                //// �ٿ�ε� �Ϸ� �� Firebase ���� false�� ���� (�ɼ�)
                //refer_isMadeObject.SetValueAsync(false).ContinueWith(task =>
                //{
                //        Debug.Log("refer_isMadeObject ���� false�� ����");
                //});
            }
        }
    }

    public async void DownloadFile(string remoteFilePath, string localFilePath)
    {
        // ���� �ٿ�ε�
        StorageReference textRef = storageRef.Child(remoteFilePath);
        byte[] textBytes = await textRef.GetBytesAsync(5000000000);   // �ִ� 5GB

        // ���ÿ� ���� ����
        File.WriteAllBytes(localFilePath, textBytes);
    }

    public async void DownloadFBX()
    {
        //string remoteFilePath1 = "Objects/obj1.fbx"; // Firebase Storage�� �ִ� ���� ���
        //string localFilePath1 = "Assets/FBXs/obj1.fbx"; // ���ÿ� ������ ���� ���
        //string remoteFilePath2 = "Objects/texture1.png"; // Firebase Storage�� �ִ� ���� ���
        //string localFilePath2 = "Assets/FBXs/texture1.png"; // ���ÿ� ������ ���� ���
        int childCount = 0;

        DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        DataSnapshot snapshot = await databaseRef.Child("inputs").GetValueAsync();

        if (snapshot.Exists)
        {
            // �ڽ� ���� ���
            childCount = (int)snapshot.ChildrenCount;
            //Debug.Log($"inputs�� �ڽ� ����: {childCount}");

            for (int i = 1; i <= childCount; i++)
            {
                DownloadFile($"Objects/obj{i}.fbx", $"Assets/FBXs/obj{i}.fbx");
                DownloadFile($"Objects/texture{i}.png", $"Assets/FBXs/texture{i}.png");
            }

            Debug.Log("4.���� �ٿ�ε� �Ϸ�!");
            onViewerButton = true;
        }
    }


    //public void Download()
    //{
    //    string remoteFilePath = "point_cloud_instantsplat.ply"; // Firebase Storage�� �ִ� ���� ���
    //    string localFilePath = "./point_cloud_obj1.ply"; // ���ÿ� ������ ���� ���
    //    //string localFilePath = Application.persistentDataPath + "/test_down.txt"; // ���ÿ� ������ ���� ���
    //    //string localFilePath = "./Assets/Database/point_cloud.ply"; // ���ÿ� ������ ���� ���

    //    //Debug.Log(localFilePath);
    //    DownloadFile(remoteFilePath, localFilePath);
    //}
    ////////////////////////////////////////////////
}
