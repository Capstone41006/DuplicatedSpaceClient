using Firebase.Database;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

public class FirebaseManagerModify : MonoBehaviour
{
    // Firebase Realtime Database ����
    private FirebaseDatabase database;
    DatabaseReference reference_inputs;

    public OculusFunctions OculusData;

    public Transform update_transform;           // ��ġ ������ �������� ��
    public Transform load_transform;

    private Vector3 loaded_position;
    private Quaternion loaded_rotation;
    private Vector3 loaded_scale;

    private int count = 1;

    private void Start()
    {
        OculusData = GameObject.Find("Settings").GetComponent<OculusFunctions>();
        reference_inputs = FirebaseDatabase.DefaultInstance.GetReference("inputs"); // �Է°����� ���� �ֻ�� ������Ʈ

        //InitializeTransform();
    }

    private void Awake()
    {
        // Firebase �ʱ�ȭ
        database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
    }

    private void Update()
    {
        if (OculusData.buttonB)
        {
            string inputKey = "input1";
            DatabaseReference inputReference = reference_inputs.Child(inputKey);

            inputReference.Child("pos_x").SetValueAsync(update_transform.position.x);
            inputReference.Child("pos_y").SetValueAsync(update_transform.position.y);
            inputReference.Child("pos_z").SetValueAsync(update_transform.position.z);
            inputReference.Child("rot_x").SetValueAsync(update_transform.rotation.eulerAngles.x); // Euler�� ����
            inputReference.Child("rot_y").SetValueAsync(update_transform.rotation.eulerAngles.y); // Euler�� ����
            inputReference.Child("rot_z").SetValueAsync(update_transform.rotation.eulerAngles.z); // Euler�� ����
            inputReference.Child("s_x").SetValueAsync(update_transform.localScale.x);
            inputReference.Child("s_y").SetValueAsync(update_transform.localScale.y);
            inputReference.Child("s_z").SetValueAsync(update_transform.localScale.z);

            OculusData.buttonB = false;
        }

        if (OculusData.buttonA)
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
                //Debug.Log($"Position: ({posX}, {posY}, {posZ})");
                //Debug.Log($"Rotation: ({rotX}, {rotY}, {rotZ})");
                //Debug.Log($"Scale: ({scaleX}, {scaleY}, {scaleZ})");

                // Cube�� Transform ����
                loaded_position = new Vector3(posX, posY, posZ);
                loaded_rotation = Quaternion.Euler(rotX, rotY, rotZ);
                loaded_scale = new Vector3(scaleX, scaleY, scaleZ);
                Debug.Log("Update");
            });

            load_transform.transform.position = loaded_position;
            load_transform.transform.rotation = loaded_rotation;
            load_transform.transform.localScale = loaded_scale;

            Debug.Log(loaded_position);
            Debug.Log(loaded_rotation);
            Debug.Log(loaded_scale);

            OculusData.buttonA = false;
        }
    }

    private void InitializeTransform()
    {
        //loaded_position = new Vector3(0, 0, 0);
        //loaded_rotation = Quaternion.Euler(0, 0, 0);
        //loaded_scale = new Vector3(0, 0, 0);

        
    }
}
