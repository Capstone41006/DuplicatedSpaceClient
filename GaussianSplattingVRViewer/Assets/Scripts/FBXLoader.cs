using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using System.Collections.Generic;

public class FBXLoader : MonoBehaviour
{
    public string fbxFolderPath = "Assets/FBXs";
    public List<Transform> IMSI;

    [MenuItem("Tools/Load FBXs To Hierarchy")]

    private void Start()
    {
        string[] fbxFiles = Directory.GetFiles(fbxFolderPath, "*.fbx");

        // �� FBX ������ Hierarchy�� �ε�
        foreach (string fbxFile in fbxFiles)
        {
            string assetPath = fbxFile.Replace("\\", "/");      // ��θ� Unity�� �ν��� �� �ִ� ���·� ��ȯ
            GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);    // FBX ������ GameObject�� �ε�
            fbxObject.transform.localPosition = new Vector3(0, 1, 0);
            fbxObject.transform.localRotation = Quaternion.Euler(new Vector3(30, -25, -180));
            fbxObject.transform.localScale = Vector3.one * 500;


            if (fbxObject != null)
            {
                GameObject grabWorldObject = GameObject.Find("GrabWorld");
                XRGeneralGrabTransformer grabWorldTransformer = grabWorldObject.GetComponent<XRGeneralGrabTransformer>();

				// FBX�� Hierarchy�� �߰��ϰ� �̸� ����
				GameObject instance = Instantiate(fbxObject, transform);    // �� ��ũ��Ʈ�� ���� ������Ʈ�� �ڽ����� �߰�
                instance.name = fbxObject.name;                             // �ν��Ͻ� �̸� ����

				instance.layer = LayerMask.NameToLayer("UI");

				// XR Grab Interactable, XR General Grab Transformer, Mesh Collider ������Ʈ �߰�
				//BoxCollider boxCollider = instance.AddComponent<BoxCollider>();
				MeshCollider meshCollider = instance.AddComponent<MeshCollider>();

                // FBX�� Mesh ����
                MeshFilter meshFilter = instance.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null)
                {
                    meshCollider.sharedMesh = meshFilter.sharedMesh; // MeshCollider�� �޽� ����
                    meshCollider.convex = true; // XR���� �浹 ó���� ���� convex�� ����
                }

                XRGrabInteractable grabInteractable = instance.AddComponent<XRGrabInteractable>();
                grabInteractable.selectMode = InteractableSelectMode.Multiple;
                grabInteractable.focusMode = InteractableFocusMode.Multiple;
                grabInteractable.throwOnDetach = false;
                grabInteractable.useDynamicAttach = true;
                grabInteractable.matchAttachPosition = true;
                grabInteractable.matchAttachRotation = true;
                grabInteractable.snapToColliderVolume = true;
                grabInteractable.reinitializeDynamicAttachEverySingleGrab = true;
                grabInteractable.startingMultipleGrabTransformers.Add(grabWorldTransformer);

                XRGeneralGrabTransformer xrGeneralGrabTransformer = instance.AddComponent<XRGeneralGrabTransformer>();
                xrGeneralGrabTransformer.allowOneHandedScaling = false;
                xrGeneralGrabTransformer.allowTwoHandedScaling = true;
                xrGeneralGrabTransformer.clampScaling = false;
                xrGeneralGrabTransformer.scaleMultiplier = 1.0f;

                // �̹� �����ϴ� Rigidbody ������Ʈ�� �����ͼ� useGravity ����
                Rigidbody rb = instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;  // Rigidbody�� useGravity�� false�� ����
                    rb.isKinematic = true;
                }

                // FBX ���ϸ��� ���� ���� (��: obj1 -> 1)
                string fbxFileName = Path.GetFileNameWithoutExtension(fbxFile);
                string fileNumber = fbxFileName.Substring(fbxFileName.Length - 1);  // ���ϸ� ���� ���� ����

                // �ؽ�ó ���� ��� ���� (��: texture1.png)
                string textureFileName = "texture" + fileNumber + ".png";
                string texturePath = Path.Combine(fbxFolderPath, textureFileName).Replace("\\", "/");

                // �ؽ�ó �ε�
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                if (texture != null)
                {
                    // �� ��Ƽ���� ���� �� �ؽ�ó ����
                    Material newMaterial = new Material(Shader.Find("Standard"));
                    newMaterial.mainTexture = texture;

                    // FBX�� ��� MeshRenderer�� ��Ƽ���� ����
                    MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in meshRenderers)
                    {
                        renderer.material = newMaterial;
                    }
                }
            }
        }
    }
}
