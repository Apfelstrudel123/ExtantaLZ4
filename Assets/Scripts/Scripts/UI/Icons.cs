using UnityEngine;
using Gameplay;
namespace UI
{
    public class Icons : MonoBehaviour
    {
        public float minDistance = 1f;
        public float maxDistance = 50f;

        public GameObject icon = null;
        public GameObject text = null;
        public string ending = "m";

        private void Update()
        {
            if (Core.Game.GameState == GameState.Active)
            {
                Vector3 invPos = Player.Transform.position + 2 * (transform.position - Player.Transform.position);
                transform.LookAt(invPos, Vector3.up);
                float f = (transform.position - Player.Transform.position).magnitude;
                transform.localScale = new Vector3(f, f, f);
            }
        }
    }
}