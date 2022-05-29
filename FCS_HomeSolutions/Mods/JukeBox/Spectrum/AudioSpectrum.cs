using UnityEngine;

namespace FCS_HomeSolutions.Mods.JukeBox.Spectrum
{
    /// <summary>
    /// Mini "engine" for analyzing spectrum data
    /// Feel free to get fancy in here for more accurate visualizations!
    /// </summary>
    public class AudioSpectrum : MonoBehaviour
    {
        internal void Initialize(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }

        private void Update()
        {
            // get the data
            _audioSource.GetSpectrumData(m_audioSpectrum, 0, FFTWindow.Hamming);

            // assign spectrum value
            // this "engine" focuses on the simplicity of other classes only..
            // ..needing to retrieve one value (spectrumValue)
            if (m_audioSpectrum != null && m_audioSpectrum.Length > 0)
            {
                spectrumValue = m_audioSpectrum[0] * 100;
            }
        }

        private void Start()
        {
            /// initialize buffer
            m_audioSpectrum = new float[128];
        }

        // This value served to AudioSyncer for beat extraction
        public static float spectrumValue { get; private set; }

        // Unity fills this up for us
        private float[] m_audioSpectrum;
        private AudioSource _audioSource;
    }
}
