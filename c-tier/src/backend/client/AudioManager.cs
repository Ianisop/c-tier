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
        public static int frameLength = 512; // 512 default
        public static short[] inputAudioFrames;
        public static Dictionary<int, string> inputDevices = new Dictionary<int, string>();
        public static Dictionary<int, string> outputDevices = new Dictionary<int, string>();



        public static void Init()
        {
            FindDevices();
            Console.WriteLine("Finding devices...");
            recorder = PvRecorder.Create(frameLength, 0);
            Console.WriteLine("Recorder open with device");

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
        }



    }
}
