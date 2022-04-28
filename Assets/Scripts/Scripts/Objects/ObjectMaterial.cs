using UnityEngine;

public class ObjectMaterial : MonoBehaviour
{
    public float thickness;
    public string materialType;

    [SerializeField] private bool changeOffest = false;
    [SerializeField] private bool randomOffest = false;
    [SerializeField] private bool disableInEditor = false;
    [SerializeField] private Vector2 offset;

    private void Start()
    {
        if(changeOffest)
        {
            if(randomOffest)
            {
                offset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
            GetComponent<MeshRenderer>().sharedMaterial.mainTextureOffset = offset;
        }
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (changeOffest && !disableInEditor)
        {
            if (randomOffest)
            {
                offset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
            GetComponent<MeshRenderer>().sharedMaterial.mainTextureOffset = offset;
        }
    }

#endif
}