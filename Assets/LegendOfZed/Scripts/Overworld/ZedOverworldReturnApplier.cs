using TopDownShooter;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    public class ZedOverworldReturnApplier : MonoBehaviour
    {
        public bool ApplyOnStart = true;
        public float SpawnYOffset = 0.15f;

        private void Start()
        {
            if (ApplyOnStart)
            {
                ApplyReturnState();
            }
        }

        [ContextMenu("Apply Return State")]
        public void ApplyReturnState()
        {
            if (!ZedOverworldReturnState.HasReturnState)
            {
                return;
            }

            ZedOverworldGenerationManager manager = FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager != null)
            {
                manager.OverworldSeed = ZedOverworldReturnState.OverworldSeed;
                manager.GenerateOrReuse();
            }

            Transform player = FindPlayer();
            if (player != null)
            {
                player.position = ZedOverworldReturnState.ReturnPosition + Vector3.up * SpawnYOffset;
                player.rotation = ZedOverworldReturnState.ReturnRotation;
            }

            Debug.Log("Applied overworld return state from portal " + ZedOverworldReturnState.LastPortalId + " in scene " + SceneManager.GetActiveScene().name + ".", this);
        }

        private static Transform FindPlayer()
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                return playerController.transform;
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            return tagged != null ? tagged.transform : null;
        }
    }
}
