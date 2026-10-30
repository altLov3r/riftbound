using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    public partial class PlayerController
    {
        readonly HashSet<string> landmarks = new HashSet<string>();
        public int LandmarksFound => landmarks.Count;
        public bool HasLandmark(string id) => landmarks.Contains(id);
        static bool ValidLandmark(string id) =>
            id == "willow-crossing" || id == "greenhollow-post" || id == "ancient-grove" ||
            id == "scarlet-crypts" || id == "redridge-frontier";
        public Landmark NearbyLandmark()
        {
            Landmark nearest = null; float distance = 3.5f;
            foreach (var landmark in FindObjectsByType<Landmark>(FindObjectsSortMode.None))
            {
                float current = Vector3.Distance(transform.position, landmark.transform.position);
                if (!HasLandmark(landmark.landmarkId) && current < distance) { distance = current; nearest = landmark; }
            }
            return nearest;
        }
        public void DiscoverLandmark(string id)
        {
            if (!Stats.Alive || FrontendMenu.IsOpen || MountWindowOpen || (orbit && orbit.Paused) || !ValidLandmark(id) || !landmarks.Add(id)) return;
            Gold += 15; AddExperience(35);
            if (id == "scarlet-crypts")
            {
                Message = "Discovered: Scarlet Crypts! Cathedral dungeon entrance in the mists of Duskwood. +25 gold / +50 XP.";
                AddExperience(25);
            }
            else if (id == "redridge-frontier")
            {
                Message = "Discovered: Redridge Frontier! Mountain pass to the Orc Badlands. +25 gold / +50 XP.";
                AddExperience(25);
            }
            else
            {
                Message = LandmarksFound >= 3 ? "Greenhollow explored! Zebra mount unlocked. [V] Change mount / [Z] Summon" : $"Greenhollow landmark discovered: {LandmarksFound}/3. +10 gold / +25 XP.";
                if (LandmarksFound == 3) AddExperience(50);
            }
            GameSound.Play(SoundCue.Rune, transform.position); SaveAdventure(false);
        }
    }
}
