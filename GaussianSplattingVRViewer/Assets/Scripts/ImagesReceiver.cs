using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class ImagesReceiver : MonoBehaviour
{
    private HttpListener listener;
    public string savePath;
    private int imageCounter = 0;

    void Start()
    {
        // ���� �۾� ���丮�� "ReceivedData" ������ "Assets" ���� ���� ����
        savePath = Path.Combine(Application.dataPath, "Images");

        // ���丮�� ������ ����
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // HTTP Listener ����
        listener = new HttpListener();
        listener.Prefixes.Add("http://*:8080/upload/"); // *�� ��� ��Ʈ��ũ �������̽��� �ǹ�
        listener.Start();
        Debug.Log("Listening for incoming requests...");

        // �񵿱� ��û ó�� ����
        Task.Run(() => StartListening());
    }

    async Task StartListening()
    {
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;

            if (request.HttpMethod == "POST")
            {
                // �̹��� ���� �̸��� �����ϰ� ����
                string filePath = Path.Combine(savePath, $"{imageCounter++:D5}.jpg");

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await request.InputStream.CopyToAsync(fileStream);
                }

                Debug.Log("Image saved to: " + filePath);

                HttpListenerResponse response = context.Response;
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
            }
        }
    }

    void OnApplicationQuit()
    {
        listener.Stop();
    }
}
