using UnityEngine;

public class FireVFX : MonoBehaviour
{
    [HideInInspector] public FireInstance fireInstance;
    private void OnHideEarly()
    {
        fireInstance.vfx = null;
    }
}