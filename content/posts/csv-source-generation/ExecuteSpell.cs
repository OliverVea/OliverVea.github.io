public Result ExecuteSpell(Spell spell, Target origin, Target target)
{
    if (spell.Cost > origin.Mana)
    {
        return Result.Error("Not enough mana to cast spell");
    }

    origin.Mana -= spell.Cost;
    target.Health -= spell.Damage;

    return Result.Success;
}