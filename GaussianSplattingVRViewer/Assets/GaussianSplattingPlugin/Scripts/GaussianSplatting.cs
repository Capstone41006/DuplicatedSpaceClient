using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;

public class GaussianSplatting : MonoBehaviour
{

    private class GaussianSplattingNI
    {
        public const int INIT_EVENT = 0x0001;
        public const int DRAW_EVENT = 0x0002;

        [DllImport("gaussiansplatting", EntryPoint = "GetRenderEventFunc")] public static extern System.IntPtr GetRenderEventFunc();
        [DllImport("gaussiansplatting", EntryPoint = "IsAPIReady")] public static extern bool IsAPIReady();
        [DllImport("gaussiansplatting", EntryPoint = "GetLastMessage")] private static extern System.IntPtr _GetLastMessage();
        static public string GetLastMessage() { return Marshal.PtrToStringAnsi(_GetLastMessage()); }
        [DllImport("gaussiansplatting", EntryPoint = "LoadModel")] public static extern bool LoadModel(string file);
        [DllImport("gaussiansplatting", EntryPoint = "SetNbPov")] public static extern void SetNbPov(int nb_pov);
        [DllImport("gaussiansplatting", EntryPoint = "SetPovParameters")] public static extern void SetPovParameters(int pov, int width, int height);
        [DllImport("gaussiansplatting", EntryPoint = "IsInitialized")] public static extern bool IsInitialized();
        [DllImport("gaussiansplatting", EntryPoint = "GetTextureNativePointer")] public static extern System.IntPtr GetTextureNativePointer(int pov);
        [DllImport("gaussiansplatting", EntryPoint = "SetDrawParameters")] public static extern void SetDrawParameters(int pov, float[] position, float[] rotation, float[] proj, float fovy, float[] frustums);
        [DllImport("gaussiansplatting", EntryPoint = "SetCrop")] public static extern void SetCrop(float[] box_min, float[] box_max);
        [DllImport("gaussiansplatting", EntryPoint = "GetSceneSize")] public static extern void GetSceneSize(float[] scene_min, float[] scene_max);
        [DllImport("gaussiansplatting", EntryPoint = "IsDrawn")] public static extern bool IsDrawn();
        [DllImport("gaussiansplatting", EntryPoint = "GetNbSplat")] public static extern int GetNbSplat();
    }

    ////////////////// Information Field //////////////////
    [Header("Init Parameters")]
    public int model_type = 0;
    public string[] model_file_path_list = { "", };
    public string model_file_path = "";
    public TextAsset default_model;
    public Material mat;
    public Camera cam;
    public bool isXr;
    public List<Transform> trackTKSs;
    public Transform trackTRS;
    public bool initCrop = true;

    [Header("Dynamic Parameters")]
    public bool loadModelEvent = false;
    public bool sendInitEvent = false;
    public bool sendDrawEvent = false;
    public float renderScale = 0.5f;
    [Range(0.1f, 1f)]
    public float texFactor = 0.5f;
    public Vector3 cropMin = Vector3.zero;
    public Vector3 cropMax = Vector3.one;

    [Header("Informations")]
    public bool loaded = false;
    public bool initialized = false;
    public int nb_splats = 0;
    public string lastMessage = "";
    public Vector2Int internalTexSize;
    public Texture2D[] tex;
    public bool isInError = false;
    public Vector3 sceneMin = Vector3.zero;
    public Vector3 sceneMax = Vector3.one;
    ////////////////// Information Field //////////////////

    private float lastTexFactor = 0.5f;
    private GameObject real_leye, real_reye;
    private System.IntPtr renderEventFunc = System.IntPtr.Zero;
    private Thread thLoad = null;
    private Texture2D blackTexture = null;
    private int countDrawErrors = 0;
    private bool waitForTexture = false;
    private Vector3 curCropMin = Vector3.zero;
    private Vector3 curCropMax = Vector3.one;


