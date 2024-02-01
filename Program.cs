using Spectre.Console;

namespace ConsoleWarrior
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var font = FigletFont.Load("epic.flf");
            AnsiConsole.Write(
                new FigletText(font, "Hello, Warrior!").LeftJustified().Color(Color.Red)
            );

            var nameInput = AnsiConsole.Prompt(
                new TextPrompt<string>("What is your [red]name[/]?").AllowEmpty()
            );
            string warriorName = string.IsNullOrWhiteSpace(nameInput)
                ? "Nameless Warrior"
                : nameInput;
            var hero = new Hero(name: warriorName, color: "red", maxHP: 5, attackDie: 5);

            AnsiConsole.MarkupLine($"Well met, [{hero.Color}]{hero.Name}[/].\n");

            do
            {
                Creature foe = GetFoe(hero.FelledFoes);
                bool heroSurvives = hero.Encounter(foe);

                Pause();

                if (heroSurvives)
                {
                    if (hero.HP < hero.MaxHP)
                        hero.Rest();

                    Pause();

                    hero.VisitMerchant();

                    Pause();
                }
                else
                {
                    var rule = new Rule("Death");
                    rule.Justification = Justify.Left;
                    AnsiConsole.Write(rule);
                    AnsiConsole.MarkupLine(
                        $"[orange1]{hero.Gold} gold[/] pieces spill out of your coinpurse."
                    );
                    Console.WriteLine(
                        $"You felled {hero.FelledFoes} foes before meeting your end."
                    );
                    AnsiConsole.MarkupLine($"Rest in peace, [{hero.Color}]{hero.Name}[/].");
                }
            } while (hero.HP > 0);
        }

        public static void Pause()
        {
            AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices("Proceed"));
        }

        public static Creature GetFoe(int felledFoes)
        {
            var foe = new Creature(
                name: "Goblin",
                color: "chartreuse3",
                maxHP: 5,
                attackDie: 4,
                gold: 4
            );

            switch (felledFoes)
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
                case <= 8:
                    foe.Name = "Manticore";
                    foe.Color = "darkgoldenrod";
                    foe.MaxHP = 15;
                    foe.AttackDie = 10;
                    foe.Gold = 8;
                    break;
                case > 8:
                    foe.Name = "Lich";
                    foe.Color = "royalblue1";
                    foe.MaxHP = 18;
                    foe.AttackDie = 15;
                    foe.Gold = 8;
                    break;
                default:

            }
            return foe;
        }
    }

    public class Creature(
        string name,
        string color,
        int maxHP,
        int attackDie,
        int gold = 0,
        bool isArmored = false,
        bool isShielded = false
    )
    {
        public string Name { get; set; } = name;
        public string Color { get; set; } = color;
        public int HP { get; set; } = maxHP;
        public int MaxHP { get; set; } = maxHP;
        public int AttackDie { get; set; } = attackDie;
        public int Gold { get; set; } = gold;
        public bool IsArmored { get; set; } = isArmored;
        public bool IsShielded { get; set; } = isShielded;

        public int Attack(Creature foe)
        {
            Random rdm = new();

            bool miss = foe.IsShielded && rdm.Next(1, 4) == 1;

            if (miss)
            {
                Console.WriteLine("The shield deflected the attack.");
                return 0;
            }

            int atkDmg = rdm.Next(1, AttackDie);

            if (foe.IsArmored)
            {
                int dmgReduction = rdm.Next(1, 5);
                AnsiConsole.WriteLine($"Armor reduced damage by {dmgReduction}.");
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
        int gold = 0,
        bool isArmored = false,
        bool isShielded = false
    ) : Creature(name, color, maxHP, attackDie, gold, isArmored, isShielded)
    {
        public int FelledFoes { get; set; }
        public List<string> MerchantInventory { get; set; } =
            ["Leather Armor - 10gp", "Shield - 12gp", "Morning Star - 14gp", "None"];

        public void Rest()
        {
            var rule = new Rule("[dodgerblue1]Rest[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);

            Random rdm = new();
            int restHP = rdm.Next(1, 3);

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

        public int Loot(Creature corpse)
        {
            Gold += corpse.Gold;

            return corpse.Gold;
        }

        public bool Encounter(Creature foe)
        {
            var rule = new Rule($"[{foe.Color}]{foe.Name} Battle[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine($"You encounter a [{foe.Color}]{foe.Name}[/].");

            do
            {
                Console.WriteLine($"You attack!");
                int atkDmg = Attack(foe);
                Console.WriteLine($"You deal {atkDmg} damage.");
                Console.WriteLine($"[{foe.Name} has {foe.HP} HP]");

                if (foe.HP <= 0)
                {
                    AnsiConsole.MarkupLine(
                        $"The [{foe.Color}]{foe.Name}[/] falls dead at your feet."
                    );
                    Console.WriteLine($"{Name} stands victorious!\n");
                    FelledFoes += 1;

                    int loot = Loot(foe);
                    AnsiConsole.MarkupLine(
                        $"You loot the [{foe.Color}]{foe.Name}[/] for [orange1]{loot} gold[/] pieces. "
                            + "You drop them into your coinpurse."
                    );
                    AnsiConsole.MarkupLine($"You are carrying [orange1]{Gold} gold[/].\n");

                    return true;
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        $"The [{foe.Color}]{foe.Name}[/] still stands, sneering at you.\n"
                    );
                    AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] attacks!");
                    int foeAtkDmg = foe.Attack(this);
                    AnsiConsole.MarkupLine(
                        $"The [{foe.Color}]{foe.Name}[/] deals {foeAtkDmg} damage."
                    );
                    Console.WriteLine($"[hero has {HP} HP]");

                    if (HP <= 0)
                    {
                        AnsiConsole.MarkupLine(
                            $"The [{foe.Color}]{foe.Name}[/] strikes you down.\n"
                        );

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
            var rule = new Rule("[blueviolet]Merchant[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine("You encounter a [blueviolet]Merchant[/].");
            AnsiConsole.MarkupLine($"You have [orange1]{Gold} gold[/] pieces.");

            string purchase = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Hello, weary traveler. See anything you like?")
                    .AddChoices(MerchantInventory)
            );

            if (purchase == "Leather Armor - 10gp" && Gold >= 10)
            {
                AnsiConsole.WriteLine("You think this will protect you? Good luck.");
                Gold -= 10;
                MerchantInventory.Remove(purchase);
                AnsiConsole.MarkupLine($"You are left with [orange1]{Gold} gold[/] pieces.\n");
                IsArmored = true;
            }
            else if (purchase == "Shield - 12gp" && Gold >= 10)
            {
                AnsiConsole.WriteLine("Ah, the trusty shield. May it guard you well.");
                Gold -= 12;
                MerchantInventory.Remove(purchase);
                AnsiConsole.MarkupLine($"You are left with [orange1]{Gold} gold[/] pieces.\n");
                IsShielded = true;
            }
            else if (purchase == "Morning Star - 14gp" && Gold >= 14)
            {
                AnsiConsole.WriteLine("So, you lust for blood. Heh heh... Strike true, warrior.");
                Gold -= 14;
                MerchantInventory.Remove(purchase);
                AnsiConsole.MarkupLine($"You are left with [orange1]{Gold} gold[/] pieces.\n");
                AttackDie = 8;
            }
            else
            {
                AnsiConsole.WriteLine("Come back when you're ready to spend some coin.\n");
            }
        }
    }
}
