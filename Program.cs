﻿using Spectre.Console;

namespace ConsoleWarrior;

static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, Warrior!");

        var warriorName = AnsiConsole.Ask<string>("What is your [red]name[/]?");
        var hero = new Hero(warriorName);

        AnsiConsole.Markup($"Well met, [red]{hero.Name}[/].\n\n");

        while (hero.HP > 0)
        {
            hero.Rest();

            AnsiConsole.Markup($"You encounter a [chartreuse3]goblin[/].\n");
            var goblin = new Goblin();
            hero.Encounter(goblin);
        }
    }
}

public abstract class Creature
{
    public abstract int HP { get; set; }
    public abstract int AttackDie { get; set; }
    public abstract int LootMax { get; }

    public int Attack(Creature foe)
    {
        Random rdm = new();
        var atkDmg = rdm.Next(1, AttackDie);
        foe.HP -= atkDmg;

        return atkDmg;
    }

    public int LootGP
    {
        get
        {
            Random rdm = new();
            return rdm.Next(LootMax);
        }
    }
}

public class Hero : Creature
{
    public override int HP { get; set; } = 5;
    public override int AttackDie { get; set; } = 4;
    public override int LootMax { get => Gold; }
    public string Name { get; set; } = "Nameless Warrior";
    public int MaxHP { get; set; } = 5;
    public int Gold { get; set; }
    public int FelledFoes { get; set; }

    public Hero(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name;
        }
    }

    public void Rest()
    {
        if (HP < MaxHP)
        {
            Random rdm = new();
            var restHP = rdm.Next(1, 3);

            if (HP + restHP <= MaxHP)
            {
                HP += restHP;
                Console.WriteLine($"You rest and restore {restHP} HP.");
            }
            else
            {
                HP = 5;
                Console.WriteLine($"You rest and feel fully restored.");
            }
            Console.WriteLine($"You have {HP} HP.\n");
        }
    }

    public int Loot(Creature creature)
    {
        Gold += creature.LootGP;

        return creature.LootGP;
    }

    public void Encounter(Goblin goblin)
    {
        while (goblin.HP > 0 && HP > 0)
        {
            AnsiConsole.Markup($"You attack!\n");
            var atkDmg = Attack(goblin);
            AnsiConsole.Markup($"You deal {atkDmg} damage.\n");
            Console.WriteLine($"[goblin has {goblin.HP} HP]");

            if (goblin.HP <= 0)
            {
                AnsiConsole.Markup($"The [chartreuse3]goblin[/] falls dead at your feet.\n");
                Console.WriteLine($"{Name} stands victorious!\n");
                FelledFoes += 1;

                var loot = Loot(goblin);
                AnsiConsole.Markup($"You loot the [chartreuse3]goblin[/] for {loot} gold pieces. "
                    + "You drop them into your coinpurse.\n");
                Console.WriteLine($"You are carrying {Gold} gold.\n");
            }
            else
            {
                AnsiConsole.Markup("The [chartreuse3]goblin[/] still stands, sneering at you.\n\n");
                AnsiConsole.Markup("The [chartreuse3]goblin[/] attacks!\n");
                var foeAtkDmg = goblin.Attack(this);
                AnsiConsole.Markup($"The [chartreuse3]goblin[/] deals {foeAtkDmg} damage.\n");
                Console.WriteLine($"[hero has {HP} HP]");

                if (HP <= 0)
                {
                    AnsiConsole.Markup("The [chartreuse3]goblin[/] strikes you down.\n\n");
                    Console.WriteLine($"{Gold} gold pieces spill out of your coinpurse.");
                    Console.WriteLine($"You felled {FelledFoes} foes before meeting your end.");
                    Console.WriteLine($"Rest in peace, {Name}.");
                }
                else
                {
                    Console.WriteLine($"You are hurt but not dead yet. "
                        + "You steel your nerves for another attack.\n");
                }
            }
        }
    }
}

public class Goblin : Creature
{
    public override int HP { get; set; } = 5;
    public override int AttackDie { get; set; } = 4;
    public override int LootMax { get; } = 5;
}