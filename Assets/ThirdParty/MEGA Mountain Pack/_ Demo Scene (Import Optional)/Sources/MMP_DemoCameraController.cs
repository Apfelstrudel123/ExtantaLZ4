using System.Collections;
using UnityEngine;

namespace MMP
{
    public class MMP_DemoCameraController : MonoBehaviour
    {
        // Public Variables ========================================================================
        [Header("Camera Control")]
        [Range(0.1f, 3.0f)]
        public float lookSensitivity = 1;
        [Range(0.1f, 10.0f)]
        public float moveSensitivity = 5;
        public Vector3 worldLimits = new Vector3(1000, 500, 1000);

        [Header("Zooming")]
        public float fov1 = 30f;
        public float fov2 = 60f;
        public float fov3 = 15f;
        public float fovLerpDuration = 1;

        [Header("Sun Animation")]
        public Transform sunT;
        public Light sunL;
        public AnimationCurve sunRotX = new AnimationCurve(new Keyframe(0, 5), new Keyframe(15, 30), new Keyframe(30, 5));
        public AnimationCurve sunRotY = AnimationCurve.Linear(0, 0, 30, 360);
        public AnimationCurve sunIntensity = new AnimationCurve(new Keyframe(0, 1.3f), new Keyframe(15, 1.2f), new Keyframe(30, 1.3f));
        public bool animateSun;
        public AnimationCurve atmosphereThickness;
        public Material sky;
        public Gradient fogColor;

        [Header("PostFX Reference")]
        public GameObject postFX;

        [Header("Mountains")]
        public Transform mountainParent;
        public GameObject[] mountainSets;
        int currentMountainSet = 2;

        // Private Variables =======================================================================
        float fovTargetValue = 30;
        Transform t;
        Transform child;
        Transform camT;
        RaycastHit hit;
        Ray ray = new Ray();
        Rect r = new Rect();
        RaycastHit dofHit;
        float targetPan, targetPitch;
        Vector3 targetPos;
        [HideInInspector] public float myTime;
        int lod;
        bool hideGUI;
        //==========================================================================================
        //==========================================================================================
        //==========================================================================================



        void Start()
        {
            //sunRotX.postWrapMode = WrapMode.Loop;
            //sunRotY.postWrapMode = WrapMode.Loop;
            //sunIntensity.postWrapMode = WrapMode.Loop;
            SetupCameraRig();
            targetPos = transform.position;
            fovTargetValue = fov1;
            QualitySettings.shadowCascades = 1;
            QualitySettings.shadowDistance = 2000;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (sideScroll) CloneMaterials();
        } //========================================================================================



        void OnGUI()
        {
            float sw = Screen.width;
            float sh = Screen.height;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.wordWrap = true;
            style.richText = true;

            if (!hideGUI)
            {
                string text = 
                    "WASD/QE\n" +
                    "LMB/RMB\n" +
                    "Scrollwheel\n" +
                    "Space\n" +
                    "C";
                style.fontStyle = FontStyle.Bold;
                r.Set(30, sh - 85, 300, 500);
                UnityEngine.GUI.Label(r, text, style);
                r.x += 80;

                text = 
                    "=\n" +
                    "=\n" +
                    "=\n" +
                    "=\n" +
                    "=";
                style.fontStyle = FontStyle.Normal;
                UnityEngine.GUI.Label(r, text, style);
                r.x += 15;

                text = 
                    "Move\n" +
                    "Zoom\n" +
                    "Change Mountain Height\n" +
                    "Animate Sun\n" +
                    "Lock/Unlock Camera";
                style.fontStyle = FontStyle.Normal;
                UnityEngine.GUI.Label(r, text, style);
                r.x += 170;

                string pfx1 = "4\n";
                string pfx2 = "=\n";
                string pfx3 = "Toggle PostFX\n";

                if (postFX == null)
                {
                    pfx1 = "\n";
                    pfx2 = "\n";
                    pfx3 = "\n";
                }


                text = 
                    "1\n" +
                    "2\n" +
                    "3\n" +
                    pfx1 +
                    "5";
                style.fontStyle = FontStyle.Bold;
                UnityEngine.GUI.Label(r, text, style);
                r.x += 15;

                text = 
                    "=\n" +
                    "=\n" +
                    "=\n" +
                    pfx2 +
                    "=";
                style.fontStyle = FontStyle.Normal;
                UnityEngine.GUI.Label(r, text, style);
                r.x += 15;

                text = 
                    "Switch Mountain Set\n" +
                    " Change Max LOD (" + lod + ")\n" +
                    "Toggle Ground Plane\n" +
                    pfx3 +
                    "Reset";
                style.fontStyle = FontStyle.Normal;
                UnityEngine.GUI.Label(r, text, style);
                r.x += 120;
            }
        } //========================================================================================

