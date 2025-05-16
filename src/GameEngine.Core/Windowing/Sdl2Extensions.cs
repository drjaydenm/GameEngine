using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace GameEngine.Core.Windowing
{
    public unsafe struct SDL_DisplayMode
    {
        public uint format;
        public int w;
        public int h;
        public int refresh_rate;
        public void* driverdata;
    }

    public static unsafe class Sdl2Extensions
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetDesktopDisplayMode_t(int displayIndex, SDL_DisplayMode* mode);
        private static SDL_GetDesktopDisplayMode_t s_sdl_getDesktopDisplayMode = Sdl2Native.LoadFunction<SDL_GetDesktopDisplayMode_t>("SDL_GetDesktopDisplayMode");
        public static int SDL_GetDesktopDisplayMode(int displayIndex, SDL_DisplayMode* mode) => s_sdl_getDesktopDisplayMode(displayIndex, mode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetNumVideoDisplays_t();
        private static SDL_GetNumVideoDisplays_t s_sdl_getNumVideoDisplays = Sdl2Native.LoadFunction<SDL_GetNumVideoDisplays_t>("SDL_GetNumVideoDisplays");
        public static int SDL_GetNumVideoDisplays() => s_sdl_getNumVideoDisplays();
    }
}
