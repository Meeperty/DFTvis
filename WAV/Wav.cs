using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DFTvis.WindowsSound
{
	public struct WavFile
	{
		public enum BitsPerSample
		{
			EigthBits,
			SixteenBits
		}

		byte[] RIFF = new byte[4];
		UInt32 chunkSize;
		byte[] format = new byte[4];
		byte[] fmtchunkID = new byte[4];
		UInt32 fmtchunkSize; 
		UInt16 audioFormat; //should always be 0x0001, PCM
		UInt16 numChannels; 
		UInt32 sampleRate; //samples per second
		UInt32 byteRate; //for buffer estimation
		UInt16 blockAlign; //block alignment of waveform data, in bytes
		UInt16 bitsPerSample;
		byte[] chunkID = new byte[4];
		UInt32 datachunkSize;

		byte[] byteData = new byte[0];
		Int16[] int16Data = new Int16[0];
		BitsPerSample bps = BitsPerSample.SixteenBits;

		public T[] GetData<T>(int channel = 1)
		{
			switch (bps)
			{
				case BitsPerSample.EigthBits:
					T[] data = new T[byteData.Length];
					for (int i = 0; i < byteData.Length; i++)
					{
						if (i % numChannels == 0) 
						{ 
							data[i / numChannels] = (T)Convert.ChangeType(byteData[i], typeof(T)); 
						}
					}
					return data;
				default: //BitsPerSample.SixteenBits
					T[] data2 = new T[int16Data.Length];
					for (int i = 0; i < byteData.Length; i++)
					{
						if (i % numChannels == 0) 
						{ 
							data2[i / numChannels] = (T)Convert.ChangeType(byteData[i], typeof(T)); 
						}
					}
					return data2;
			}
		}

		public WavFile(string filePath)
		{
			using (BinaryReader br = new(File.Open(filePath, FileMode.Open)))
			{
				RIFF = br.ReadBytes(4);
				chunkSize = br.ReadUInt32();
				format = br.ReadBytes(4);
				fmtchunkID = br.ReadBytes(4);
				fmtchunkSize = br.ReadUInt32();
				audioFormat = br.ReadUInt16();
				numChannels = br.ReadUInt16();
				sampleRate = br.ReadUInt32();
				byteRate = br.ReadUInt32();
				blockAlign = br.ReadUInt16();
				bitsPerSample = br.ReadUInt16();
				chunkID = br.ReadBytes(4);
				datachunkSize = br.ReadUInt32();

				//Debug.Assert(RIFF == new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }, "Not a RIFF file");
				//Debug.Assert(format == new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' }, "Not a WAVE file");
				//Debug.Assert(audioFormat == 1, "Not a PCM file");

				switch (bitsPerSample)
				{
					case 8:
						byteData = br.ReadBytes((int)datachunkSize);
						int16Data = new Int16[0];
						bps = BitsPerSample.EigthBits;
						break;

					case 16:
						int numint16 = (int)(datachunkSize / sizeof(Int16));
						int16Data = new Int16[numint16];
						byte[] int16Buf;
						for (int i = 0; i < numint16; i++)
						{
							int16Buf = br.ReadBytes(2);
							int16Data[i] = BitConverter.ToInt16(int16Buf, 0);
						}
						byteData = new byte[0];
						bps = BitsPerSample.SixteenBits;
						break;

					default:
						Debug.Assert(false, $"unsupported number of bits per sample: {bitsPerSample}");
						break;
				}
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new();

			sb.AppendLine("Riff header");
			sb.AppendLine(" chunkSize     " + chunkSize);
			sb.AppendLine(" format        " + Encoding.ASCII.GetString(format));
			sb.AppendLine(" fmtchunkID    " + Encoding.ASCII.GetString(fmtchunkID));
			sb.AppendLine(" fmtchunkSize  " + fmtchunkSize);
			sb.AppendLine(" audioFormat   " + audioFormat);
			sb.AppendLine(" numChannels   " + numChannels);
			sb.AppendLine(" sampleRate    " + sampleRate);
			sb.AppendLine(" byteRate      " + byteRate);
			sb.AppendLine(" blockAlign    " + blockAlign);
			sb.AppendLine(" bitsPerSample " + bitsPerSample);

			return sb.ToString();
		}

	}
}
