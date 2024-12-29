using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;
using Firebase.Database;

public class VRVideoPointTracker : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Renderer videoRenderer;              // Plane�� Renderer ������Ʈ
    public XRRayInteractor rayInteractor;       // VR ��Ʈ�ѷ��� ����� XRRayInteractor
    public RenderTexture renderTexture;         // Plane�� ����� RenderTexture

    void Start()
    {
        // ���� �÷��̾��� Ÿ�� �ؽ�ó�� RenderTexture�� ����
        videoPlayer.targetTexture = renderTexture;

        // Plane�� Material�� RenderTexture�� ����
        videoRenderer.material.mainTexture = renderTexture;
    }

    void Update()
    {
        // ����ĳ��Ʈ�� Plane ������ �¾Ҵ��� üũ
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log("Hit!");
            if (hit.transform == videoRenderer.transform)
            {
                // Plane�� Ŭ���� ��ǥ�� ��� ���� ���� ��ġ�� ���
                Vector3 localPoint = videoRenderer.transform.InverseTransformPoint(hit.point);

                // Plane�� ���� ��ǥ�� 0-1 ������ ����ȭ
                Vector2 normalizedPoint = new Vector2(
                    (localPoint.x + 0.5f) / videoRenderer.transform.localScale.x,
                    (localPoint.y + 0.5f) / videoRenderer.transform.localScale.y);

                Debug.Log(normalizedPoint);
                // ���� ������ ��� �ð��� �Բ� ��ǥ�� Firebase�� ����
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
