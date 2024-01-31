using Spectre.Console;

namespace ConsoleWarrior
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, Warrior!");

            var warriorName = AnsiConsole.Ask<string>("What is your [red]name[/]?");
            var hero = new Hero(name: warriorName, color: "red", maxHP: 5, attackDie: 5, gold: 0);

            AnsiConsole.Markup($"Well met, [{hero.Color}]{hero.Name}[/].\n\n");

            while (hero.HP > 0)
            {
                hero.Rest();

                var creature = new Creature(name: "Goblin", color: "chartreuse3", maxHP: 5, attackDie: 4, gold: 4);
                AnsiConsole.Markup($"You encounter a [{creature.Color}]{creature.Name}[/].\n");
                hero.Encounter(creature);
            }
        }
    }

    public class Creature
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int AttackDie { get; set; }
        public int Gold { get; set; }

        public Creature(string name, string color, int maxHP, int attackDie, int gold)
        {
            Name = name;
            Color = color;
            HP = maxHP;
            MaxHP = maxHP;
            AttackDie = attackDie;
            Gold = gold;
        }

        public int Attack(Creature foe)
        {
            Random rdm = new();
            var atkDmg = rdm.Next(1, AttackDie);
            foe.HP -= atkDmg;

            return atkDmg;
        }
    }

    public class Hero : Creature
    {
        public int FelledFoes { get; set; }

        public Hero(string name, string color, int maxHP, int attackDie, int gold)
            : base(name, color, maxHP, attackDie, gold) { }

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
                    HP = MaxHP;
                    Console.WriteLine($"You rest and feel fully restored.");
                }
                Console.WriteLine($"You have {HP} HP.\n");
            }
        }

        public int Loot(Creature corpse)
        {
            Gold += corpse.Gold;

            return corpse.Gold;
        }

        public void Encounter(Creature creature)
        {
            while (creature.HP > 0 && HP > 0)
            {
                AnsiConsole.Markup($"You attack!\n");
                var atkDmg = Attack(creature);
                AnsiConsole.Markup($"You deal {atkDmg} damage.\n");
                Console.WriteLine($"[{creature.Name} has {creature.HP} HP]");

                if (creature.HP <= 0)
                {
                    AnsiConsole.Markup($"The [{creature.Color}]{creature.Name}[/] falls dead at your feet.\n");
                    Console.WriteLine($"{Name} stands victorious!\n");
                    FelledFoes += 1;

                    var loot = Loot(creature);
                    AnsiConsole.Markup($"You loot the [{creature.Color}]{creature.Name}[/] for {loot} gold pieces. "
                        + "You drop them into your coinpurse.\n");
                    Console.WriteLine($"You are carrying {Gold} gold.\n");
                }
                else
                {
                    AnsiConsole.Markup($"The [{creature.Color}]{creature.Name}[/] still stands, sneering at you.\n\n");
                    AnsiConsole.Markup($"The [{creature.Color}]{creature.Name}[/] attacks!\n");
                    var foeAtkDmg = creature.Attack(this);
                    AnsiConsole.Markup($"The [{creature.Color}]{creature.Name}[/] deals {foeAtkDmg} damage.\n");
                    Console.WriteLine($"[hero has {HP} HP]");

                    if (HP <= 0)
                    {
                        AnsiConsole.Markup($"The [{creature.Color}]{creature.Name}[/] strikes you down.\n\n");
                        Console.WriteLine($"{Gold} gold pieces spill out of your coinpurse.");
                        Console.WriteLine($"You felled {FelledFoes} foes before meeting your end.");
                        AnsiConsole.Markup($"Rest in peace, [{Color}]{Name}[/].\n");
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
}