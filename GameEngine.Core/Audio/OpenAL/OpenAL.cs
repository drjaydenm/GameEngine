using System;
using System.Runtime.InteropServices;
using NativeLibraryLoader;

namespace GameEngine.Core.Audio.OpenAL
{
    public static class OpenAL
    {
        private static readonly NativeLibrary openALLib = LoadOpenAL();
        private static NativeLibrary LoadOpenAL()
        {
            string name;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                name = "openal32.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                name = "libopenal.so.1";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                name = "/System/Library/Frameworks/OpenAL.framework/OpenAL";
            }
            else
            {
                throw new Exception("Unsupported platform for OpenAL");
            }

            return new NativeLibrary(name);
        }

        private static T LoadFunction<T>(string name)
        {
            return openALLib.LoadFunction<T>(name);
        }

        internal enum ALBufferi
        {
            UnpackBlockAlignmentSoft = 0x200C,
            LoopSoftPointsExt = 0x2015
        }

        internal enum AlcError
        {
            NoError = 0,
        }

        internal enum AlcGetInteger
        {
            CaptureSamples = 0x0312,
        }

        internal enum AlcGetString
        {
            CaptureDeviceSpecifier = 0x0310,
            CaptureDefaultDeviceSpecifier = 0x0311,
            Extensions = 0x1006,
        }

        internal enum ALDistanceModel
        {
            None = 0,
            InverseDistanceClamped = 0xD002
        }

        internal enum ALError
        {
            NoError = 0,
            InvalidName = 0xA001,
            InvalidEnum = 0xA002,
            InvalidValue = 0xA003,
            InvalidOperation = 0xA004,
            OutOfMemory = 0xA005
        }

        internal enum ALFormat
        {
            Mono8 = 0x1100,
            Mono16 = 0x1101,
            Stereo8 = 0x1102,
            Stereo16 = 0x1103,
            MonoIma4 = 0x1300,
            StereoIma4 = 0x1301,
            MonoMSAdpcm = 0x1302,
            StereoMSAdpcm = 0x1303,
            MonoFloat32 = 0x10010,
            StereoFloat32 = 0x10011
        }

        internal enum ALGetBufferi
        {
            Bits = 0x2002,
            Channels = 0x2003,
            Size = 0x2004
        }

        internal enum ALGetSourcei
        {
            SampleOffset = 0x1025,
            SourceState = 0x1010,
            BuffersQueued = 0x1015,
            BuffersProcessed = 0x1016
        }

        internal enum ALGetString
        {
            Extensions = 0xB004
        }

        internal enum ALListener3f
        {
            Position = 0x1004,
            Velocity = 0x1006
        }

        internal enum ALSource3f
        {
            Position = 0x1004,
            Velocity = 0x1006
        }

        internal enum ALSourceb
        {
            Looping = 0x1007
        }

        internal enum ALSourcef
        {
            Pitch = 0x1003,
            Gain = 0x100A,
            ReferenceDistance = 0x1020
        }

        internal enum ALSourcei
        {
            SourceRelative = 0x202,
            Buffer = 0x1009,
            EfxDirectFilter = 0x20005,
            EfxAuxilarySendFilter = 0x20006
        }

        internal enum ALSourceState
        {
            Initial = 0x1011,
            Playing = 0x1012,
            Paused = 0x1013,
            Stopped = 0x1014
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alenable(int cap);
        internal static d_alenable Enable = LoadFunction<d_alenable>("alEnable");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferdata(uint bid, int format, IntPtr data, int size, int freq);
        internal static d_albufferdata alBufferData = LoadFunction<d_albufferdata>("alBufferData");

