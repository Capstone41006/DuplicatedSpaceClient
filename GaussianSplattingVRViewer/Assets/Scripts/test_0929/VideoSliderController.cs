using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;

public class VideoControlUI : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Slider timeSlider;
    public XRRayInteractor rayInteractor;       // VR Controller�� ����
    public LayerMask interactableLayerMask;     // ��ȣ�ۿ� ������ ���̾� ���� (�ʿ� ��)
    public float last_time_value;

    private bool isDraggingSlider = false;      // �����̴��� ���� ���� ������ ����
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
            Debug.LogError("MP4 ������ ���ε� �ϰ� �ٽ� �����ϱ�");

        // ���� �� ���̸� �����̴��� �ִ밪���� ����
        //timeSlider.maxValue = (float)videoPlayer.length;
        //Debug.Log($"video time length is : {timeSlider.maxValue}��");
    }

    void OnPrepareCompleted(VideoPlayer vp)
    {
        timeSlider.maxValue = (float)videoPlayer.length;
        videoPlayer.Play();
        Debug.Log("Video Prepared, Length: " + videoPlayer.length);
    }

    void Update()
    {
        // Ray�� ������ ����Ͽ� �浹 ����
        Ray ray = new Ray(rayInteractor.transform.position, rayInteractor.transform.forward);
        RaycastHit hit;


        // ���� ��Ʈ��
        if (Physics.Raycast(ray, out hit, 100f, interactableLayerMask) && hit.transform.name == "Video Plane")         // �����̴��� �ǵ�ȴٸ�, �� ������ �ð����� �����̴� ����
        {
            videoPlayer.Pause();                        // ������ ���߰�
            last_time_value = timeSlider.value;              // �� ������ �ð��� ���
            isDraggingSlider = true;
            //Debug.Log(last_time_value);
        }
        else
        {
            last_time_value = timeSlider.value;              // �����̴� ������Ʈ�� ���� �ð��� ���
        }

        // �����̴� ��Ʈ��
        if (!isDraggingSlider)                  // �����̴��� �ǵ�� ���� �ʴٸ�, ���� �÷��̾��� ���� �����̴� ������ ��� ������
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
