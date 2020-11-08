using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeformableTerrainController : MonoBehaviour
{
    public DeformableTerrainManager manager;
    public int priorityIndex = 0;
    public bool EnableUpdate = true;
    [Range(0, 30)]
    [Tooltip("Lower means higher priority")]
    public int priority = 0;
    int lastpriority = 0;
    public ComputeShader computeAccumulate;
    int computeKernelMax;
    int computeKernelMin;
    int computeKernelClear;
    int computeKernelPassthrough;
    int computeKernelLerp;
    public LayerMask renderMask;
    [Range(0,.25f)]
    public float SnowAccum = .25f;
    public Vector3 dir = new Vector3(-90, 0, 0);
    public Vector2 OffsetDepth = new Vector2(0, .5f);
    public Vector2 MeshScale = new Vector2(5, 5);
    public Vector2Int MeshResolution = new Vector2Int(255, 255);
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    public Vector2Int CameraResolutionMult = new Vector2Int(64, 64);
    const int CameraResolutionBase = 4;
    public GameObject camHolder;
    public Camera cam;
    public RenderTexture rtex;
    public RenderTexture tex;
    public bool AutoLoad = true;
    public bool AutoSave = true;
    public Texture2D ttex;
    //[Range(0, 10)]
    //public int Detail_BlurRadius = 1;
    //int Detail_lastBlurRadius = 1;
    //[Range(0, 10)]
    //public int Detail_BlurSteps = 2;
    //public RenderTexture dtex;
    public bool forceGenerate = true;
    public bool generated = false;
    public Mesh mesh;
    public MeshFilter meshf;
    public Renderer rend;
    public Material mat;
    public string depthTexName = "_Deformable_Main";
    public string depthResName = "_Deformable_Main_Res";
    public string depthHeightName = "_Deformable_Main_Height";
    float updateAccum;

    void Awake()
    {
        manager = FindObjectOfType<DeformableTerrainManager>();
        priorityIndex = manager.terrains.Count;
        manager.terrains.Add(this);
        if (!rend)
            rend = GetComponent<Renderer>();
        if (!meshf)
            meshf = rend.GetComponent<MeshFilter>();
        if (!meshf.mesh || forceGenerate)
            CreateMesh();
        else
            mesh = meshf.mesh;
        mat = new Material(rend.material);
        rend.material = mat;
        SetupCamera();
    }

    public void CreateMesh()
    {
        //MeshResolution = new Vector2Int(MeshResolution.x +1, MeshResolution.y +1);
        MeshResolution.x = Mathf.Clamp(MeshResolution.x, 4, 255);
        MeshResolution.y = Mathf.Clamp(MeshResolution.y, 4, 255);
        vertices = new Vector3[(MeshResolution.x + 1) * (MeshResolution.y + 1)];
        uvs = new Vector2[(MeshResolution.x + 1) * (MeshResolution.y + 1)];
        float xScale = 1.0f / (float)MeshResolution.x;
        float yScale = 1.0f / (float)MeshResolution.y;
        for (int i = 0, z = 0; z <= MeshResolution.y; z++)
        {
            for (int x = 0; x <= MeshResolution.x; x++)
            {
                vertices[i] = new Vector3((x * MeshScale.x * xScale * 2) - MeshScale.x, 0, (z * MeshScale.y * yScale * 2) - MeshScale.y);
                uvs[i] = new Vector2((x * xScale), (z * yScale));
                i++;
            }
        }
        triangles = new int[MeshResolution.x * MeshResolution.y * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < MeshResolution.y; z++)
        {
            for (int x = 0; x < MeshResolution.x; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + MeshResolution.x + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + MeshResolution.x + 1;
                triangles[tris + 5] = vert + MeshResolution.x + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }
        mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        meshf.mesh = mesh;
        generated = true;
    }

    public void SetupCamera()
    {
        //if (camHolder)
        //{
        //    DestroyImmediate(camHolder);
        //}
        if (!camHolder)
        {
            camHolder = new GameObject("DeformableTerrainCamera");
            camHolder.transform.parent = transform;
            camHolder.transform.position = transform.position + new Vector3(0, OffsetDepth.x, 0);
            camHolder.transform.localRotation = Quaternion.Euler(dir);
            cam = camHolder.AddComponent<Camera>();
        }
        else
        {
            camHolder = Instantiate(camHolder, transform);
            camHolder.transform.position = transform.position + new Vector3(0, OffsetDepth.x, 0);
            camHolder.transform.localRotation = Quaternion.Euler(dir);
            cam = camHolder.GetComponent<Camera>();
        }
        cam.orthographic = true;
        //computeKernelSimpleBlur8 = computeBlur.FindKernel("SimpleBlur8");
        computeKernelMax = computeAccumulate.FindKernel("Max");
        computeKernelMin = computeAccumulate.FindKernel("Max");
        computeKernelClear = computeAccumulate.FindKernel("Clear");
        computeKernelPassthrough = computeAccumulate.FindKernel("Passthrough");
        computeKernelLerp = computeAccumulate.FindKernel("LerpTexture");
        rtex = new RenderTexture(CameraResolutionMult.x * CameraResolutionBase, CameraResolutionMult.y * CameraResolutionBase, 16, RenderTextureFormat.Depth);
        rtex.wrapMode = TextureWrapMode.Repeat;
        rtex.autoGenerateMips = false;
        //rtex.enableRandomWrite = true;
        rtex.Create();
        tex = new RenderTexture(CameraResolutionMult.x * CameraResolutionBase, CameraResolutionMult.y * CameraResolutionBase, 0, RenderTextureFormat.ARGBHalf);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.autoGenerateMips = false;
        tex.enableRandomWrite = true;
        tex.Create();
            LoadTexture();
        if (ttex || AutoLoad)
        {
        }
        else
        {
            ttex = new Texture2D(CameraResolutionMult.x * CameraResolutionBase, CameraResolutionMult.y * CameraResolutionBase, TextureFormat.ARGB32, false);
        }
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.black;
        cam.cameraType = CameraType.SceneView;
        cam.depthTextureMode = DepthTextureMode.Depth;
        if (OffsetDepth.x <= 0.01)
            cam.nearClipPlane = 0.01f;
        else
            cam.nearClipPlane = OffsetDepth.x;
        cam.farClipPlane = OffsetDepth.y;
        cam.orthographicSize = Mathf.Max(MeshScale.x, MeshScale.y) * Mathf.Max(transform.localScale.x, transform.localScale.z);
        cam.cullingMask = renderMask;
        cam.targetTexture = rtex;
        rend.material.SetTexture(depthTexName, tex);
        rend.material.SetFloat(depthHeightName, OffsetDepth.y);
        rend.material.SetVector(depthResName, new Vector4(CameraResolutionMult.x * CameraResolutionBase, CameraResolutionMult.y * CameraResolutionBase, 0, 0));
        computeAccumulate.SetVector("ColorLerp", new Vector4(0, 0, 0, 0));
        UpdateTexture();
    }

    void Update()
    {
        try { updateAccum += Time.deltaTime * GameManager.Instance.atmosphericsController.Rain * SnowAccum; } catch { }
        if (EnableUpdate)
        {
            if (lastpriority == priority)
            {

            }
            else
            {
                lastpriority = priority;
                if (priority <= 0)
                {
                    cam.enabled = true;
                }
                else
                {
                    cam.enabled = false;
                }
            }
            if (priority <= 0)
            {
                computeAccumulate.SetTexture(computeKernelMax, "Input", rtex);
                computeAccumulate.SetTexture(computeKernelMax, "Result", tex);
                computeAccumulate.Dispatch(computeKernelMax, CameraResolutionMult.x, CameraResolutionMult.y, 1);
                if(updateAccum > 0f)
                {
                    computeAccumulate.SetTexture(computeKernelLerp, "Result", tex);
                    computeAccumulate.SetFloat("ColorLerpT", updateAccum);
                    computeAccumulate.Dispatch(computeKernelLerp, CameraResolutionMult.x, CameraResolutionMult.y, 1);
                    updateAccum = 0;
                }
                if (Time.frameCount % 4 != priorityIndex % 4)
                {

                }
                else
                {
                    UpdateTexture();
                }
            }
            else
            {
                if (Time.frameCount % priority != priorityIndex % priority)
                {

                }
                else
                {
                    cam.Render();
                    computeAccumulate.SetTexture(computeKernelMax, "Input", rtex);
                    computeAccumulate.SetTexture(computeKernelMax, "Result", tex);
                    computeAccumulate.Dispatch(computeKernelMax, CameraResolutionMult.x, CameraResolutionMult.y, 1);
                    if (updateAccum > 0f)
                    {
                        computeAccumulate.SetTexture(computeKernelLerp, "Result", tex);
                        computeAccumulate.SetFloat("ColorLerpT", updateAccum);
                        computeAccumulate.Dispatch(computeKernelLerp, CameraResolutionMult.x, CameraResolutionMult.y, 1);
                        updateAccum = 0;
                    }
                }
                if (Time.frameCount % (priority * 4) != (priorityIndex + 1) % (priority * 4))
                {
                    //Debug.Log("ko:" + Time.frameCount % priority * 4 + ":" + priorityIndex + 1 % priority * 4);
                }
                else
                {
                    //Debug.Log("UpdatedTexture");
                    UpdateTexture();
                }
            }
        }
        else
        {
            priority = 60;
            cam.enabled = false;
        }
    }

    public void UpdateTexture()
    {
        RenderTexture tempRender = RenderTexture.active;
        RenderTexture.active = tex;
        ttex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        ttex.Apply();
        RenderTexture.active = tempRender;
    }

    public System.Tuple<Color, Vector2> GetDepth(Transform pos)
    {
        Vector2 PlanePos = new Vector2(transform.position.x, transform.position.z);
        Vector2 TransformPos = new Vector2(pos.position.x, pos.position.z);
        Vector2 relativePos = TransformPos - PlanePos;
        if (generated)
        {
            Vector2 relativePosScaled = relativePos / (MeshScale);
            if (relativePosScaled.x > 1f || relativePosScaled.y > 1f || relativePosScaled.x < -1f || relativePosScaled.y < -1f)
            {
                return new System.Tuple<Color, Vector2>(Color.clear, relativePosScaled);
            }
            else
            {
                return new System.Tuple<Color, Vector2>(ttex.GetPixelBilinear(relativePosScaled.x - 0.5f, relativePosScaled.y - 0.5f), relativePosScaled);
            }
        }
        else
        {
            return new System.Tuple<Color, Vector2>(Color.clear, relativePos);
        }
    }

    public float RelativeDistance(Transform pos)
    {
        Vector2 PlanePos = new Vector2(transform.position.x, transform.position.z);
        Vector2 TransformPos = new Vector2(pos.position.x, pos.position.z);
        Vector2 relativePos = TransformPos - PlanePos;
        if (generated)
        {
            Vector2 relativePosScaled = relativePos / (MeshScale);
            if (relativePosScaled.x > 1f || relativePosScaled.y > 1f || relativePosScaled.x < -1f || relativePosScaled.y < -1f)
            {
                relativePosScaled.x = Mathf.Abs(relativePosScaled.x);
                relativePosScaled.y = Mathf.Abs(relativePosScaled.y);
                if (relativePosScaled.x < 1f || relativePosScaled.y < 1f)
                {
                    return Mathf.Max(relativePosScaled.x - .5f, relativePosScaled.y - .5f);
                }
                else
                {
                    return Mathf.Min(relativePosScaled.x - .5f, relativePosScaled.y - .5f);
                }
            }
            else
            {
                return 0f;
            }
        }
        else
        {
            return 10000f;
        }
    }

    void OnDestroy()
    {
        if (AutoSave)
        {
            UpdateTexture();
            SaveTexture();
        }
    }

    public void SaveTexture()
    {
        try
        {
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name);

            byte[] bytes = ttex.EncodeToPNG();

            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "/" + priorityIndex + ".png", bytes);
        }
        catch { }
    }
    public void LoadTexture()
    {
        Debug.Log(Application.persistentDataPath);
        if (AutoLoad)
        {
            if (!ttex)
                ttex = new Texture2D(CameraResolutionMult.x * CameraResolutionBase, CameraResolutionMult.y * CameraResolutionBase, TextureFormat.ARGB32, false);
            try
            {
                if (System.IO.File.Exists(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "/" + priorityIndex + ".png"))
                {

                    byte[] bytes = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "/" + priorityIndex + ".png");
                    ttex = new Texture2D(1, 1);
                    ttex.LoadImage(bytes);
                    ttex.Apply();
                }
            }
            catch { }
        }
        else if (!ttex)
        {
            ttex = new Texture2D(CameraResolutionMult.x * CameraResolutionBase, CameraResolutionMult.y * CameraResolutionBase, TextureFormat.ARGB32, false);
        }
        RenderTexture lastActive = RenderTexture.active;
        RenderTexture.active = tex;
        // Copy your texture ref to the render texture
        Graphics.Blit(ttex, tex);
        RenderTexture.active = lastActive;
        ttex = new Texture2D(CameraResolutionMult.x * CameraResolutionBase, CameraResolutionMult.y * CameraResolutionBase, TextureFormat.ARGB32, false);
        UpdateTexture();
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //        return;
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], .1f);
    //    }
    //}
}
