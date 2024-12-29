using Firebase.Database;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

public class FirebaseManagerDS : MonoBehaviour
{
    // Firebase Realtime Database ����
    private FirebaseDatabase database;
    DatabaseReference reference_inputs;

    public OculusFunctions OculusData;

    public Transform load_transform;

    private Vector3 loaded_position;
    private Quaternion loaded_rotation;
    private Vector3 loaded_scale;

    public bool flag = false;

    public int cnt = 1;

    private void Start()
    {
        OculusData = GameObject.Find("Settings").GetComponent<OculusFunctions>();
        reference_inputs = FirebaseDatabase.DefaultInstance.GetReference("inputs"); // �Է°����� ���� �ֻ�� ������Ʈ

        //InitializeTransform();
    }

    private void Awake()
    {
        // Firebase �ʱ�ȭ
        database = FirebaseDatabase.GetInstance("https://fir-study-***.firebaseio.com/");
    }

    private void Update()
    {
        if(flag)
        {
            string inputKey = "input1";
            DatabaseReference inputReference = reference_inputs.Child(inputKey);

            inputReference.GetValueAsync().ContinueWith(task =>
            {
                DataSnapshot snapshot = task.Result; // input1�� ���� �����͸� ������

                float posX = float.Parse(snapshot.Child("pos_x").Value.ToString());
                float posY = float.Parse(snapshot.Child("pos_y").Value.ToString());
                float posZ = float.Parse(snapshot.Child("pos_z").Value.ToString());
                float rotX = float.Parse(snapshot.Child("rot_x").Value.ToString());
                float rotY = float.Parse(snapshot.Child("rot_y").Value.ToString());
                float rotZ = float.Parse(snapshot.Child("rot_z").Value.ToString());
                float scaleX = float.Parse(snapshot.Child("s_x").Value.ToString());
                float scaleY = float.Parse(snapshot.Child("s_y").Value.ToString());
                float scaleZ = float.Parse(snapshot.Child("s_z").Value.ToString());

                // Debug.Log�� �� �� ���
                Debug.LogWarning($"Position: ({posX}, {posY}, {posZ})");
                Debug.LogWarning($"Rotation: ({rotX}, {rotY}, {rotZ})");
                Debug.LogWarning($"Scale: ({scaleX}, {scaleY}, {scaleZ})");

                // Cube�� Transform ����
                loaded_position = new Vector3(posX, posY, posZ);
                loaded_rotation = Quaternion.Euler(rotX, rotY, rotZ);
                loaded_scale = new Vector3(scaleX, scaleY, scaleZ);
                Debug.LogWarning("Update");
            });

            load_transform.transform.position = loaded_position;
            load_transform.transform.rotation = loaded_rotation;
            load_transform.transform.localScale = loaded_scale;

            flag = false;
        }

        
    }
}
