public OneOf<Success, ErrorMessage> ExecuteSpell(Spell spell, Target origin, Target target)
{
    if (spell.Cost > origin.Mana)
    {
        return new ErrorMessage("Not enough mana to cast spell");
    }

    origin.Mana -= spell.Cost;
    target.Health -= spell.Damage;

    return success;
}