    // Function - Get left eye pose, right eye pose
    bool TryGetEyesPoses(out Vector3 lpos, out Vector3 rpos, out Quaternion lrot, out Quaternion rrot)
    {
        lpos = Vector3.zero;
        rpos = Vector3.zero;
        lrot = Quaternion.identity;
        rrot = Quaternion.identity;
        int nbfound = 0;
        List<XRNodeState> states = new List<XRNodeState>();               // List that deal with XR states
        InputTracking.GetNodeStates(states);                              // Put XR state data into "state value" 

        foreach (XRNodeState state in states)                             // at list named "states", pull "state" object which type is XRNodeState for each loop
        {
            if (state.tracked && state.nodeType == XRNode.LeftEye)        // process Left Eye data
            {
                if (state.TryGetPosition(out Vector3 tpos)) { lpos = tpos; nbfound += 1; }
                if (state.TryGetRotation(out Quaternion trot)) { lrot = trot; nbfound += 1; }
            }
            if (state.tracked && state.nodeType == XRNode.RightEye)       // process Right Eye data
            {
                if (state.TryGetPosition(out Vector3 tpos)) { rpos = tpos; nbfound += 1; }
                if (state.TryGetRotation(out Quaternion trot)) { rrot = trot; nbfound += 1; }
            }
        }
        return nbfound == 4;
    }


    // Camera Render Event Handle Setting
    private void OnEnable()
    {
        Camera.onPreRender += OnPreRenderCallback;   // OnPreRenderCallback method registered at Camera's onPreRender event
        isInError = false;                           // It works before the Camera Renderings
    }

    private void OnDisable()
    {
        Camera.onPreRender -= OnPreRenderCallback;
        tex = null;
        lastMessage = "";
    }


    // Function - Set Black Texture which was set at Start()
    public void SetBlackTexture()
    {
        mat.SetTexture("_GaussianSplattingTexLeftEye", blackTexture);    // set "_GaussianSpalttingTexLeftEye" shader and blackTexture texture to mat shader 
        mat.SetTexture("_GaussianSplattingTexRightEye", blackTexture);
    }


    void OnPreRenderCallback(Camera camera)
    {
        if (loaded && renderEventFunc != System.IntPtr.Zero && sendDrawEvent && waitForTexture)
        {
            waitForTexture = false;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (!GaussianSplattingNI.IsDrawn() && sw.ElapsedMilliseconds < 1000)
            {
                Thread.Sleep(0);
            }

            if (sw.ElapsedMilliseconds >= 1000)
            {
                countDrawErrors += 1;
                //if 5 consecutive try in error stop !!!
                if (countDrawErrors >= 5)
                {
                    lastMessage = GaussianSplattingNI.GetLastMessage();
                    Debug.Log("Stop draw error: " + lastMessage);
                    isInError = true;
                    //Stop trying...
                    sendDrawEvent = false;
                }
            }
            else
            {
                countDrawErrors = 0;
            }
        }
    }

    // Fuction - 4*4 mat -> float[16]
    float[] matToFloat(Matrix4x4 mat)
    {
        return new float[16]
        {
            mat.m00, mat.m10, mat.m20, mat.m30,
            mat.m01, mat.m11, mat.m21, mat.m31,
            mat.m02, -mat.m12, mat.m22, mat.m32,
            mat.m03, mat.m13, mat.m23, mat.m33,
        };
    }



