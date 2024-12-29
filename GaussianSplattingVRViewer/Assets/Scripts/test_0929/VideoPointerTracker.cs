using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;
using Firebase.Database;

public class VRVideoPointTracker : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Renderer videoRenderer;              // Plane의 Renderer 컴포넌트
    public XRRayInteractor rayInteractor;       // VR 컨트롤러에 연결된 XRRayInteractor
    public RenderTexture renderTexture;         // Plane에 적용될 RenderTexture

    void Start()
    {
        // 비디오 플레이어의 타겟 텍스처로 RenderTexture를 설정
        videoPlayer.targetTexture = renderTexture;

        // Plane의 Material에 RenderTexture를 적용
        videoRenderer.material.mainTexture = renderTexture;
    }

    void Update()
    {
        // 레이캐스트가 Plane 영역에 맞았는지 체크
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log("Hit!");
            if (hit.transform == videoRenderer.transform)
            {
                // Plane에 클릭된 좌표를 얻기 위해 로컬 위치를 계산
                Vector3 localPoint = videoRenderer.transform.InverseTransformPoint(hit.point);

                // Plane의 로컬 좌표를 0-1 범위로 정규화
                Vector2 normalizedPoint = new Vector2(
                    (localPoint.x + 0.5f) / videoRenderer.transform.localScale.x,
                    (localPoint.y + 0.5f) / videoRenderer.transform.localScale.y);

                Debug.Log(normalizedPoint);
                // 현재 비디오의 재생 시간과 함께 좌표를 Firebase에 저장
                //SavePointToFirebase(normalizedPoint, videoPlayer.time);
            }
        }
    }

    void SavePointToFirebase(Vector2 point, double time)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        string key = reference.Child("videoPoints").Push().Key;

        var videoPointData = new
        {
            x = point.x,
            y = point.y,
            time = time
        };

        reference.Child("videoPoints").Child(key).SetRawJsonValueAsync(JsonUtility.ToJson(videoPointData));
    }
}
