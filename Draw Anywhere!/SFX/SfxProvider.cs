using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
