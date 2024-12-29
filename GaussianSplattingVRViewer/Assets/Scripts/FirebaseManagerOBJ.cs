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
    // Firebase Realtime Database 연결
    private FirebaseDatabase database;
    // Firebase Storage 연결
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

    // Scene 관리
    public GameObject ViewerSceneButton;
    public TextMeshProUGUI InputLevelCounterUI;
    public bool isDownloaded = false;
    private bool onViewerButton = false;
    public bool isSelected = false;

    // SAM2 처리
    public XRRayInteractor rayInteractor;
    public Camera mainCamera; // 메인 카메라
    public RectTransform canvasRectTransform; // Canvas의 RectTransform
    public GameObject plane; // Plane 오브젝트
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
        reference_inputs = FirebaseDatabase.DefaultInstance.GetReference("inputs");         // 입력값들을 담을 최상단 오브젝트

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
        //// Firebase 초기화 ////
        // Firebase Realtime Database 초기화
        database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
        Debug.Log("1.파이어베이스 초기화!");
        // 데이터베이스 접근

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
    }


    private void Update()
    {
        // XR Ray Interactor로부터 RaycastHit 정보 얻기
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // 충돌한 오브젝트가 Plane인 경우
            if (hit.collider.gameObject == plane)
            {
                // 충돌 지점의 월드 좌표
                Vector3 worldPoint = hit.point;

                // 월드 좌표를 화면 좌표로 변환
                Vector2 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);

                // 화면 좌표를 Canvas의 로컬 좌표로 변환
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    screenPoint,
                    mainCamera,
                    out localPoint
                );

                // Canvas RectTransform의 크기 가져오기
                Vector2 size = canvasRectTransform.sizeDelta;

                //Debug.Log(localPoint);      // (width, height)=(1080,720)에 대해, (-540,-360)~(540,360)을 반환
                //Debug.Log(size);              // (width, height) 값을 그대로 반환
                // 로컬 좌표를 (0, 0) ~ (1, 1)로 정규화
                float normalizedX = ( localPoint.x*2 + size.x) / (2*size.x);
                float normalizedY = (-localPoint.y*2 + size.y) / (2*size.y);        // localPoint.y는 방향 반대로 하는 이유: y축 방향이 반대가 됨

                // 결과 로그 출력
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

        // UI 이벤트가 발생했는지 확인
        //if (eventSystem.IsPointerOverGameObject())
        //{
        //    // Debug.Log("Pointer is over UI element");
        //}

        // isMade가 작동하면,
        refer_isMadeObject.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to read value: " + task.Exception.ToString());
                return;
            }

            // 읽어온 값을 bool 형으로 변환하여 가져옴
            DataSnapshot snapshot = task.Result;
            object value = snapshot.Value; // 가져온 값은 Object 형식으로 반환됨

            // bool로 형변환
            bool isMade = (bool)value;

            if (isMade == true && !isDownloaded)
            {
                Debug.Log("3.FBX 파일 다운로드 중!");
                isDownloaded = true;
                //Download();
                DownloadFBX();
                //refer_isMadeObject.SetValueAsync(false);
            }
        });

        //if (onViewerButton == true)
        //{
        //    ViewerSceneButton.SetActive(true);      // 다음 씬으로 넘어가는 버튼 활성화
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
        // 동영상 파일들을 스토리지에 업로드 
        //UploadVideoInFolder("Assets/VideoObject", "Video_Object/");                                       ////////// Firebase 결제 이슈로 현재는 이미 업로드된 것에서 사용 //////////

        // Space 학습 모드 ON
        refer_modeObject.SetValueAsync(true);
        Debug.Log("Send Space Making Request");
    }

    //////////// (x,y) 업데이트 ////////////
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

    //    // 'istrain' 키 값을 true로 변경
    //    refer_modeObject.SetValueAsync(true);
    //}


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
    //        // 파일 업로드
    //        UploadFile(imageFile, Path.Combine(storagePath, fileName));
    //    }
    //}

    void UploadVideoInFolder(string folderPath, string storagePath)
    {
        // 폴더 내의 모든 이미지 파일을 가져오기
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
            ContentType = "video/mp4"  // 여기서 MIME 타입을 설정
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
    ////////////////////////////////////////////////


    //////////// 2. FBX, PNG 다운 ////////////
    //// FBX, PNG 파일 다운로드 이벤트 함수
    private void OnValueChanged(object sender, ValueChangedEventArgs args)
    {
        // 읽어온 값을 bool로 변환
        if (args.Snapshot.Value is bool isMade)
        {
            Debug.Log("refer_isMadeObject 값 변경 감지: " + isMade);

            if (isMade && !isDownloaded) // true가 되고 아직 다운로드되지 않은 경우
            {
                Debug.Log("3. FBX 파일 다운로드 시작!");
                isDownloaded = true; // 중복 실행 방지
                DownloadFBX();

                //// 다운로드 완료 후 Firebase 값을 false로 설정 (옵션)
                //refer_isMadeObject.SetValueAsync(false).ContinueWith(task =>
                //{
                //        Debug.Log("refer_isMadeObject 값을 false로 설정");
                //});
            }
        }
    }

    public async void DownloadFile(string remoteFilePath, string localFilePath)
    {
        // 파일 다운로드
        StorageReference textRef = storageRef.Child(remoteFilePath);
        byte[] textBytes = await textRef.GetBytesAsync(5000000000);   // 최대 5GB

        // 로컬에 파일 저장
        File.WriteAllBytes(localFilePath, textBytes);
    }

    public async void DownloadFBX()
    {
        //string remoteFilePath1 = "Objects/obj1.fbx"; // Firebase Storage에 있는 파일 경로
        //string localFilePath1 = "Assets/FBXs/obj1.fbx"; // 로컬에 저장할 파일 경로
        //string remoteFilePath2 = "Objects/texture1.png"; // Firebase Storage에 있는 파일 경로
        //string localFilePath2 = "Assets/FBXs/texture1.png"; // 로컬에 저장할 파일 경로
        int childCount = 0;

        DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        DataSnapshot snapshot = await databaseRef.Child("inputs").GetValueAsync();

        if (snapshot.Exists)
        {
            // 자식 개수 출력
            childCount = (int)snapshot.ChildrenCount;
            //Debug.Log($"inputs의 자식 개수: {childCount}");

            for (int i = 1; i <= childCount; i++)
            {
                DownloadFile($"Objects/obj{i}.fbx", $"Assets/FBXs/obj{i}.fbx");
                DownloadFile($"Objects/texture{i}.png", $"Assets/FBXs/texture{i}.png");
            }

            Debug.Log("4.파일 다운로드 완료!");
            onViewerButton = true;
        }
    }


    //public void Download()
    //{
    //    string remoteFilePath = "point_cloud_instantsplat.ply"; // Firebase Storage에 있는 파일 경로
    //    string localFilePath = "./point_cloud_obj1.ply"; // 로컬에 저장할 파일 경로
    //    //string localFilePath = Application.persistentDataPath + "/test_down.txt"; // 로컬에 저장할 파일 경로
    //    //string localFilePath = "./Assets/Database/point_cloud.ply"; // 로컬에 저장할 파일 경로

    //    //Debug.Log(localFilePath);
    //    DownloadFile(remoteFilePath, localFilePath);
    //}
    ////////////////////////////////////////////////
}
