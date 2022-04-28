using UnityEngine;
namespace SensorToolkit
{
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public class FOVCollider : MonoBehaviour 
    {
        [Tooltip("The length of the field of view cone in world units.")]
        public float length = 5f;

        [Tooltip("The size of the field of view cones base in world units.")]
        public float baseSize = 0.5f;

        [Range(1f, 180f), Tooltip("The arc angle of the fov cone.")]
        public float fovAngle = 90f;

        [Range(1f, 180f), Tooltip("The elevation angle of the cone.")]
        public float elevationAngle = 90f;

        [Range(0, 8), Tooltip("The number of vertices used to approximate the arc of the fov cone. Ideally this should be as low as possible.")]
        public int resolution = 0;

        public Mesh FOVMesh { get { return mesh; } }

        private Mesh mesh;
        private MeshCollider mc;
        private Vector3[] pts;
        private int[] triangles;

        void Awake()
        {
            mc = GetComponent<MeshCollider>();
            CreateCollider();
        }

        void OnValidate()
        {
            length = Mathf.Max(0f, length);
            baseSize = Mathf.Max(0f, baseSize);
            if (mc != null) 
            {
                CreateCollider();
            }
        }

        public void CreateCollider()
        {
            pts = new Vector3[4 + (2+resolution)*(2+resolution)];
            // There are 2 triangles on the base
            var baseTriangleIndices = 2 * 3;
            // The arc is (Resolution+2) vertices to each side, making (Resolution+1)*(Resolution+1) boxes of 2 tris each
            var arcTriangleIndices = (resolution + 1) * (resolution + 1) * 2 * 3;
            // There are 4 sides to the cone, and each side has Resolution+2 triangles
            var sideTriangleIndices = (resolution + 2) * 3;
            triangles = new int[baseTriangleIndices + arcTriangleIndices + sideTriangleIndices*4];

            // Base points
            pts[0] = new Vector3(-baseSize / 2f, -baseSize / 2f, 0f); // Bottom Left
            pts[1] = new Vector3(baseSize / 2f, -baseSize / 2f, 0f);  // Bottom Right
            pts[2] = new Vector3(baseSize / 2f, baseSize / 2f, 0f);   // Top Right
            pts[3] = new Vector3(-baseSize / 2f, baseSize / 2f, 0f);  // Top Left
            triangles[0] = 2; triangles[1] = 1; triangles[2] = 0; triangles[3] = 3; triangles[4] = 2; triangles[5] = 0;

            for (int y = 0; y < 2+resolution; y++)
            {
                for (int x = 0; x < 2+resolution; x++)
                {
                    int i = 4 + y * (2 + resolution) + x;
                    float ay = Mathf.Lerp(-fovAngle / 2f, fovAngle / 2f, (float)x / (float)(resolution + 1));
                    float ax = Mathf.Lerp(-elevationAngle / 2f, elevationAngle / 2f, (float)y / (float)(resolution + 1));
                    Vector3 p = Quaternion.Euler(ax, ay, 0f) * Vector3.forward * length;
                    pts[i] = p;

                    if (x < (1+resolution) && y < (1+resolution))
                    {
                        var ti = baseTriangleIndices + (y * (resolution + 1) + x) * 3 * 2;
                        triangles[ti] = i + 1 + (2 + resolution); // top right
                        triangles[ti + 1] = i + 1; // bottom right
                        triangles[ti + 2] = i; // bottom left
                        triangles[ti + 3] = i + (2 + resolution); // top left
                        triangles[ti + 4] = i + (2 + resolution) + 1; // top right
                        triangles[ti + 5] = i; // bottom left
                    }
                }
            }

            // Top and bottom side triangles
            for (int x = 0; x < 2+resolution; x++)
            {
                var iTop = 4 + x;
                var iBottom = 4 + (1 + resolution) * (2 + resolution) + x;

                var tiTop = baseTriangleIndices + arcTriangleIndices + x*3;
                var tiBottom = tiTop + sideTriangleIndices;
                if (x == 0)
                {
                    triangles[tiTop] = 2;
                    triangles[tiTop+1] = 3;
                    triangles[tiTop + 2] = iTop;

                    triangles[tiBottom] = 0;
                    triangles[tiBottom + 1] = 1;
                    triangles[tiBottom + 2] = iBottom;
                }
                else
                {
                    triangles[tiTop] = iTop;
                    triangles[tiTop + 1] = 2;
                    triangles[tiTop + 2] = iTop-1;

                    triangles[tiBottom] = 1;
                    triangles[tiBottom + 1] = iBottom;
                    triangles[tiBottom + 2] = iBottom-1;
                }
            }

            // Left and right side triangles
            var yIncr = 2 + resolution;
            for (int y = 0; y < 2 + resolution; y++)
            {
                var iLeft = 4 + y*(2+resolution);
                var iRight = iLeft + (1+resolution);

                var tiLeft = baseTriangleIndices + arcTriangleIndices + sideTriangleIndices*2 + y*3;
                var tiRight = tiLeft + sideTriangleIndices;
                if (y == 0)
                {
                    triangles[tiLeft] = 3;
                    triangles[tiLeft + 1] = 0;
                    triangles[tiLeft + 2] = iLeft;

                    triangles[tiRight] = 1;
                    triangles[tiRight + 1] = 2;
                    triangles[tiRight + 2] = iRight;
                }
                else
                {
                    triangles[tiLeft] = 0;
                    triangles[tiLeft + 1] = iLeft;
                    triangles[tiLeft + 2] = iLeft - yIncr;

                    triangles[tiRight] = iRight;
                    triangles[tiRight + 1] = 1;
                    triangles[tiRight + 2] = iRight - yIncr;
                }
            }

            releaseMesh();
            mesh = new Mesh();
            mc.sharedMesh = mesh;
            mesh.vertices = pts;
            mesh.triangles = triangles;
            mesh.name = "FOVColliderPoints";
            mc.convex = true;
            mc.isTrigger = true;
        }

        void releaseMesh()
        {
            if (mc.sharedMesh != null && mc.sharedMesh == mesh)
            {
                DestroyImmediate(mc.sharedMesh, true);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach(Vector3 p in pts)
            {
                Gizmos.DrawSphere(transform.TransformPoint(p), 0.1f);
            }
        }
    }
}