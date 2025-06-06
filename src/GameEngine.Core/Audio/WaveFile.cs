﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace GameEngine.Core.Audio
{
    public class WaveFile
    {
        public byte[] Data { get; }
        public AudioBufferFormat Format { get; }
        public int SizeInBytes { get; }
        public int Frequency { get; }

        public unsafe WaveFile(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                var riffChunkDescriptor = br.ReadStruct<RIFFChunkDescriptor>();
                var id = Encoding.ASCII.GetString(riffChunkDescriptor.ChunkID, 4);
                if (id != "RIFF")
                {
                    throw new InvalidOperationException("Not a valid wave file ID: " + id);
                }
                var format = Encoding.ASCII.GetString(riffChunkDescriptor.Format, 4);
                if (format != "WAVE")
                {
                    throw new InvalidOperationException("Not a valid wave file format : " + format);
                }

                var fmtSubChunk = br.ReadStruct<FmtSubChunk>();
                var fmtChunkID = Encoding.ASCII.GetString(fmtSubChunk.Subchunk1ID, 4);
                if (fmtChunkID != "fmt ")
                {
                    throw new InvalidOperationException("Not a supported fmt sub-chunk ID: " + fmtChunkID);
                }

                Format = MapFormat(fmtSubChunk.NumChannels, fmtSubChunk.BitsPerSample);
                Frequency = fmtSubChunk.SampleRate;

                // SubChunk2ID
                br.ReadInt32();
                var subchunk2Size = br.ReadInt32();
                Data = br.ReadBytes(subchunk2Size);
                SizeInBytes = subchunk2Size;
            }
        }

        private AudioBufferFormat MapFormat(short numChannels, short bitsPerSample)
        {
            if (numChannels == 1 || numChannels == 2)
            {
                if (bitsPerSample == 8)
                {
                    return numChannels == 1 ? AudioBufferFormat.Mono8 : AudioBufferFormat.Stereo8;
                }
                else if (bitsPerSample == 16)
                {
                    return numChannels == 1 ? AudioBufferFormat.Mono16 : AudioBufferFormat.Stereo16;
                }
                else
                {
                    throw new InvalidOperationException("Unsupported bit depth in wave file: " + bitsPerSample);
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported number of channels in wave file: " + numChannels);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct RIFFChunkDescriptor
        {
            public fixed byte ChunkID[4];
            public int ChunkSize;
            public fixed byte Format[4];
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct FmtSubChunk
        {
            public fixed byte Subchunk1ID[4];
            public int Subchunk1Size;
            public short AudioFormat;
            public short NumChannels;
            public int SampleRate;
            public int ByteRate;
            public short BlockAlign;
            public short BitsPerSample;
        }
    }

    public static class BinaryReaderExtensions
    {
        public static unsafe T ReadStruct<T>(this BinaryReader br) where T : struct
        {
            return ReadStruct<T>(br, Unsafe.SizeOf<T>());
        }

        public static unsafe T ReadStruct<T>(this BinaryReader br, int structSizeInBytes) where T : struct
        {
            byte* data = stackalloc byte[structSizeInBytes];
            for (int i = 0; i < structSizeInBytes; i++)
            {
                data[i] = br.ReadByte();
            }

            return Unsafe.Read<T>(data);
        }
    }
}
