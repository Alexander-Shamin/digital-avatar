using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin.Examples
{
    public class PhraseSwitcher : MonoBehaviour
    {
        [System.Serializable]
        public struct Phrase
        {
            public TextAsset anim;
            public AudioClip audio;
        }

        public UniversalPlayer player;
        public JsonPlayer character;
        public Phrase[] phrases;
        public float interpolation = 0.5f;

        private int phraseIndex = 0;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var phrase = phrases[phraseIndex];
                player.source.clip = phrase.audio;
                character.SetJsonToPlay(phrase.anim.text, player, interpolation);

                phraseIndex++;
                if (phraseIndex >= phrases.Length)
                    phraseIndex = 0;
            }
        }
    }
}
