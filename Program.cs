using Spectre.Console;

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

public class Hero : ICreature
{
    public string Name { get; set; } = "Nameless Warrior";
    public int MaxHP { get; set; } = 5;
    public int HP { get; set; } = 5;
    public int AttackDie { get; set; } = 4;
    public int Gold { get; set; }
    public int FelledFoes { get; set; }

    public Hero(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name;
        }
    }

    public void Attack(ICreature creature)
    {
        AnsiConsole.Markup($"You attack!\n");

        Random rdm = new();
        var atkDmg = rdm.Next(1, AttackDie);
        creature.HP -= atkDmg;

        AnsiConsole.Markup($"You deal {atkDmg} damage.\n");
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

    public int Loot(ILootable lootable)
    {
        Gold += lootable.LootGP;

        return lootable.LootGP;
    }

    public void Encounter(Goblin goblin)
    {
        while (goblin.HP > 0 && HP > 0)
        {
            Attack(goblin);
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
                goblin.Attack(this);
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

public interface ICreature
{
    public int HP { get; set; }
    public int AttackDie { get; set; }
    public void Attack(ICreature creature);
}

public interface ILootable
{
    public int LootGP { get; }
}

public class Goblin : ICreature, ILootable
{
    public int HP { get; set; } = 5;
    public int AttackDie { get; set; } = 4;
    public int LootGP
    {
        get
        {
            Random rdm = new();
            return rdm.Next(5);
        }
    }

    public void Attack(ICreature creature)
    {
        AnsiConsole.Markup($"The [chartreuse3]goblin[/] attacks!\n");

        Random rdm = new();

        var atkDmg = rdm.Next(1, AttackDie);
        creature.HP -= atkDmg;

        AnsiConsole.Markup($"The [chartreuse3]goblin[/] deals {atkDmg} damage.\n");
    }
}