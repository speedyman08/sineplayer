using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace csaudioplayer;

// Basic PCM player
public class AudioRenderer
{
    private readonly int _audioSeconds;
    private readonly int _sampleRate;
    private readonly int _bitDepth;
    private readonly int _bytesPerSample;

    private readonly AudioClient _audioClient;
    private readonly AudioRenderClient _renderer;

    public AudioRenderer() : this(8, 44100, 1)
    {
    }

    public AudioRenderer(int bitDepth, int sampleRate, int audioSeconds)
    {
        _bitDepth = bitDepth;
        _sampleRate = sampleRate;
        _audioSeconds = audioSeconds;

        if (_bitDepth % 8 != 0)
        {
            throw new ArgumentException("bitDepth shouldn't be weird, must be multiple of 8");
        }

        _bytesPerSample = _bitDepth / 8;

        var audioDevice = SelectDefaultDevice();

        // grab this once only, getter makes new audioclient every time
        _audioClient = audioDevice.AudioClient;

        var audioFormat = WaveFormat.CreateCustomFormat(
            WaveFormatEncoding.Pcm,
            _sampleRate,
            1,
            _sampleRate * (_bytesPerSample),
            _bytesPerSample,
            _bitDepth
        );

        _audioClient.Initialize(
            AudioClientShareMode.Shared,
            // the actual audio device may not work at 44.1kHz
            AudioClientStreamFlags.AutoConvertPcm,
            // 10 million (hundred) nanoseconds is 1 second
            _audioSeconds * 10000000,
            0,
            audioFormat,
            Guid.Empty
        );

        _renderer = _audioClient.AudioRenderClient;
    }

    private MMDevice SelectDefaultDevice()
    {
        var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        Console.WriteLine($"Using device \"{device.FriendlyName}\"");

        return device;
    }

    
    private byte[] ConvertSignalLong(long[] longBuffer)
    {
        if (_bitDepth == 8)
        {
            // NOT sbyte, 8 bit must be unsigned as per wav spec
            return Array.ConvertAll(longBuffer, x => (byte)x);
        }

        var bufBytes = new byte[longBuffer.Length * _bytesPerSample];
        var idx = 0;
        foreach (var val in longBuffer)
        {
            var sample = BitConverter.GetBytes(val);
            Array.Copy(sample, 0, bufBytes, idx, _bytesPerSample);
            idx += _bytesPerSample;
        }

        return bufBytes;
    }

    public void PlaySignal(long[] longBuffer)
    {
        var byteBuf = ConvertSignalLong(longBuffer);

        var frameCount = _audioSeconds * _sampleRate;
        var bufPtr = _renderer.GetBuffer(frameCount);

        Marshal.Copy(byteBuf, 0, bufPtr, frameCount * _bytesPerSample);
        _renderer.ReleaseBuffer(frameCount, AudioClientBufferFlags.None);
        _audioClient.Start();

        Thread.Sleep(_audioSeconds * 1000);
    }
}