        [Header("Scrolling")]
        public bool sideScroll;
        public Vector2 rangeX;
        public float z, y;
        public float speed;
        float progress;

        [Header("Material Lerping")]
        public Material[] materials;
        public float lerpSpeed;
        int matIndex = 0;
        float lerpVal = -4;
        Material[] tempMaterials;
        Material tempMaterial;



        void CloneMaterials()
        {
            //tempMaterials = new Material[5];
            //for (int i = 0; i < tempMaterials.Length; i++)
            //{
            //    tempMaterials[i] = new Material(materials[0]);
            //}


            //foreach (GameObject go in mountainSets)
            //{
            //    Transform parent = go.transform;
            //    MeshRenderer[] mr = parent.GetComponentsInChildren<MeshRenderer>();
            //}


            tempMaterial = new Material(materials[0]);
            MeshRenderer[] renderers = mountainParent.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var mr in renderers)
            {
                mr.sharedMaterial = tempMaterial;
            }

        }

        void MaterialLerp()
        {
            lerpVal += Time.deltaTime * lerpSpeed;
            if (lerpVal > 2)
            {
                lerpVal = 0;
                matIndex++;

                //if (matIndex >= 0 && matIndex < 7)
                //{
                //    mountainSets[currentMountainSet].SetActive(false);
                //    currentMountainSet = 0;
                //    mountainSets[currentMountainSet].SetActive(true);
                //}
                //if (matIndex >= 7 && matIndex < 14)
                //{
                //    mountainSets[currentMountainSet].SetActive(false);
                //    currentMountainSet = 1;
                //    mountainSets[currentMountainSet].SetActive(true);
                //}
                //if (matIndex >= 14 && matIndex < 21)
                //{
                //    mountainSets[currentMountainSet].SetActive(false);
                //    currentMountainSet = 2;
                //    mountainSets[currentMountainSet].SetActive(true);
                //}

            }
            //if (matIndex >= materials.Length) matIndex = 0;

            int matIndex1 = matIndex;//(int)Mathf.Repeat(matIndex + 1, materials.Length);
            if (matIndex >= materials.Length -1) { matIndex1 = 6;  lerpVal = 0; }
            int matIndex2 = (int)Mathf.Repeat(matIndex1 + 1, materials.Length);



            tempMaterial.Lerp(materials[matIndex1], materials[matIndex2], Mathf.SmoothStep(0,1,Mathf.Clamp01(lerpVal)));
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) ScreenCapture.CaptureScreenshot("Screenshot.png", 1); 

            if (animateSun)
            {
                myTime += Time.unscaledDeltaTime;

                sunT.eulerAngles = new Vector3(sunRotX.Evaluate(myTime), sunRotY.Evaluate(myTime), sunT.eulerAngles.z);
                sunL.intensity = sunIntensity.Evaluate(myTime);

                sky.SetFloat("_AtmosphereThickness", atmosphereThickness.Evaluate(sunT.eulerAngles.x));
                RenderSettings.fogColor = fogColor.Evaluate(sunT.eulerAngles.x / 25);
            }


            if (sideScroll)
            {
                progress += Time.deltaTime * speed;
                progress = Mathf.Repeat(progress, 1);
                float x = Mathf.Lerp(rangeX.x, rangeX.y, progress);
                Vector3 pos = new Vector3(x, y, z);
                t.position = pos;

                //return;
            }

            if (sideScroll) MaterialLerp();



