using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
public class WaterPlane : MonoBehaviour
{
    public LocalVolumetricFog fog;
    public BoxCollider _collider;
    public float depth;
    public float length;
    public float width;

    public void Recalculate()
    {
        if(fog != null)
        {
            fog.parameters.size = new Vector3(width, depth, length);
            fog.transform.localPosition = new Vector3(0f, -depth / 2f, 0f);
            fog.parameters.positiveFade.y = depth / 2f;
        }

        if(_collider != null)
        {
            _collider.size = new Vector3(width , depth, length);
            _collider.center = new Vector3(0f, -depth / 2f, 0f);
        }
    }
}