        internal static void BufferData(int bid, ALFormat format, byte[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            alBufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        internal static void BufferData(int bid, ALFormat format, short[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            alBufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_aldeletebuffers(int n, int* buffers);
        internal static d_aldeletebuffers alDeleteBuffers = LoadFunction<d_aldeletebuffers>("alDeleteBuffers");

        internal static void DeleteBuffers(params int[] buffers)
        {
            DeleteBuffers(buffers.Length, ref buffers[0]);
        }

        internal unsafe static void DeleteBuffers(int n, ref int buffers)
        {
            fixed (int* pbuffers = &buffers)
            {
                alDeleteBuffers(n, pbuffers);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferi(int buffer, ALBufferi param, int value);
        internal static d_albufferi Bufferi = LoadFunction<d_albufferi>("alBufferi");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetbufferi(int bid, ALGetBufferi param, out int value);
        internal static d_algetbufferi GetBufferi = LoadFunction<d_algetbufferi>("alGetBufferi");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferiv(int bid, ALBufferi param, int[] values);
        internal static d_albufferiv Bufferiv = LoadFunction<d_albufferiv>("alBufferiv");

        internal static void GetBuffer(int bid, ALGetBufferi param, out int value)
        {
            GetBufferi(bid, param, out value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_algenbuffers(int count, int* buffers);
        internal static d_algenbuffers alGenBuffers = LoadFunction<d_algenbuffers>("alGenBuffers");

        internal unsafe static void GenBuffers(int count, out int[] buffers)
        {
            buffers = new int[count];
            fixed (int* ptr = &buffers[0])
            {
                alGenBuffers(count, ptr);
            }
        }

        internal static void GenBuffers(int count, out int buffer)
        {
            int[] ret;
            GenBuffers(count, out ret);
            buffer = ret[0];
        }

        internal static int[] GenBuffers(int count)
        {
            int[] ret;
            GenBuffers(count, out ret);
            return ret;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algensources(int n, uint[] sources);
        internal static d_algensources alGenSources = LoadFunction<d_algensources>("alGenSources");


        internal static void GenSources(int[] sources)
        {
            uint[] temp = new uint[sources.Length];
            alGenSources(temp.Length, temp);
            for (int i = 0; i < temp.Length; i++)
            {
                sources[i] = (int)temp[i];
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ALError d_algeterror();
        internal static d_algeterror GetError = LoadFunction<d_algeterror>("alGetError");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alisbuffer(uint buffer);
        internal static d_alisbuffer alIsBuffer = LoadFunction<d_alisbuffer>("alIsBuffer");

        internal static bool IsBuffer(int buffer)
        {
            return alIsBuffer((uint)buffer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcepause(uint source);
        internal static d_alsourcepause alSourcePause = LoadFunction<d_alsourcepause>("alSourcePause");

        internal static void SourcePause(int source)
        {
            alSourcePause((uint)source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourceplay(uint source);
        internal static d_alsourceplay alSourcePlay = LoadFunction<d_alsourceplay>("alSourcePlay");

        internal static void SourcePlay(int source)
        {
            alSourcePlay((uint)source);
        }

        internal static string GetErrorString(ALError errorCode)
        {
            return errorCode.ToString();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alissource(int source);
        internal static d_alissource IsSource = LoadFunction<d_alissource>("alIsSource");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_aldeletesources(int n, ref int sources);
        internal static d_aldeletesources alDeleteSources = LoadFunction<d_aldeletesources>("alDeleteSources");

        internal static void DeleteSource(int source)
        {
            alDeleteSources(1, ref source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcestop(int sourceId);
        internal static d_alsourcestop SourceStop = LoadFunction<d_alsourcestop>("alSourceStop");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcei(int sourceId, int i, int a);
        internal static d_alsourcei alSourcei = LoadFunction<d_alsourcei>("alSourcei");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsource3i(int sourceId, ALSourcei i, int a, int b, int c);
        internal static d_alsource3i alSource3i = LoadFunction<d_alsource3i>("alSource3i");

        internal static void Source(int sourceId, ALSourcei i, int a)
        {
            alSourcei(sourceId, (int)i, a);
        }

        internal static void Source(int sourceId, ALSourceb i, bool a)
        {
            alSourcei(sourceId, (int)i, a ? 1 : 0);
        }

        internal static void Source(int sourceId, ALSource3f i, float x, float y, float z)
        {
            alSource3f(sourceId, i, x, y, z);
        }

        internal static void Source(int sourceId, ALSourcef i, float dist)
        {
            alSourcef(sourceId, i, dist);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcef(int sourceId, ALSourcef i, float a);
        internal static d_alsourcef alSourcef = LoadFunction<d_alsourcef>("alSourcef");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsource3f(int sourceId, ALSource3f i, float x, float y, float z);
        internal static d_alsource3f alSource3f = LoadFunction<d_alsource3f>("alSource3f");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetsourcei(int sourceId, ALGetSourcei i, out int state);
        internal static d_algetsourcei GetSource = LoadFunction<d_algetsourcei>("alGetSourcei");

        internal static ALSourceState GetSourceState(int sourceId)
        {
            int state;
            GetSource(sourceId, ALGetSourcei.SourceState, out state);
            return (ALSourceState)state;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_allistener3f(ALListener3f param, float value1, float value2, float value3);
        internal static d_allistener3f Listener = LoadFunction<d_allistener3f>("alListener3f");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetlistener3f(ALListener3f param, out float value1, out float value2, out float value3);
        internal static d_algetlistener3f GetListener = LoadFunction<d_algetlistener3f>("alGetListener3f");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_aldistancemodel(ALDistanceModel model);
        internal static d_aldistancemodel DistanceModel = LoadFunction<d_aldistancemodel>("alDistanceModel");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_aldopplerfactor(float value);
        internal static d_aldopplerfactor DopplerFactor = LoadFunction<d_aldopplerfactor>("alDopplerFactor");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alsourcequeuebuffers(int sourceId, int numEntries, int* buffers);
        internal static d_alsourcequeuebuffers alSourceQueueBuffers = LoadFunction<d_alsourcequeuebuffers>("alSourceQueueBuffers");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alsourceunqueuebuffers(int sourceId, int numEntries, int* salvaged);
        internal static d_alsourceunqueuebuffers alSourceUnqueueBuffers = LoadFunction<d_alsourceunqueuebuffers>("alSourceUnqueueBuffers");

        internal static unsafe void SourceQueueBuffers(int sourceId, int numEntries, int[] buffers)
        {
            fixed (int* ptr = &buffers[0])
            {
                alSourceQueueBuffers(sourceId, numEntries, ptr);
            }
        }

        internal unsafe static void SourceQueueBuffer(int sourceId, int buffer)
        {
            alSourceQueueBuffers(sourceId, 1, &buffer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourceunqueuebuffers2(int sid, int numEntries, out int[] bids);
        internal static d_alsourceunqueuebuffers2 alSourceUnqueueBuffers2 = LoadFunction<d_alsourceunqueuebuffers2>("alSourceUnqueueBuffers");

        internal static unsafe int[] SourceUnqueueBuffers(int sourceId, int numEntries)
        {
            if (numEntries <= 0)
            {
                throw new ArgumentOutOfRangeException("numEntries", "Must be greater than zero.");
            }
            int[] array = new int[numEntries];
            fixed (int* ptr = &array[0])
            {
                alSourceUnqueueBuffers(sourceId, numEntries, ptr);
            }
            return array;
        }

        internal static void SourceUnqueueBuffers(int sid, int numENtries, out int[] bids)
        {
            alSourceUnqueueBuffers2(sid, numENtries, out bids);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int d_algetenumvalue(string enumName);
        internal static d_algetenumvalue alGetEnumValue = LoadFunction<d_algetenumvalue>("alGetEnumValue");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alisextensionpresent(string extensionName);
        internal static d_alisextensionpresent IsExtensionPresent = LoadFunction<d_alisextensionpresent>("alIsExtensionPresent");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_algetprocaddress(string functionName);
        internal static d_algetprocaddress alGetProcAddress = LoadFunction<d_algetprocaddress>("alGetProcAddress");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_algetstring(int p);
        private static d_algetstring alGetString = LoadFunction<d_algetstring>("alGetString");

        internal static string GetString(int p)
        {
            return Marshal.PtrToStringAnsi(alGetString(p));
        }

        internal static string Get(ALGetString p)
        {
            return GetString((int)p);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccreatecontext(IntPtr device, int[] attributes);
        internal static d_alccreatecontext CreateContext = LoadFunction<d_alccreatecontext>("alcCreateContext");

        internal static AlcError GetErrorALC()
        {
            return GetErrorForDevice(IntPtr.Zero);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate AlcError d_alcgeterror(IntPtr device);
        internal static d_alcgeterror GetErrorForDevice = LoadFunction<d_alcgeterror>("alcGetError");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcgetintegerv(IntPtr device, int param, int size, int[] values);
        internal static d_alcgetintegerv alcGetIntegerv = LoadFunction<d_alcgetintegerv>("alcGetIntegerv");

        internal static void GetInteger(IntPtr device, AlcGetInteger param, int size, int[] values)
        {
            alcGetIntegerv(device, (int)param, size, values);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcgetcurrentcontext();
        internal static d_alcgetcurrentcontext GetCurrentContext = LoadFunction<d_alcgetcurrentcontext>("alcGetCurrentContext");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcmakecontextcurrent(IntPtr context);
        internal static d_alcmakecontextcurrent MakeContextCurrent = LoadFunction<d_alcmakecontextcurrent>("alcMakeContextCurrent");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdestroycontext(IntPtr context);
        internal static d_alcdestroycontext DestroyContext = LoadFunction<d_alcdestroycontext>("alcDestroyContext");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcclosedevice(IntPtr device);
        internal static d_alcclosedevice CloseDevice = LoadFunction<d_alcclosedevice>("alcCloseDevice");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcopendevice(string device);
        internal static d_alcopendevice OpenDevice = LoadFunction<d_alcopendevice>("alcOpenDevice");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccaptureopendevice(string device, uint sampleRate, int format, int sampleSize);
        internal static d_alccaptureopendevice alcCaptureOpenDevice = LoadFunction<d_alccaptureopendevice>("alcCaptureOpenDevice");

        internal static IntPtr CaptureOpenDevice(string device, uint sampleRate, ALFormat format, int sampleSize)
        {
            return alcCaptureOpenDevice(device, sampleRate, (int)format, sampleSize);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccapturestart(IntPtr device);
        internal static d_alccapturestart CaptureStart = LoadFunction<d_alccapturestart>("alcCaptureStart");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alccapturesamples(IntPtr device, IntPtr buffer, int samples);
        internal static d_alccapturesamples CaptureSamples = LoadFunction<d_alccapturesamples>("alcCaptureSamples");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccapturestop(IntPtr device);
        internal static d_alccapturestop CaptureStop = LoadFunction<d_alccapturestop>("alcCaptureStop");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccaptureclosedevice(IntPtr device);
        internal static d_alccaptureclosedevice CaptureCloseDevice = LoadFunction<d_alccaptureclosedevice>("alcCaptureCloseDevice");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alcisextensionpresent(IntPtr device, string extensionName);
        internal static d_alcisextensionpresent IsExtensionPresentALC = LoadFunction<d_alcisextensionpresent>("alcIsExtensionPresent");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcgetstring(IntPtr device, int p);
        internal static d_alcgetstring alcGetString = LoadFunction<d_alcgetstring>("alcGetString");

        internal static string GetString(IntPtr device, int p)
        {
            return Marshal.PtrToStringAnsi(alcGetString(device, p));
        }

        internal static string GetString(IntPtr device, AlcGetString p)
        {
            return GetString(device, (int)p);
        }

#if IOS
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcsuspendcontext(IntPtr context);
        internal static d_alcsuspendcontext SuspendContext = FuncLoader.LoadFunction<d_alcsuspendcontext>(AL.NativeLibrary, "alcSuspendContext");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcprocesscontext(IntPtr context);
        internal static d_alcprocesscontext ProcessContext = FuncLoader.LoadFunction<d_alcprocesscontext>(AL.NativeLibrary, "alcProcessContext");
#endif

#if ANDROID
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdevicepausesoft(IntPtr device);
        internal static d_alcdevicepausesoft DevicePause = FuncLoader.LoadFunction<d_alcdevicepausesoft>(AL.NativeLibrary, "alcDevicePauseSOFT");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdeviceresumesoft(IntPtr device);
        internal static d_alcdeviceresumesoft DeviceResume = FuncLoader.LoadFunction<d_alcdeviceresumesoft>(AL.NativeLibrary, "alcDeviceResumeSOFT");
#endif
    }
}