            if (Cursor.lockState == CursorLockMode.Locked && !sideScroll) CameraControl();
            if (Input.GetMouseButtonDown(0)) fovTargetValue = fov3;
            if (Input.GetMouseButtonUp(0)) fovTargetValue = fov1;
            if (Input.GetMouseButtonDown(1)) fovTargetValue = fov2;
            if (Input.GetMouseButtonUp(1)) fovTargetValue = fov1;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fovTargetValue, Time.unscaledDeltaTime*fovLerpDuration);

            // Switch Mountains
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchMountain();
                ResetSettings();
                //mountainParent.localScale = new Vector3(1, 0.2f, 1);
            }
            // Switch LOD
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                lod++; 
                lod %= 8;
                QualitySettings.maximumLODLevel = lod;
            }

            // Enable/Disable Ground
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                foreach (GameObject mountainSet in mountainSets)
                {
                    GameObject go = mountainSet.transform.GetChild(0).gameObject;
                    go.SetActive(!go.activeSelf);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) && postFX != null) postFX.SetActive(!postFX.activeSelf);

            // Reset
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ResetSettings();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                animateSun = !animateSun;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }





            if (Input.GetKeyDown(KeyCode.F12))
            {
                hideGUI = !hideGUI;
                if (hideGUI)
                {
                    Camera.main.rect = new Rect(0,0,1,1);
                }
                else
                {
                    Camera.main.rect = new Rect(0, 0.1f, 1, 0.8f);
                }
            }


            float scale = mountainParent.localScale.y;
            if (Input.GetKey(KeyCode.KeypadPlus))
            {
                scale += Time.deltaTime * 0.5f;
            }
            if (Input.GetKey(KeyCode.KeypadMinus))
            {
                scale -= Time.deltaTime * 0.5f;
            }
            scale += Input.mouseScrollDelta.y * 0.05f;

            scale = Mathf.Clamp(scale, 0.2f, 1.0f);
            mountainParent.localScale = new Vector3(1, scale, 1);

        } //========================================================================================


        private void ResetSettings()
        {
            lod = 0;
            QualitySettings.maximumLODLevel = lod;
            mountainParent.localScale = Vector3.one;

            //foreach (GameObject mountainSet in mountainSets)
            //{
            //    GameObject go = mountainSet.transform.GetChild(0).gameObject;
            //    go.SetActive(false);
            //}
            if (postFX != null) postFX.SetActive(true);
        }


        void SwitchMountain()
        {
            mountainSets[currentMountainSet].SetActive(false);
            currentMountainSet++;
            currentMountainSet %= mountainSets.Length;
            mountainSets[currentMountainSet].SetActive(true);
        }



        void CameraControl()
        {

            targetPan += Input.GetAxisRaw("Mouse X") * lookSensitivity;
            targetPitch -= Input.GetAxisRaw("Mouse Y") * lookSensitivity;
            t.localEulerAngles += new Vector3(0, Input.GetAxisRaw("Mouse X") * lookSensitivity, 0);
            child.localEulerAngles -= new Vector3(0, 0, Input.GetAxisRaw("Mouse Y") * lookSensitivity);
            Vector3 motion = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) motion.z += 1;
            if (Input.GetKey(KeyCode.S)) motion.z -= 1;
            if (Input.GetKey(KeyCode.D)) motion.x += 1;
            if (Input.GetKey(KeyCode.A)) motion.x -= 1;
            if (Input.GetKey(KeyCode.E)) motion.y += 1;
            if (Input.GetKey(KeyCode.Q)) motion.y -= 1;
            if (Input.GetKey(KeyCode.LeftShift)) motion *= 5;
            t.position += camT.TransformDirection(motion) * Time.unscaledDeltaTime * 16 * moveSensitivity;

            // Limits
            ray.origin = t.position + Vector3.up * 200;
            if (Physics.SphereCast(ray, 2f, out hit, 220f)) if (t.position.y < hit.point.y + 2f) t.position = new Vector3(t.position.x, hit.point.y + 2f, t.position.z);
            if (t.position.y > worldLimits.y) t.position = new Vector3(t.position.x, worldLimits.y, t.position.z);
            if (t.position.y < 2) t.position = new Vector3(t.position.x, 2, t.position.z);
            if (t.position.x > worldLimits.x) t.position = new Vector3(worldLimits.x, t.position.y, t.position.z);
            if (t.position.x < -worldLimits.x) t.position = new Vector3(-worldLimits.x, t.position.y, t.position.z);
            if (t.position.z > worldLimits.z) t.position = new Vector3(t.position.x, t.position.y, worldLimits.z);
            if (t.position.z < -worldLimits.z) t.position = new Vector3(t.position.x, t.position.y, -worldLimits.z);
        } //========================================================================================



        void SetupCameraRig()
        {
            t = transform;
            Transform[] children = t.GetComponentsInChildren<Transform>();
            child = new GameObject("Tilt").transform;
            child.parent = t;
            child.localPosition = Vector3.zero;
            child.transform.localEulerAngles = new Vector3(0, 90, 33);
            for (int i = 0; i < children.Length; i++) { if (children[i] != t) children[i].parent = child; }
            camT = Camera.main.transform;
            camT.parent = child;
            camT.forward = t.forward;
            camT.localPosition = Vector3.zero;
            camT.localRotation = Quaternion.Euler(0, -90, 0);
            ray.direction = Vector3.down;
        } //========================================================================================
    }
}
