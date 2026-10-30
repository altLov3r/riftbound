using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riftbound
{
    public static class PrototypeEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OpenCurrentAdventure()
        {
            if (SceneManager.GetActiveScene().name != "Outpost") return;
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player && Application.CanStreamedLevelBeLoaded("Outpost-Updated"))
                SceneManager.LoadScene("Outpost-Updated");
        }
    }
}
