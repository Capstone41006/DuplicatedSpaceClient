using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadImage2Canvas : MonoBehaviour
{
    //public GameObject obj;      // RawImage 컴포넌트를 가진 오브젝트
    public GameObject plane;    // Plane 컴포넌트를 가진 오브젝트

    void Start()
    {
        // 현재 실행 중인 경로를 가져옴
        string currentPath = Application.dataPath;

        // Images 폴더 경로 생성
        string imagesPath = Path.Combine(currentPath, "Images");

        // Images 폴더 내 파일 목록 가져오기
        string[] imageFiles = Directory.GetFiles(imagesPath);

        if (imageFiles.Length > 0)
        {
            // 첫 번째 이미지 파일의 경로를 가져옴
            string firstImagePath = imageFiles[0];

            // 이미지 데이터를 읽어서 Texture2D로 변환
            Texture2D texture = LoadTexture(firstImagePath);

            // obj의 RawImage 컴포넌트에 텍스처 적용
            //RawImage rawImage = obj.GetComponent<RawImage>();
            // rawImage.texture = texture;
            Renderer renderer = plane.GetComponent<Renderer>();
            renderer.material.mainTexture = texture;
        }
        else
        {
            Debug.LogError("Images 폴더에 이미지 파일이 없습니다.");
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        // 파일에서 바이트 배열 읽기
        byte[] fileData = File.ReadAllBytes(filePath);

        // 텍스처 생성
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // 이미지 데이터로 텍스처 로드

        return texture;
    }
}
