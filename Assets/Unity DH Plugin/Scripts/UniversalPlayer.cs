using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace ARVRLab.UnityDH_Plugin
{
    public class UniversalPlayer : MonoBehaviour
    {
        public AudioSource source;
        public VideoPlayer player;

        [HideInInspector]
        public float percentTime;
        [HideInInspector]
        public float absoluteTime;

        private void Update()
        {
            if (player)
            {
                var totalTime = player.clip.length;
                percentTime = (float)(player.time / totalTime);
                absoluteTime = (float)player.time;
            }
            else if (source && source.clip != null)
            {
                var totalTime = source.clip.length;
                percentTime = source.time / totalTime;
                absoluteTime = source.time;
            }
        }

        public void Play()
        {
            if (player)
            {
                player.Play();
            }
            else if (source && source.clip != null)
            {
                source.Play();
            }
        }
    }
}