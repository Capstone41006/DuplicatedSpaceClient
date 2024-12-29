using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 다음으로 이동할 씬의 이름
    public string robby_scene_name = "Robby";
    public string object_sellectting_scene_name = "ObjectSellecting";
    public string duplicated_scene_name = "DuplicatedSpace";
    //public string vr_viewer_scene_name = "GaussianSplattingVR";

    // 다음 씬으로 이동하는 함수
    //public void LoadVRViewerScene()
    //{
    //    SceneManager.LoadScene(vr_viewer_scene_name);
    //}

    public void LoadObjectSellectingScene()
    {
        SceneManager.LoadScene(object_sellectting_scene_name);
    }

    public void LoadRobbyScene()
    {
        SceneManager.LoadScene(robby_scene_name);
    }

    public void LoadDuplicatedScene()
    {
        SceneManager.LoadScene(duplicated_scene_name);
    }
}
