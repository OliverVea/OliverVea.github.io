public static class Spells
{
    public class Spell
    {
        public int Damage { get; }
        public float Range { get; }
        public int Cost { get; }

        public Spell(int damage, float range, int cost)
        {
            Damage = damage;
            Range = range;
            Cost = cost;
        }
    }

    public static readonly Spell Fireball = new(15, 5, 15);
    public static readonly Spell Frostbolt = new(12, 5, 20);
    public static readonly Spell HeavyStrike = new(20, 0.7f, 5);
}