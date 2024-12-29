using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;

public class VideoControlUI : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Slider timeSlider;
    public XRRayInteractor rayInteractor;       // VR Controller의 레이
    public LayerMask interactableLayerMask;     // 상호작용 가능한 레이어 설정 (필요 시)
    public float last_time_value;

    private bool isDraggingSlider = false;      // 슬라이더가 현재 조작 중인지 여부
    private string videoFolderPath = "Assets/VideoObject/";

    void Start()
    {
        string videoPath = FindFirstMp4File(videoFolderPath);

        if (!string.IsNullOrEmpty(videoPath))
        {
            videoPlayer.url = Path.Combine(Application.dataPath, videoPath.Replace("Assets/", ""));
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnPrepareCompleted;
        }
        else
            Debug.LogError("MP4 파일을 업로드 하고 다시 실행하기");

        // 비디오 총 길이를 슬라이더의 최대값으로 설정
        //timeSlider.maxValue = (float)videoPlayer.length;
        //Debug.Log($"video time length is : {timeSlider.maxValue}초");
    }

    void OnPrepareCompleted(VideoPlayer vp)
    {
        timeSlider.maxValue = (float)videoPlayer.length;
        videoPlayer.Play();
        Debug.Log("Video Prepared, Length: " + videoPlayer.length);
    }

    void Update()
    {
        // Ray의 방향을 계산하여 충돌 감지
        Ray ray = new Ray(rayInteractor.transform.position, rayInteractor.transform.forward);
        RaycastHit hit;


        // 영상 컨트롤
        if (Physics.Raycast(ray, out hit, 100f, interactableLayerMask) && hit.transform.name == "Video Plane")         // 슬라이더를 건드렸다면, 그 순간의 시간으로 슬라이더 멈춤
        {
            videoPlayer.Pause();                        // 영상을 멈추고
            last_time_value = timeSlider.value;              // 그 순간의 시간을 기록
            isDraggingSlider = true;
            //Debug.Log(last_time_value);
        }
        else
        {
            last_time_value = timeSlider.value;              // 슬라이더 업데이트를 위해 시간을 기록
        }

        // 슬라이더 컨트롤
        if (!isDraggingSlider)                  // 슬라이더를 건들고 있지 않다면, 비디오 플레이어의 값을 슬라이더 값으로 계속 매핑함
        {
            timeSlider.value = (float)videoPlayer.time;
        }
    }

    private string FindFirstMp4File(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.mp4", SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
            return files[0];

        return null;
    }

    public void RePlay()
    {
        videoPlayer.time = last_time_value;
        videoPlayer.Play();
        isDraggingSlider = false;
    }
}
