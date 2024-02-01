﻿using Spectre.Console;

namespace ConsoleWarrior
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, Warrior!");

            var warriorName = AnsiConsole.Ask<string>("What is your [red]name[/]?");
            var hero = new Hero(
                name: warriorName,
                color: "red",
                maxHP: 5,
                attackDie: 5,
                gold: 0,
                isShielded: false
            );

            AnsiConsole.Markup($"Well met, [{hero.Color}]{hero.Name}[/].\n\n");

            do
            {
                bool heroSurvives = hero.Encounter();

                if (heroSurvives)
                {
                    hero.Rest();
                    hero.VisitMerchant();
                }
                else
                {
                    AnsiConsole.Markup(
                        $"[orange1]{hero.Gold} gold[/] pieces spill out of your coinpurse.\n"
                    );
                    Console.WriteLine(
                        $"You felled {hero.FelledFoes} foes before meeting your end."
                    );
                    AnsiConsole.Markup($"Rest in peace, [{hero.Color}]{hero.Name}[/].\n");
                }
            } while (hero.HP > 0);
        }
    }

    public class Creature(
        string name,
        string color,
        int maxHP,
        int attackDie,
        int gold,
        bool isShielded = false
    )
    {
        public string Name { get; set; } = name;
        public string Color { get; set; } = color;
        public int HP { get; set; } = maxHP;
        public int MaxHP { get; set; } = maxHP;
        public int AttackDie { get; set; } = attackDie;
        public int Gold { get; set; } = gold;
        public bool IsShielded { get; set; } = isShielded;

        public int Attack(Creature foe)
        {
            Random rdm = new();
            var atkDmg = rdm.Next(1, AttackDie);

            if (foe.IsShielded)
            {
                var dmgReduction = rdm.Next(1, 5);
                AnsiConsole.WriteLine($"Shield reduced damage by {dmgReduction}.");
                atkDmg = Math.Max(atkDmg - dmgReduction, 0);
            }
            foe.HP -= atkDmg;

            return atkDmg;
        }
    }

    public class Hero(
        string name,
        string color,
        int maxHP,
        int attackDie,
        int gold,
        bool isShielded
    ) : Creature(name, color, maxHP, attackDie, gold, isShielded)
    {
        public int FelledFoes { get; set; }
        public List<string> Inventory { get; set; } = ["Shield - 10gp", "None"];

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
                Console.WriteLine($"[hero has {HP} HP]\n");
            }
        }

        public int Loot(Creature corpse)
        {
            Gold += corpse.Gold;

            return corpse.Gold;
        }

        public bool Encounter()
        {
            var foe = new Creature(
                name: "Goblin",
                color: "chartreuse3",
                maxHP: 5,
                attackDie: 4,
                gold: 4
            );

            switch (FelledFoes)
            {
                case <= 3:
                    break;
                case <= 6:
                    foe.Name = "Cultist";
                    foe.Color = "orangered1";
                    foe.MaxHP = 8;
                    foe.AttackDie = 7;
                    foe.Gold = 6;
                    break;
                case > 6:
                    foe.Name = "Manticore";
                    foe.Color = "blueviolet";
                    foe.MaxHP = 15;
                    foe.AttackDie = 10;
                    foe.Gold = 8;
                    break;
                default:

            }
            AnsiConsole.Markup($"You encounter a [{foe.Color}]{foe.Name}[/].\n");

            do
            {
                Console.WriteLine($"You attack!");
                var atkDmg = Attack(foe);
                Console.WriteLine($"You deal {atkDmg} damage.");
                Console.WriteLine($"[{foe.Name} has {foe.HP} HP]");

                if (foe.HP <= 0)
                {
                    AnsiConsole.Markup(
                        $"The [{foe.Color}]{foe.Name}[/] falls dead at your feet.\n"
                    );
                    Console.WriteLine($"{Name} stands victorious!\n");
                    FelledFoes += 1;

                    var loot = Loot(foe);
                    AnsiConsole.Markup(
                        $"You loot the [{foe.Color}]{foe.Name}[/] for [orange1]{loot} gold[/] pieces. "
                            + "You drop them into your coinpurse.\n"
                    );
                    AnsiConsole.Markup($"You are carrying [orange1]{Gold} gold[/].\n");

                    return true;
                }
                else
                {
                    AnsiConsole.Markup(
                        $"The [{foe.Color}]{foe.Name}[/] still stands, sneering at you.\n\n"
                    );
                    AnsiConsole.Markup($"The [{foe.Color}]{foe.Name}[/] attacks!\n");
                    var foeAtkDmg = foe.Attack(this);
                    AnsiConsole.Markup(
                        $"The [{foe.Color}]{foe.Name}[/] deals {foeAtkDmg} damage.\n"
                    );
                    Console.WriteLine($"[hero has {HP} HP]");

                    if (HP <= 0)
                    {
                        AnsiConsole.Markup($"The [{foe.Color}]{foe.Name}[/] strikes you down.\n\n");

                        return false;
                    }
                    else
                    {
                        if (foeAtkDmg > 0)
                            Console.WriteLine("You are hurt, but not dead yet.");
                        Console.WriteLine("You steel your nerves for another attack.\n");
                    }
                }
            } while (foe.HP > 0 && HP > 0);

            return true;
        }

        public void VisitMerchant()
        {
            AnsiConsole.Markup("You encounter a [blueviolet]Merchant[/].\n");
            AnsiConsole.Markup($"You have [orange1]{Gold} gold[/] pieces.\n");

            var purchase = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Hello, weary traveler. See anything you like?")
                    .AddChoices(Inventory)
            );

            if (purchase == "Shield - 10gp" && Gold >= 10)
            {
                AnsiConsole.WriteLine("Ah, the trusty shield. May it guard you well.");
                Gold -= 10;
                Inventory.Remove(purchase);
                AnsiConsole.Markup($"You are left with [orange1]{Gold} gold[/] pieces.\n\n");
                IsShielded = true;
            }
            else
            {
                AnsiConsole.WriteLine("Come back when you've earned some coin.\n");
            }
        }
    }
}
