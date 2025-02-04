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
        private static Queue<short[]> audioQueue = new Queue<short[]>();

        /// <summary>
        /// Initializes the audio manager devices and creates a recording devices
        /// </summary>
        public static void Init()
        {
            FindDevices();
            ClientFrontend.app.inputDeviceLabel.Text = "Input Device: " + inputDevices[1];
            ClientFrontend.app.outputDeviceLabel.Text = "Output Device: " + outputDevices[1];
            recorder = PvRecorder.Create(frameLength, 1);
            speaker = new PvSpeaker(
                sampleRate:sampleRate,
                bitsPerSample:bitsPerSample,
                deviceIndex:1
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


        public static void Play(short[] samples)
        {
            lock (audioQueue)
            {
                audioQueue.Enqueue(samples);
            }

            if (!speaker.IsStarted)
            {
                speaker.Start();
                Task.Run(() => ProcessAudioQueue());
            }
        }
        private static void ProcessAudioQueue()
        {
            while (speaker.IsStarted)
            {
                short[] nextSamples = null;

                lock (audioQueue)
                {
                    if (audioQueue.Count > 0)
                        nextSamples = audioQueue.Dequeue();
                }

                if (nextSamples != null)
                {
                    speaker.Write(Utils.ShortArrayToByteArray(nextSamples));
                }
                else
                {
                    Task.Delay(1).Wait(); // Prevents CPU overuse if queue is empty
                }
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

            string[] odevices = PvSpeaker.GetAvailableDevices();
            for (int i = 0; i < odevices.Length; i++)
            {
                outputDevices.Add(i, odevices[i]);
            }
        }



    }
}
