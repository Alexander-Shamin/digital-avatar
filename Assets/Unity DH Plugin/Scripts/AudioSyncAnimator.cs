using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin
{
    public class AudioSyncAnimator : MonoBehaviour
    {
        private Animator anim;
        public UniversalPlayer player;

        void Start()
        {
            anim = GetComponent<Animator>();
            anim.enabled = false;
        }

        void Update()
        {
            // Change time for currently played state
            anim.Play(0, 0, player.percentTime);
            anim.Update(0.0f);
        }
    }
}