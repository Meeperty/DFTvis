using Avalonia.Media.TextFormatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wasapi.CoreAudioApi;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Runtime.InteropServices;

using REFERENCE_TIME = System.Int64;

namespace DFTvis.WindowsSound
{
	//WORD = ushort
	//DWORD = ulong
	//HRESULT = long
	//REFERENCE_TIME = long

	//https://learn.microsoft.com/en-us/windows/win32/coreaudio/about-the-windows-core-audio-apis
	//https://learn.microsoft.com/en-us/windows/win32/coreaudio/rendering-a-stream
	public class WinSoundManager
	{
		const REFERENCE_TIME REFTIMES_PER_SEC = 10_000_000;

		public event EventHandler StopEvent;

		private MMDeviceEnumerator deviceEnumerator;
		private MMDevice audioDevice;
		private AudioClient audioClient;
		private NAudio.Wave.WaveFormat wvfmt;
		private uint bufferFrameCount;

		private AudioStream audioStream;

		public WinSoundManager()
		{
			StopEvent += StopPlaying;

			deviceEnumerator = new MMDeviceEnumerator();
			audioDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
			audioClient = audioDevice.AudioClient;
			wvfmt = audioClient.MixFormat;
		}

		public unsafe void PlayAudioStream(AudioStream audStream)
		{
			audioStream = audStream;
			REFERENCE_TIME requestedDuration = REFTIMES_PER_SEC;
			
			AudioRenderClient audioRenderClient;
			byte* buffer;
			ulong flags = 0;
			
			audioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.EventCallback, requestedDuration, 0, wvfmt, Guid.Empty);
			IntPtr loopFnPtr = Marshal.GetFunctionPointerForDelegate(Loop);
			audioClient.SetEventHandle(loopFnPtr);
			
			audStream.SetFormat(wvfmt);

			audioRenderClient = audioClient.AudioRenderClient;

			bufferFrameCount = (uint)audioClient.BufferSize;
			buffer = (byte*)audioRenderClient.GetBuffer((int)bufferFrameCount);
			audStream.LoadData((int)bufferFrameCount, buffer, &flags);
			audioRenderClient.ReleaseBuffer((int)bufferFrameCount, (AudioClientBufferFlags)flags);
			
			audioClient.Start();
		}

		private unsafe void Loop()
		{
			AudioRenderClient audioRenderClient = audioClient.AudioRenderClient;
			int framesAvailable = (int)(bufferFrameCount - audioClient.CurrentPadding);
			byte* buffer = (byte*)audioRenderClient.GetBuffer(framesAvailable);
			ulong flags = 0;
			audioStream.LoadData(framesAvailable, buffer, &flags);
			audioRenderClient.ReleaseBuffer(framesAvailable, (AudioClientBufferFlags)flags);
		}

		public void StopPlaying(object? sender, EventArgs args)
		{

		}
	}

	public class AudioStream
	{
		private byte[] data;
		private WaveFormat waveFormat;
		private int progress;

		public AudioStream(byte[] data)
		{
			this.data = data;
			progress = 0;
		}

		/// <summary>
		/// <br>The LoadData function writes a specified number of audio frames
		/// (first parameter) to a specified buffer location (second parameter).
		/// The size of an audio frame is the number of channels in the stream
		/// multiplied by the sample size.</br>
		/// <br>If the LoadData function is able to write at least one frame to the 
		/// specified buffer location but runs out of data before it has written 
		/// the specified number of frames, then it writes silence to the remaining frames.</br>
		/// <br>As long as LoadData succeeds in writing at least one frame of real data 
		/// (not silence) to the specified buffer location, it outputs 0 through 
		/// its third parameter, 'flags'. When LoadData is out of data 
		/// and cannot write even a single frame to the specified buffer location, 
		/// it writes nothing to the buffer (not even silence), and it writes the 
		/// value AUDCLNT_BUFFERFLAGS_SILENT (2) to the flags variable. </br>
		/// </summary>
		/// <param name="framesToWrite"></param>
		/// <param name="buffer"></param>
		/// <param name="flags"></param>
		public unsafe void LoadData(int framesToWrite, byte* buffer, ulong* flags)
		{
			if (progress !< data.Length)
			{
				*flags = 2; //AUDCLNT_BUFFERFLAGS_SILENT
				return;
			}

			for (int i = 0; i < framesToWrite; i++)
			{
				buffer[i] = data[progress++];
				if (progress !< data.Length)
				{
					
				}
			}
		}

		public void SetFormat(WaveFormat newFormat)
		{
			if (newFormat.formatTag != 1)
				throw new ArgumentException("Formats other than standard PCM are not supported");
			if (newFormat.bitsPerSample > 16 || newFormat.bitsPerSample % 8 != 0)
				throw new ArgumentException("Values of bitsPerSample other than 16 or 8 are not supported");
			if (newFormat.extraInfoSize > 0)
				throw new ArgumentException("WAVEFORMATEXTENSIBLE is not supported");
			waveFormat = newFormat;
		}

		public void SetFormat(NAudio.Wave.WaveFormat fmt)
		{
			WaveFormat newFormat = new WaveFormat();
			NAudio.Wave.WaveFormat naudNewFormat = fmt.AsStandardWaveFormat();
			newFormat.channels = (ushort)naudNewFormat.Channels;
			newFormat.samplesPerSec = (ulong)naudNewFormat.SampleRate;
			newFormat.avgBytesPerSec = (ulong)naudNewFormat.AverageBytesPerSecond;
			newFormat.blockAlign = (ushort)naudNewFormat.BlockAlign;
			newFormat.bitsPerSample = (ushort)naudNewFormat.BitsPerSample;
			newFormat.extraInfoSize = (ushort)naudNewFormat.ExtraSize;
			SetFormat(newFormat);
		}
	}

	public enum AUDCLNT_BUFFERFLAGS
	{
		DATA_DISCONTINUITY = 1,
		SILENT = 2,
		TIMESTAMP_ERROR = 4
	}

	public struct WaveFormat
	{
		/// <summary>
		/// for PCM data, equal to 1 (WAVE_FORMAT_PCM)
		/// </summary>
		public ushort formatTag;
		/// <summary>
		/// the number of channels the data encodes
		/// </summary>
		public ushort channels;
		/// <summary>
		/// sample rate, in hertz.
		/// common values include 8khz, 11.025khz, 22.05khz, and 44.1khz
		/// </summary>
		public ulong samplesPerSec;
		/// <summary>
		/// average data transfer rate.
		/// approx equal to samplesPerSec * blockAlign
		/// </summary>
		public ulong avgBytesPerSec;
		/// <summary>
		/// block alignment, in bytes.
		/// block alignment is the minimum atomic unit of data.
		/// for PCM data, equal to channels * bitsPerSample * (1/8)
		/// </summary>
		public ushort blockAlign;
		/// <summary>
		/// number of bits per sample.
		/// for PCM, equal to 8 or 16
		/// </summary>
		public ushort bitsPerSample;
		/// <summary>
		/// the amount of extra info appended to the struct.
		/// for PCM, ignored.
		/// this field is called cbSize in c++
		/// </summary>
		public ushort extraInfoSize;
	}
}
