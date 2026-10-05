using System;
using System.Collections.Generic;
using SketchEngine.Diagnostics;
using UnityEngine;
using UnityEngine.Audio;

using SketchEngine.SO;

namespace SketchEngine.Audio
{
    public class AudioMixerFactory : ScriptableObjectSingleton<AudioMixerFactory>
    {
        [Serializable]
        public class BusMapper
        {
            public AudioBus Bus;
            public AudioMixerGroup MixerGroup;
        }

        // AudioBus có giá trị thưa (10, 20, ...) nên index mảng theo giá trị enum lớn nhất.
        const int BusSlotCount = 64;

        [SerializeField] List<BusMapper> _mappers = new List<BusMapper>();
        [SerializeField] AudioConfig _sfxUIButtonClick;

        // Mảng cố định, tra cứu O(1), không cấp phát khi phát âm thanh.
        [NonSerialized] AudioMixerGroup[] _groups;

        public static AudioConfig SfxUIButtonClick => Instance._sfxUIButtonClick;

        public static AudioMixerGroup GetGroup(AudioBus bus)
        {
            if (bus == AudioBus.Master || bus == AudioBus.None)
                return null;

            var factory = Instance;
            if (factory == null)
                return null;

            factory.EnsureGroups();

            int slot = (int)bus;
            if (slot < 0 || slot >= BusSlotCount)
                return null;

            AudioMixerGroup group = factory._groups[slot];
            if (group == null)
                SketchDebug.LogWarning($"Mixer group not found for bus: {bus}");

            return group;
        }

        public static void OverrideMixer(AudioMixer mixer)
        {
            var factory = Instance;
            if (mixer == null || factory == null)
                return;

            factory.EnsureGroups();

            foreach (AudioBus bus in Enum.GetValues(typeof(AudioBus)))
            {
                if (bus == AudioBus.None || bus == AudioBus.Master) continue;

                AudioMixerGroup[] groups = mixer.FindMatchingGroups(bus.ToString());
                if (groups != null && groups.Length > 0)
                    factory._groups[(int)bus] = groups[0];
            }
        }

        // Khởi tạo lazy: dù chưa qua OnInitialize vẫn không NRE.
        void EnsureGroups()
        {
            if (_groups != null)
                return;

            _groups = new AudioMixerGroup[BusSlotCount];
            for (int i = 0; i < _mappers.Count; i++)
            {
                BusMapper entry = _mappers[i];
                if (entry == null) continue;

                int slot = (int)entry.Bus;
                if (slot >= 0 && slot < BusSlotCount && _groups[slot] == null)
                    _groups[slot] = entry.MixerGroup;
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _groups = null;
            EnsureGroups();
        }
    }
}
