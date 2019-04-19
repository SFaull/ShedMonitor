using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace SmartMonitorApp
{
    public static class Audio
    {
        static SoundPlayer mplayer = null;
        static bool isPlaying = false;

        /// <summary>
        /// Play one of the predefined sound files
        /// </summary>
        /// <param name="sound"></param>
        public static void Play()
        {
            System.Media.SoundPlayer myPlayer = new System.Media.SoundPlayer();
            myPlayer.Stream = Properties.Resources.SmokeAlarm;
            myPlayer.Play();
        }


        /// <summary>
        /// Play one of the predefined sound files
        /// </summary>
        /// <param name="sound"></param>
        public static void PlayLoop()
        {
            if (!isPlaying)
            {
                if(mplayer == null)
                    mplayer = new System.Media.SoundPlayer();
                mplayer.Stream = Properties.Resources.SmokeAlarm;
                mplayer.PlayLooping();
                isPlaying = true;
            }
        }


        public static void StopLoop()
        {
            if (isPlaying && mplayer != null)
            {
                mplayer.Stop();
                isPlaying = false;
            }
        }
    }
}
