using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadImage2Canvas : MonoBehaviour
{
    //public GameObject obj;      // RawImage ������Ʈ�� ���� ������Ʈ
    public GameObject plane;    // Plane ������Ʈ�� ���� ������Ʈ

    void Start()
    {
        // ���� ���� ���� ��θ� ������
        string currentPath = Application.dataPath;

        // Images ���� ��� ����
        string imagesPath = Path.Combine(currentPath, "Images");

        // Images ���� �� ���� ��� ��������
        string[] imageFiles = Directory.GetFiles(imagesPath);

        if (imageFiles.Length > 0)
        {
            // ù ��° �̹��� ������ ��θ� ������
            string firstImagePath = imageFiles[0];

            // �̹��� �����͸� �о Texture2D�� ��ȯ
            Texture2D texture = LoadTexture(firstImagePath);

            // obj�� RawImage ������Ʈ�� �ؽ�ó ����
            //RawImage rawImage = obj.GetComponent<RawImage>();
            // rawImage.texture = texture;
            Renderer renderer = plane.GetComponent<Renderer>();
            renderer.material.mainTexture = texture;
        }
        else
        {
            Debug.LogError("Images ������ �̹��� ������ �����ϴ�.");
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        // ���Ͽ��� ����Ʈ �迭 �б�
        byte[] fileData = File.ReadAllBytes(filePath);

        // �ؽ�ó ����
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // �̹��� �����ͷ� �ؽ�ó �ε�

        return texture;
    }
}
