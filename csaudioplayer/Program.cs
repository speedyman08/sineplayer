namespace csaudioplayer;

static class Player
{
    public static void Main()
    {
        AudioRenderer renderer = new(8, 44100, 5);
        
        Console.Write("Select sine frequency: ");
        var freq = Int32.Parse(Console.ReadLine());
        
        SineGenerator gen = new(freq);
        
        renderer.PlaySignal(gen.MakeBuffer(5));
    }
}