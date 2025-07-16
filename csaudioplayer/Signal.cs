namespace csaudioplayer;

public class SineGenerator
{
    private int signalFrequency;
    private int bitDepth;
    private int sampleFrequency;

    public SineGenerator(int signalFrequency, int bitDepth = 8, int sampleFrequency = 44100)
    {
        // we only support bit depths upto 64

        if (bitDepth > 64)
        {
            throw new ArgumentOutOfRangeException("bitDepth", "bitDepth cannot be larger than 64");
        }

        this.signalFrequency = signalFrequency;
        this.bitDepth = bitDepth;
        this.sampleFrequency = sampleFrequency;
    }

    private Int64 GeneratePoint(double secondsX)
    {
        Int64 amplitude = (int)Math.Pow(2, bitDepth) / 2;

        var point = amplitude * Math.Sin(2 * Math.PI * signalFrequency * secondsX);
        
        // 8-bit PCM is UNSIGNED (range 0-255)
        if (bitDepth < 9) point += amplitude;

        // anything else can be signed
        return (Int64) Math.Floor(point);
    }

    public Int64[] MakeBuffer(int secondsOfSignal)
    {
        double timeDelta = 1.0 / sampleFrequency;
        Int64[] buf = new Int64[secondsOfSignal * sampleFrequency];

        var curSample = 0;
        while (curSample < secondsOfSignal * sampleFrequency)
        {
            buf[curSample] = GeneratePoint(timeDelta * curSample);
            curSample++;
        }

        return buf;
    }
}