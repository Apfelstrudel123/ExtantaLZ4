using UnityEngine;

namespace ScriptableObjects
{
	[CreateAssetMenu(menuName = "ScriptableObjects/PlayerStateData")]
	public class PlayerStateData : ScriptableObject
    {
		public bool canLean;
		public bool canAim;
		public bool stayOnAim;
		public bool clampYaw;
		public float minYawAngle;
		public float maxYawAngle;
		public bool canTurnOnLight;

		public Vector3 headCheck;
		public Vector3 groundCheck;
		public Vector3 camPosition;

		[Header("GroundCollider")]
		public int colliderDirection;
		public float height;
		public float radius;
		public float wallHeight;
		public float wallRadius;
	}
}
