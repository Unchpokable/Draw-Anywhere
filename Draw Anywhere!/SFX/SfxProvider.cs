using System;
using System.IO;
using System.Media;

namespace DrawAnywhere.SFX
{
    internal class SfxProvider : IDisposable
    {
        private SoundPlayer _player = new();
        private Stream _sfxCameraStream = Properties.Resources.cam;

        public void PlayCamera()
        {
            _player.Stream = _sfxCameraStream;
            _player.Play();
        }

        public void Dispose()
        {
            _player?.Dispose();
            _sfxCameraStream?.Dispose();
        }
    }
}
