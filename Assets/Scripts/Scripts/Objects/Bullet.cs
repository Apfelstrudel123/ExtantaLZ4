using UnityEngine;
using System.Collections;
public class Bullet : MonoBehaviour
{
	private PoolObjectRuntime obj;
	[HideInInspector]public int dmg;

    private void Start()
    {
		obj = GetComponent<PoolObjectRuntime>();
		//obj.onRecycled.AddListener(Recycle);
    }

	private void OnCollisionEnter(Collision col)
	{
		Vector3 point = col.GetContact(0).point;
		Vector3 normal = col.GetContact(0).normal;
		if (col.collider.gameObject.TryGetComponent<AI.AIHitbox>(out AI.AIHitbox hitbox))
		{
			hitbox.TakeDamage(dmg);

			Core.ObjectPool.Request("ImpactBlood", point, Quaternion.LookRotation(normal), null);
		}
		else if (col.gameObject.TryGetComponent<ObjectHealth>(out ObjectHealth health))
		{
			health.SubtractHealth(dmg, ObjectHealth.DamageType.Bullet);
		}

		if (col.transform.TryGetComponent<GameWorld.TerrainInstance>(out GameWorld.TerrainInstance t))
		{
			Core.ObjectPool.Request("ImpactDirt", point, Quaternion.LookRotation(normal), null);
		}
		else if (col.transform.TryGetComponent<ObjectMaterial>(out ObjectMaterial mat))
		{
			Core.ObjectPool.Request("Impact" + mat.materialType, point, Quaternion.LookRotation(normal), null);
		}

		obj.Hide();
	}
}