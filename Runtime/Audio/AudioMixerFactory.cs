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
        [System.Serializable]
        public class BusMapper
        {
            public AudioBus Bus;
            public AudioMixerGroup MixerGroup;
        }

        [SerializeField] List<BusMapper> _mappers = new();

        [SerializeField] AudioConfig _sfxUIButtonClick;

        [NonSerialized] Dictionary<AudioBus, AudioMixerGroup> _lookup;

        public static AudioConfig SfxUIButtonClick => Instance._sfxUIButtonClick;

        public static AudioMixerGroup GetGroup(AudioBus bus)
        {
            if (bus == AudioBus.Master || bus == AudioBus.None)
                return null;

            if (Instance._lookup.TryGetValue(bus, out var group))
                return group;

            CodeSketchDebug.LogWarning($"Mixer group not found for bus: {bus}");
            return null;
        }

        public static void OverrideMixer(AudioMixer mixer)
        {
            if (mixer == null) return;

            var inst = Instance;
            inst._lookup ??= new Dictionary<AudioBus, AudioMixerGroup>();

            foreach (AudioBus bus in Enum.GetValues(typeof(AudioBus)))
            {
                if (bus == AudioBus.None) continue;

                var groups = mixer.FindMatchingGroups(bus.ToString());
                if (groups != null && groups.Length > 0)
                {
                    inst._lookup[bus] = groups[0];
                }
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _lookup = new Dictionary<AudioBus, AudioMixerGroup>();
            foreach (var entry in _mappers)
            {
                _lookup.TryAdd(entry.Bus, entry.MixerGroup);
            }
        }
    }
}
