using System;
using System.IO;

namespace R2MagicDecoder
{
    /// <summary>
    /// Decodes and encodes Rayman 2 data files.
    /// </summary>
    public static class MagicDecoder
    {
        /// <summary>
        /// Decodes or encodes a single byte and calculates a new magic value.
        /// </summary>
        /// <param name="b">The byte to decode or encode.</param>
        /// <param name="magic">The magic value.</param>
        public static byte Unmagic(byte b, ref uint magic)
        {
            b ^= (byte) ((magic >> 8) & 0xFF);
            magic = 16807 * (magic ^ 123459876) - 0x7FFFFFFF * ((magic ^ 123459876) / 0x1F31D);
            return b;
        }

        /// <summary>
        /// Decodes or encodes a byte array.
        /// </summary>
        /// <param name="inBytes">The byte array to decode.</param>
        /// <param name="getMagicFromHeader">
        /// If <code>true</code>, uses the first 4 bytes as the initial magic value.<para/>
        /// If <code>false</code>, uses the default initial magic value.
        /// </param>
        public static byte[] DecodeBytes(byte[] inBytes, bool getMagicFromHeader = false)
        {
            byte[] outBytes = new byte[inBytes.LongLength];
            uint magic = getMagicFromHeader ? BitConverter.ToUInt32(inBytes, 0) : 1790299257;

            for (int i = 0; i < 4; i++)
                outBytes[i] = inBytes[i];

            for (long i = 4; i < inBytes.LongLength; i++)
                outBytes[i] = Unmagic(inBytes[i], ref magic);

            return outBytes;
        }

        /// <summary>
        /// Decodes or encodes a file.
        /// </summary>
        /// <param name="inFile">The path to the file that will be decoded.</param>
        /// <param name="outFile">The path to the output file.</param>
        /// <param name="getMagicFromHeader">
        /// If <code>true</code>, uses the first 4 bytes as the initial magic value.<para/>
        /// If <code>false</code>, uses the default initial magic value.
        /// </param>
        public static void DecodeFile(string inFile, string outFile, bool getMagicFromHeader = false)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(inFile)))
            {
                byte[] header = reader.ReadBytes(4);
                uint magic = getMagicFromHeader ? BitConverter.ToUInt32(header, 0) : 1790299257;
                long length = reader.BaseStream.Length;

                using (BinaryWriter writer = new BinaryWriter(File.Open(outFile, FileMode.Create)))
                {
                    writer.Write(header);

                    while (reader.BaseStream.Position < length)
                    {
                        byte read = reader.ReadByte();
                        byte decoded = Unmagic(read, ref magic);

                        writer.Write(decoded);
                    }
                }
            }
        }
    }
}
