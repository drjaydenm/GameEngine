using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Enumeration;

namespace GameEngine.Core.Audio.OpenAL
{
    public unsafe class OpenALAudioSystem : IAudioSystem
    {
        private readonly ALContext _alc;
        private readonly AL _al;
        private readonly Device* _device;
        private readonly Context* _context;

        public OpenALAudioSystem()
        {
            _alc = ALContext.GetApi();
            _al = AL.GetApi();

            if (_alc.TryGetExtension<Enumeration>(null, out var enumeration))
            {
                var devices = enumeration.GetStringList(GetEnumerationContextStringList.DeviceSpecifiers);
            }

            _device = _alc.OpenDevice("");
            if (_device == null)
            {
                throw new InvalidOperationException("Could not open OpenAL device");
            }

            _context = _alc.CreateContext(_device, null);
            _alc.MakeContextCurrent(_context);

            _al.GetError();
        }

        public INativeAudioBuffer CreateNativeAudioBuffer()
        {
            return new OpenALAudioBuffer(_al);
        }

        public INativeAudioListener CreateNativeAudioListener()
        {
            return new OpenALAudioListener(_al);
        }

        public INativeAudioSource CreateNativeAudioSource()
        {
            return new OpenALAudioSource(_al);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);
            _al.Dispose();
            _alc.Dispose();
        }
    }
}
