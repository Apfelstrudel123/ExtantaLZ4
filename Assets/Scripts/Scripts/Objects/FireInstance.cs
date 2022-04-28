using UnityEngine;
using Gameplay;
public class FireInstance : MonoBehaviour
{
    [SerializeField] private int damagePerSecond;
    [HideInInspector]public GameObject vfx;

    private void OnTriggerStay(Collider other)
    {
        if (Core.Game.GameState != GameState.Active)
            return;

        if (other.TryGetComponent<ObjectHealth>(out ObjectHealth oh))
        {
            oh.AddHealth(-damagePerSecond * Time.deltaTime, ObjectHealth.DamageType.Fire);
        }
    }

    public void Recalculate()
    {
        float angle = Vector3.Angle(Cam.active.transform.forward, transform.position - Cam.active.transform.position);
        if(vfx != null && angle > 90)
        {
            vfx.GetComponent<PoolObjectRuntime>().Hide();
            vfx = null;
        }
        else if(vfx == null && angle < 90)
        {
            Core.ObjectPool.Request("FireVFX", transform.position, Quaternion.Euler(0, 0, 0), OnVFXLoaded);
        }
    }

    private void OnVFXLoaded(GameObject g)
    { 
        vfx = g;
        g.GetComponent<FireVFX>().fireInstance = this;
    }
}
