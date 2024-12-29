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

        // 각 FBX 파일을 Hierarchy에 로드
        foreach (string fbxFile in fbxFiles)
        {
            string assetPath = fbxFile.Replace("\\", "/");      // 경로를 Unity가 인식할 수 있는 형태로 변환
            GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);    // FBX 파일을 GameObject로 로드
            fbxObject.transform.localPosition = new Vector3(0, 1, 0);
            fbxObject.transform.localRotation = Quaternion.Euler(new Vector3(30, -25, -180));
            fbxObject.transform.localScale = Vector3.one * 500;


            if (fbxObject != null)
            {
                GameObject grabWorldObject = GameObject.Find("GrabWorld");
                XRGeneralGrabTransformer grabWorldTransformer = grabWorldObject.GetComponent<XRGeneralGrabTransformer>();

				// FBX를 Hierarchy에 추가하고 이름 설정
				GameObject instance = Instantiate(fbxObject, transform);    // 이 스크립트를 가진 오브젝트의 자식으로 추가
                instance.name = fbxObject.name;                             // 인스턴스 이름 설정

				instance.layer = LayerMask.NameToLayer("UI");

				// XR Grab Interactable, XR General Grab Transformer, Mesh Collider 컴포넌트 추가
				//BoxCollider boxCollider = instance.AddComponent<BoxCollider>();
				MeshCollider meshCollider = instance.AddComponent<MeshCollider>();

                // FBX의 Mesh 설정
                MeshFilter meshFilter = instance.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null)
                {
                    meshCollider.sharedMesh = meshFilter.sharedMesh; // MeshCollider에 메쉬 설정
                    meshCollider.convex = true; // XR과의 충돌 처리를 위해 convex로 설정
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

                // 이미 존재하는 Rigidbody 컴포넌트를 가져와서 useGravity 설정
                Rigidbody rb = instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;  // Rigidbody의 useGravity를 false로 설정
                    rb.isKinematic = true;
                }

                // FBX 파일명에서 숫자 추출 (예: obj1 -> 1)
                string fbxFileName = Path.GetFileNameWithoutExtension(fbxFile);
                string fileNumber = fbxFileName.Substring(fbxFileName.Length - 1);  // 파일명 끝의 숫자 추출

                // 텍스처 파일 경로 구성 (예: texture1.png)
                string textureFileName = "texture" + fileNumber + ".png";
                string texturePath = Path.Combine(fbxFolderPath, textureFileName).Replace("\\", "/");

                // 텍스처 로드
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                if (texture != null)
                {
                    // 새 머티리얼 생성 및 텍스처 연결
                    Material newMaterial = new Material(Shader.Find("Standard"));
                    newMaterial.mainTexture = texture;

                    // FBX의 모든 MeshRenderer에 머티리얼 적용
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
