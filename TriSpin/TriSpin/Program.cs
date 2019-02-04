namespace TriSpin
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Win win = new Win())
            {
                win.Run(60);
            }
        }
    }
}
