namespace pscommander
{
    public class Blink
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int PoolSize { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Blink hk)
            {
                return hk.Id == Id;
            }
            return false;
        }
    }
}