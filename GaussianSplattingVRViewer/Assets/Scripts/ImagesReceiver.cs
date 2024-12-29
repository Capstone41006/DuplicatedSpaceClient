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
        // 현재 작업 디렉토리의 "ReceivedData" 폴더를 "Assets" 폴더 내로 설정
        savePath = Path.Combine(Application.dataPath, "Images");

        // 디렉토리가 없으면 생성
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // HTTP Listener 설정
        listener = new HttpListener();
        listener.Prefixes.Add("http://*:8080/upload/"); // *는 모든 네트워크 인터페이스를 의미
        listener.Start();
        Debug.Log("Listening for incoming requests...");

        // 비동기 요청 처리 시작
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
                // 이미지 파일 이름을 고유하게 설정
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
