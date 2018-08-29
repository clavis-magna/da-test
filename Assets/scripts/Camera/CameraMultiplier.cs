using UnityEngine;
using System.Collections;

public class CameraMultiplier : MonoBehaviour {

    // for caching
    private new Transform transform;

    // default is 6, more cameras = smoother transition between cameras
    public int cameraCount = 6;
    
    public float rotationOffset = 210.0f; // 3.5 x fov of a camera (ie 3 x 60 + 30)
    public float viewportOffsetH = 0.0f; 
    public float viewportOffsetV = 0.0f;
    
    public float viewportHeight = 1f;

    // data arena dimensions
    public float screenHeight = 4.0f;
    public float screenRadius = 4.9f;
    
    public float viewerHeight = 1.2f;
    
    private Camera[] cameras;
    
    public bool disableMainCamera = true;

    public bool estimateViewFrustumOnCameras = true;
    public bool clipToArena = false;

    public float nearClipPlane = 0.3f;
    public float farClipPlane = 200.0f;

    public GameObject basePlane;

    private GameObject[] planes;

    public bool useStereo = false;
    public float eyeSeparation = 0.05f;

    void Awake() {
        transform = GetComponent<Transform> ();
    }
    
    void makeScreens() {
        
        cameras = new Camera[cameraCount];
        planes = new GameObject[cameraCount];

        // create cameras
        for (int i = 0; i < cameras.Length; i++) {
        
            // make plane representing camera view
            GameObject plane = Object.Instantiate (basePlane, transform);
            plane.SetActive(true);
            plane.name = "Plane"+ i.ToString("D3");
            plane.transform.parent = transform;
            plane.transform.Rotate(0, 0, rotationOffset + (i * (360 / cameras.Length)));
            
            // figure out screen dimensions based on camera number and screen dimensions
            float screenWidth = 2f * Mathf.Sin(Mathf.PI * 1f/cameraCount) * screenRadius;
            float screenDist = Mathf.Cos(Mathf.PI * 1f/cameraCount) * screenRadius; 
            // edges of camera frustum on screen
            plane.transform.Translate(-Vector3.up * screenDist);
            plane.transform.Translate(Vector3.forward * screenHeight * 0.5f); // half plane height, to get cameras in centre
            // plane is 10x0x10, scale based on screenHeight
            plane.transform.localScale = new Vector3(.1f * screenWidth,0f, 0.1f * screenHeight);
            planes[i] = plane;
            
            //camera pos and orientation
            GameObject go = new GameObject("Camera"+ i.ToString("D3"));
            go.AddComponent(typeof(Camera));
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.Rotate(0, rotationOffset + (i * (360 / cameras.Length)), 0);
            go.transform.Translate(Vector3.up * viewerHeight);
            //if (useStereo) go.transform.Translate (Vector3.left * eyeSeparation);

            cameras[i] = go.GetComponent<Camera>();
            cameras[i].depth = -cameraCount + i;
            
            
            // set camera rect on screen
            cameras[i].rect = new Rect(
                (2f * (viewportOffsetH + (((float) i) / cameras.Length)) % 2) / 2f,
                viewportOffsetV,
                (float) (1f / cameras.Length),
                viewportHeight
                );

            // off-axis projection settings
            cameras[i].gameObject.AddComponent(typeof(Kooima));
            Kooima kooima = cameras [i].GetComponent<Kooima> ();
            if (kooima) { 
                kooima.projectionScreen = planes[i];
                kooima.estimateViewFrustum = estimateViewFrustumOnCameras;
                kooima.setNearClipPlane = clipToArena;
                if (!clipToArena) {
                    cameras[i].nearClipPlane = nearClipPlane;
                }
                cameras[i].farClipPlane = farClipPlane;
                kooima.nearClipDistanceOffset = -0.01f;
            }
        }
        
    }

