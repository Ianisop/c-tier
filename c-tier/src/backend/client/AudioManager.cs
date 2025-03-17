using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pv;

namespace c_tier.src.backend.client
{

    public static class AudioManager
    {
        public static PvRecorder recorder;
        public static PvSpeaker speaker;
        public static int sampleRate = 22050;
        public static int bitsPerSample = 16;
        public static int frameLength = 512; // 512 default
        public static short[] inputAudioFrames;
        public static Dictionary<int, string> inputDevices = new Dictionary<int, string>();
        public static Dictionary<int, string> outputDevices = new Dictionary<int, string>();


        /// <summary>
        /// Initializes the audio manager devices and creates a recording devices
        /// </summary>
        public static void Init()
        {
            FindDevices();
            recorder = PvRecorder.Create(frameLength, 0);
            speaker = new PvSpeaker(
                sampleRate:sampleRate,
                bitsPerSample:bitsPerSample
                );
          
        }

        /// <summary>
        /// This should be used by Task.Run to record continously
        /// </summary>
        public static void Record()
        {
            recorder.Start();
            while (recorder.IsRecording)
            {
                inputAudioFrames = recorder.Read();
            }
        }

        public static void Play(byte[] samples)
        {
            speaker.Start();
            while(speaker.IsStarted)
            {
                int writtenSamples = speaker.Write(samples); // write to the audio buffer
                int flushedLength = speaker.Flush(); // this is what plays the audio
            }
            speaker.Stop();
        }

        /// <summary>
        /// Finds all the audio recording devices availible and caches them in the inputDevices dictionary
        /// </summary>
        public static void FindDevices()
        {
            string[] devices = PvRecorder.GetAvailableDevices();
            
            for(int i = 0; i < devices.Length; i++)
            {
                inputDevices.Add(i, devices[i]);
            }

            string[] odevices = PvSpeaker.GetAvailableDevices();
            for (int i = 0; i < odevices.Length; i++)
            {
                outputDevices.Add(i, odevices[i]);
            }
        }



    }
}
