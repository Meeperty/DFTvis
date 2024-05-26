using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DFTvis.WindowsSound
{
	public struct WavFile
	{
		public enum BitsPerSample
		{
			EightBits,
			SixteenBits
		}

		UInt16 audioFormat;
		UInt16 numChannels; 
		UInt32 sampleRate; //samples per second
		UInt32 byteRate; //for buffer estimation
		UInt16 blockAlign; //block alignment of waveform data, in bytes
		UInt16 bitsPerSample;
		byte[] chunkID = new byte[4];
		UInt32 datachunkSize;

		byte[] byteData = [];
		Int16[] int16Data = [];
		BitsPerSample bps = BitsPerSample.SixteenBits;


		public readonly int Channels => numChannels;

		/// <summary>
		/// Sample rate, in samples per sec
		/// </summary>
		public readonly int SampleRate => (int)sampleRate;

		/// <summary>
		/// The number of audio samples in the file
		/// </summary>
		public int SampleCount
		{
			get
			{
				switch (bps)
				{
					case BitsPerSample.EightBits:
						return (int)datachunkSize / numChannels;
					default:
						return (int)datachunkSize /(sizeof(Int16) * numChannels);
				}
			}
		}

		/// <summary>
		/// The duration of the file
		/// </summary>
		public TimeSpan Duration
		{
			get => new TimeSpan(0, 0, (int)(SampleCount / (double)SampleRate));
		}



		public readonly T[] GetData<T>(int channel = 1)
		{
			switch (bps)
			{
				case BitsPerSample.EightBits:
					T[] data = new T[byteData.Length / numChannels];
					for (int i = 0; i < byteData.Length; i++)
					{
						if (i % numChannels == 0) 
						{ 
							data[i / numChannels] = (T)Convert.ChangeType(byteData[i], typeof(T)); 
						}
					}
					return data;
				default: //BitsPerSample.SixteenBits
					T[] data2 = new T[int16Data.Length / numChannels];
					for (int i = 0; i < int16Data.Length; i++)
					{
						if (i % numChannels == 0) 
						{ 
							data2[i / numChannels] = (T)Convert.ChangeType(int16Data[i], typeof(T)); 
						}
					}
					return data2;
			}
		}

		public WavFile(string filePath)
		{
			using (BinaryReader br = new(File.Open(filePath, FileMode.Open)))
			{
				byte[] riffHeader = br.ReadBytes(4);
				if (!riffHeader.SequenceEqual(new byte[] {(byte)'R', (byte)'I', (byte)'F', (byte)'F' })) throw new ArgumentException("Attempted to parse a non-RIFF file as WAV");

				br.ReadUInt32(); //ignoring: File size remaining, minus 8 bytes

				byte[] format = br.ReadBytes(4);
				if (!format.SequenceEqual(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' })) throw new ArgumentException("Attempted to parse a non-WAV RIFF file as WAV");

				byte[] fmtchunkID = br.ReadBytes(4);
				if (!fmtchunkID.SequenceEqual(new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' })) throw new ArgumentException("WAV file contains a non-standard first chunk (fmt chunk expected)");

				UInt32 fmtChunkSize = br.ReadUInt32();
				if (fmtChunkSize != 0x10) throw new ArgumentException("WAV file fmt chunk is an unexpected length");

				audioFormat = br.ReadUInt16();
				if(audioFormat != 1) throw new NotImplementedException("Parsing WAV formats other than integer PCM is not implemented");

				numChannels = br.ReadUInt16();
				sampleRate = br.ReadUInt32();
				byteRate = br.ReadUInt32();
				blockAlign = br.ReadUInt16();
				bitsPerSample = br.ReadUInt16();
				chunkID = br.ReadBytes(4);
				if (!chunkID.SequenceEqual(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' })) throw new ArgumentException($"WAV file contains a non-standard second chunk (data chunk expected)");

				datachunkSize = br.ReadUInt32();

				switch (bitsPerSample)
				{
					case 8:
						byteData = br.ReadBytes((int)datachunkSize);
						int16Data = [];
						bps = BitsPerSample.EightBits;
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
						byteData = [];
						bps = BitsPerSample.SixteenBits;
						break;

					default:
						throw new ArgumentException($"unsupported number of bits per sample: {bitsPerSample}");
						//break; apparently i don't need a break after throw
				}
			}
		}

		//public override readonly string ToString()
		//{
		//	StringBuilder sb = new();

		//	sb.AppendLine("Riff header");
		//	sb.AppendLine(" chunkSize     " + chunkSize);
		//	sb.AppendLine(" format        " + Encoding.ASCII.GetString(format));
		//	sb.AppendLine(" fmtchunkID    " + Encoding.ASCII.GetString(fmtchunkID));
		//	sb.AppendLine(" fmtchunkSize  " + fmtchunkSize);
		//	sb.AppendLine(" audioFormat   " + audioFormat);
		//	sb.AppendLine(" numChannels   " + numChannels);
		//	sb.AppendLine(" sampleRate    " + sampleRate);
		//	sb.AppendLine(" byteRate      " + byteRate);
		//	sb.AppendLine(" blockAlign    " + blockAlign);
		//	sb.AppendLine(" bitsPerSample " + bitsPerSample);

		//	return sb.ToString();
		//}

	}
}