    // XR, Texture Setting
    private void Start()
    {
        // Verify that there is not a file  ->  this code is not used as I input a .ply file
        if (!File.Exists(model_file_path))                       
        {
            if (default_model != null)
            {
                model_file_path = Application.temporaryCachePath + "/default.ply";
                File.WriteAllBytes(model_file_path, default_model.bytes);

                //Default model automatic crop/scale/pos/rot
                trackTRS = trackTKSs[model_type];
                if (trackTRS != null)
                {
                    trackTRS.localScale = Vector3.one * 0.36f;
                    trackTRS.localPosition = new Vector3(0, 4.07f, -1.44f) * 0.36f;
                    trackTRS.localRotation = Quaternion.Euler(11, -38, 3.7f);
                }
                initCrop = false;
                texFactor = 0.7f;
                cropMin = new Vector3(-2.1f, -4.7f, -0.65f);
                cropMax = new Vector3(6.17f, 5.7f, 5.5f);
            }
        }

        isInError = false;
        countDrawErrors = 0;
        internalTexSize = Vector2Int.zero;
        tex = new Texture2D[isXr ? 2 : 1];                // if isXR is true, tex = Texture2D list[2], else tex = Texture2D list[1] 
        lastTexFactor = texFactor;
        blackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);            // Set black texture
        blackTexture.LoadRawTextureData(new byte[] { 0, 0, 0, 255 });
        blackTexture.Apply();
        SetBlackTexture();
    }


    private void Update()
    {
        // Gaussian splatting map changing
        if (model_type >= 0 && model_type < model_file_path_list.Length) { model_file_path = model_file_path_list[model_type]; }
        else { model_file_path = model_file_path_list[0]; }

        // Gaussian splatting Transfrom Setting
        if (trackTRS != null)               // If interactor transform position is set 
        {
            trackTRS = trackTKSs[model_type];
            renderScale = trackTRS.localScale.x;           // grabbed object's scale is saved to renderScale
            transform.localPosition = trackTRS.localPosition / renderScale;     // gaussian splatting object's Transform is related to trackTRS(=grabWorld)
            transform.localRotation = trackTRS.localRotation;
        }

        // If thread is finished set it to null
        if (thLoad != null && thLoad.Join(0))   // "Join(0)" means that ready for 0 seconds untill thread will be stopped
        {
            thLoad = null;
        }

        // Wait for xr ready, get Eye Poses
        if (isXr && !TryGetEyesPoses(out Vector3 _lpos, out Vector3 _rpos, out Quaternion _lrot, out Quaternion _rrot))
        {
            return;
        }

        // Verify there are pixel size constant has been changed
        if (lastTexFactor != texFactor)
        {
            sendInitEvent = true;
        }

        // .ply file debugger
        if (!File.Exists(model_file_path))
        {
            lastMessage = "File '" + model_file_path + "' does not exists.";
            isInError = true;
            return;
        }

        // GaussianSplattingNI Use
        if (GaussianSplattingNI.IsAPIReady())
        {
            initialized = GaussianSplattingNI.IsInitialized();
            lastMessage = GaussianSplattingNI.GetLastMessage();

            // Load Model, Thread processing
            if (loadModelEvent)
            {
                loadModelEvent = false;
                if (thLoad == null)
                {
                    thLoad = new Thread(() => {
                        loaded = GaussianSplattingNI.LoadModel(model_file_path);
                    });
                    thLoad.Start();
                }
            }

            // Render
            if (renderEventFunc == System.IntPtr.Zero)
            {
                renderEventFunc = GaussianSplattingNI.GetRenderEventFunc();
            }

            // Render Setting - use texFactor(pixel size constant)
            if (loaded && renderEventFunc != System.IntPtr.Zero)
            {
                // Initialization
                if (sendInitEvent)
                {
                    sendInitEvent = false;
                    isInError = false;
                    countDrawErrors = 0;

                    GaussianSplattingNI.SetNbPov(isXr ? 2 : 1);
                    internalTexSize = new Vector2Int((int)((float)cam.pixelWidth * texFactor), (int)((float)cam.pixelHeight * texFactor));

                    // Set plugins parameters for pov, and pixel size
                    for (int i = 0; i < (isXr ? 2 : 1); ++i)
                    {
                        GaussianSplattingNI.SetPovParameters(i, internalTexSize.x, internalTexSize.y);
                    }
                    lastTexFactor = texFactor;

                    GL.IssuePluginEvent(renderEventFunc, GaussianSplattingNI.INIT_EVENT);


                    // Now loading is separated from init so we can wait end of initialization.
                    // Initialization finish and 1 sec has been passed, go to the next
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    while (!GaussianSplattingNI.IsInitialized() && sw.ElapsedMilliseconds < 1000)
                    {
                        Thread.Sleep(0);
                    }

                    initialized = GaussianSplattingNI.IsInitialized();

                    // if 1 sec has been passed
                    if (sw.ElapsedMilliseconds >= 1000)
                    {
                        lastMessage = GaussianSplattingNI.GetLastMessage();
                        Debug.Log("Stop Waiting for init end: " + lastMessage);
                        isInError = true;
                    }
                    // Set Crop information
                    else
                    {
                        float[] box_min = { 0, 0, 0 };
                        float[] box_max = { 1, 1, 1 };
                        GaussianSplattingNI.GetSceneSize(box_min, box_max);
                        sceneMin = new Vector3(box_min[0], box_min[1], box_min[2]);
                        sceneMax = new Vector3(box_max[0], box_max[1], box_max[2]);
                        if (initCrop)
                        {
                            cropMin = sceneMin;
                            cropMax = sceneMax;
                            curCropMin = cropMin;
                            curCropMax = cropMax;
                        }

                        initCrop = false;

                        //Init done get external texture
                        for (int i = 0; i < (isXr ? 2 : 1); ++i)
                        {
                            IntPtr texPtr = GaussianSplattingNI.GetTextureNativePointer(i);
                            tex[i] = Texture2D.CreateExternalTexture(internalTexSize.x, internalTexSize.y, TextureFormat.RGBAFloat, false, true, texPtr);

                            mat.SetTexture(i == 0 ? "_GaussianSplattingTexLeftEye" : "_GaussianSplattingTexRightEye", tex[i]);
                        }
                    }
                }

                // Cropping
                if (curCropMin != cropMin || curCropMax != cropMax)
                {
                    curCropMin = cropMin;
                    curCropMax = cropMax;
                    float[] box_min = { curCropMin.x, curCropMin.y, curCropMin.z };
                    float[] box_max = { curCropMax.x, curCropMax.y, curCropMax.z };
                    GaussianSplattingNI.SetCrop(box_min, box_max);
                }

                // Drawing
                if (sendDrawEvent)
                {
                    waitForTexture = false;
                    bool doit = true;

                    // Verify isXR - If XR is activated, Set XR Transform
                    if (isXr)
                    {
                        if (TryGetEyesPoses(out Vector3 lpos, out Vector3 rpos, out Quaternion lrot, out Quaternion rrot))
                        {
                            if (real_leye == null) { real_leye = new GameObject("real leye"); real_leye.transform.parent = cam.transform.parent; }
                            real_leye.transform.localPosition = lpos;
                            real_leye.transform.localRotation = lrot;

                            if (real_reye == null) { real_reye = new GameObject("real reye"); real_reye.transform.parent = cam.transform.parent; }
                            real_reye.transform.localPosition = rpos;
                            real_reye.transform.localRotation = rrot;
                        }
                        else
                        {
                            doit = false;
                        }
                    }

                    // Setting Texture, Transform, Culling 
                    if (doit)
                    {
                        for (int i = 0; i < (isXr ? 2 : 1); ++i)
                        {
                            if (tex[i] != null)
                            {
                                float fovy = cam.fieldOfView * Mathf.PI / 180;
                                Matrix4x4 proj_mat = cam.projectionMatrix;
                                Vector3 pos = cam.transform.position / renderScale;
                                Quaternion rot = cam.transform.rotation;

                                // if isXR = true
                                if (isXr)
                                {
                                    // Left Eye
                                    if (i == 0)
                                    {
                                        proj_mat = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                                        pos = real_leye.transform.position / renderScale;
                                        rot = real_leye.transform.rotation;
                                    }
                                    // Right Eye
                                    else
                                    {
                                        proj_mat = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                                        pos = real_reye.transform.position / renderScale;
                                        rot = real_reye.transform.rotation;
                                    }

                                }

                                // pos and rot data change form World transform to Local transform
                                pos = transform.InverseTransformPoint(pos);
                                rot = Quaternion.Inverse(transform.rotation) * rot;

                                // TODO: Move that in dll
                                rot = Quaternion.Euler(0, 0, 180) * Quaternion.Euler(rot.eulerAngles.x, -rot.eulerAngles.y, -rot.eulerAngles.z);
                                pos.y = -pos.y;

                                // Culling with frustum
                                FrustumPlanes decomp = proj_mat.decomposeProjection;
                                float[] position = { pos.x, pos.y, pos.z };
                                float[] rotation = { rot.x, rot.y, rot.z, rot.w };
                                float[] proj = matToFloat(proj_mat);
                                float[] planes = { decomp.left, decomp.right, decomp.bottom, decomp.top, decomp.zNear, decomp.zFar };

                                GaussianSplattingNI.SetDrawParameters(i, position, rotation, proj, fovy, planes);
                            }
                            else
                            {
                                doit = false;
                            }
                        }
                    }

                    // Drawing
                    if (doit)
                    {
                        GL.IssuePluginEvent(renderEventFunc, GaussianSplattingNI.DRAW_EVENT);
                        GL.InvalidateState();
                        waitForTexture = true;
                    }
                }
            }

            // Get Splatting data
            if (loaded)
            {
                nb_splats = GaussianSplattingNI.GetNbSplat();
            }
        }
    }
}


