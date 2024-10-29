public static class Example
{
    public class Spell
    {
        public int Damage { get; init; }
        public int Cost { get; init; }
    }

    public static readonly Spell Fireball = new()
    {
        Damage = 15,
        Cost = 15,
    };

    public static readonly Spell Frostbolt = new()
    {
        Damage = 12,
        Cost = 20,
    };

    public static readonly IReadOnlyCollection<Spell> All = new[]
    {
        Fireball,
        Frostbolt,
    };
}