    void makeScreensStereo() {
        
        cameras = new Camera[2 * cameraCount];
        planes = new GameObject[cameraCount];

        // create cameras
        for (int i = 0; i < planes.Length; i++) {
        
            // make plane representing camera view
            GameObject plane = Object.Instantiate (basePlane, transform);
            plane.SetActive(true);
            plane.name = "Plane"+ i.ToString("D3");
            plane.transform.parent = transform;
            plane.transform.Rotate(0, 0, rotationOffset + (i * (360 / planes.Length)));
            
            // figure out screen dimensions based on camera number and screen dimensions
            float screenWidth = 2f * Mathf.Sin(Mathf.PI * 1f/cameraCount) * screenRadius;
            float screenDist = Mathf.Cos(Mathf.PI * 1f/cameraCount) * screenRadius; 
            // edges of camera frustum on screen
            plane.transform.Translate(-Vector3.up * screenDist);
            plane.transform.Translate(Vector3.forward * screenHeight * 0.5f); // half plane height, to get cameras in centre
            // plane is 10x0x10, scale based on screenHeight
            plane.transform.localScale = new Vector3(.1f * screenWidth,0f, 0.1f * screenHeight);
            planes[i] = plane;
            
            //camera pos and orientation
            // Left
            GameObject go = new GameObject("Camera_L"+ i.ToString("D3"));
            go.AddComponent(typeof(Camera));
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.Rotate(0, rotationOffset + (i * (360 / planes.Length)), 0);
            go.transform.Translate(Vector3.up * viewerHeight);
            go.transform.Translate (Vector3.left * -eyeSeparation);

            Camera cam = go.GetComponent<Camera>();
            cam.depth = -cameraCount + i;
            
            // set camera rect on screen
            cam.rect = new Rect(
                (2f * (viewportOffsetH + (((float) i) / planes.Length)) % 2) / 2f,
                viewportOffsetV,
                (float) (1f / planes.Length),
                0.5f * viewportHeight
                );

            // off-axis projection settings
            cam.gameObject.AddComponent(typeof(Kooima));
            Kooima kooima = cam.GetComponent<Kooima> ();
            if (kooima) { 
                kooima.projectionScreen = planes[i];
                kooima.estimateViewFrustum = estimateViewFrustumOnCameras;
                kooima.setNearClipPlane = clipToArena;
                if (!clipToArena) {
                    cam.nearClipPlane = nearClipPlane;
                }
                cam.farClipPlane = farClipPlane;
                kooima.nearClipDistanceOffset = -0.01f;
            }
            cameras[i * 2] = cam;
            
            
            // Right
            go = new GameObject("Camera_R"+ (i + 1).ToString("D3"));
            go.AddComponent(typeof(Camera));
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.Rotate(0, rotationOffset + (i * (360 / planes.Length)), 0);
            go.transform.Translate(Vector3.up * viewerHeight);
            go.transform.Translate (Vector3.left * eyeSeparation);

            cam = go.GetComponent<Camera>();
            cam.depth = -cameraCount + (i + i);
            
            // set camera rect on screen
            cam.rect = new Rect(
                (2f * (viewportOffsetH + (((float) i) / planes.Length)) % 2) / 2f,
                viewportOffsetV + (0.5f * viewportHeight),
                (float) (1f / planes.Length),
                0.5f * viewportHeight
                );

            // off-axis projection settings
            cam.gameObject.AddComponent(typeof(Kooima));
            kooima = cam.GetComponent<Kooima> ();
            if (kooima) { 
                kooima.projectionScreen = planes[i];
                kooima.estimateViewFrustum = estimateViewFrustumOnCameras;
                kooima.setNearClipPlane = clipToArena;
                if (!clipToArena) {
                    cam.nearClipPlane = nearClipPlane;
                }
                cam.farClipPlane = farClipPlane;
                kooima.nearClipDistanceOffset = -0.01f;
            }
            cameras[1 + (i * 2)] = cam;
            
        }
        
    }
    
    
    // Use this for initialization
    void Start () {
        if (useStereo) {
            makeScreensStereo();
        } else {
            makeScreens();
        }
        if (disableMainCamera && Camera.main) {
            Camera.main.gameObject.SetActive(false); // stop the main camera when running
        }
    }
    
    // minimum 3
    void setCameraCount(int count) {
        if (count == cameraCount) {
            return;
        }
        
        // destroy old cameras and screens
        for (int i = 0; i < cameras.Length; i++) {
            Destroy (cameras[i].gameObject);
        }
        for (int i = 0; i < planes.Length; i++) {
            Destroy (planes[i]);
        }
        
        if (count < 6) {
            count = 6;
        }

        if (count >= 72) {
            count = 72;
        }
        
        cameraCount = count;
        if (useStereo) {
            makeScreensStereo();
        } else {
            makeScreens();
        }
    }
    
    // Update is called once per frame

    void Update () {
        // less cameras per screen
        if (Input.GetKey (KeyCode.Minus) || Input.GetKey (KeyCode.KeypadMinus)) {
            setCameraCount(cameraCount - 6);
        }
        
        // more cameras per screen
        if (Input.GetKey (KeyCode.Equals) || Input.GetKey (KeyCode.KeypadPlus)) {
            setCameraCount(cameraCount + 6);
        }

        if (useStereo) {
            if (Input.GetKey (KeyCode.LeftBracket)) {
                eyeSeparation += 0.05f;
                for (int i = 0; i < cameras.Length; i += 2) {
                    cameras[i].transform.Translate (Vector3.left * -0.05f);
                    cameras[i + 1].transform.Translate (Vector3.left * 0.05f);
                }
            }

            if (Input.GetKey (KeyCode.RightBracket)) {
                eyeSeparation -= 0.05f;
                for (int i = 0; i < cameras.Length; i += 2) {
                    cameras[i].transform.Translate (Vector3.left * 0.05f);
                    cameras[i + 1].transform.Translate (Vector3.left * -0.05f);
                }
            }
        }

        if (Input.GetKey (KeyCode.PageUp)) {
            viewerHeight += 0.1f;
            foreach (Camera c in cameras) {
                c.transform.Translate(Vector3.up * 0.1f);
            }
        }
        
        if (Input.GetKey (KeyCode.PageDown)) {
            viewerHeight -= 0.1f;
            foreach (Camera c in cameras) {
                c.transform.Translate(Vector3.up * -0.1f);
            }
        }
        
    }

}
