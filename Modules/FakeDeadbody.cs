using UnityEngine;

namespace LethalSanity.Modules
{
    internal class FakeDeadbody : MonoBehaviour
    {
        DeadBodyInfo DeadBodyInfo;
        void Start()
        {
            DeadBodyInfo = this.GetComponent<DeadBodyInfo>();
        }
        void Update()
        {
            if (DeadBodyInfo.seenByLocalPlayer && !IsVisibleFromCamera())
            {
                UnityEngine.Object.Destroy(this.gameObject);
            }

            Main.mls.LogFatal($"{DeadBodyInfo.seenByLocalPlayer} || {!IsVisibleFromCamera()}");
        }

        private bool IsVisibleFromCamera()
        {
            Vector3 viewportPosition = LocalPlayer.PlayerController.gameplayCamera.WorldToViewportPoint(transform.position);

            // Check if the object is within the camera's viewport
            return (viewportPosition.z > 0 && ((viewportPosition.x > 0 && viewportPosition.x < 1.25f) && (viewportPosition.y > 0 && viewportPosition.y < 1.25f)));
        }
    }
}
