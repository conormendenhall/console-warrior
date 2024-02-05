using Spectre.Console;

namespace ConsoleWarrior;

public static class Program
{
    public static void Main(string[] args)
    {
        AnsiConsole.Write(new Rule("[red]Hello, warrior[/]") { Justification = Justify.Left });

        var nameInput = AnsiConsole.Prompt(
            new TextPrompt<string>("What is your [red]name[/]?").AllowEmpty()
        );
        string warriorName = string.IsNullOrWhiteSpace(nameInput) ? "Nameless Warrior" : nameInput;
        var hero = new Hero(name: warriorName, color: "red", maxHP: 5, attackDie: 5);

        Dictionary<string, int?> merchantInventory =
            new()
            {
                { "Leather Armor", 8 },
                { "Shield", 10 },
                { "Morning Star", 12 },
                { "None", null }
            };

        AnsiConsole.MarkupLine($"Well met, [{hero.Color}]{hero.Name}[/].\n");

        do
        {
            Pause("Venture forth");
            Creature foe = GetFoe(hero.Level);
            bool heroSurvives = hero.Encounter(foe);

            if (heroSurvives)
            {
                hero.Report();

                if (hero.HP < hero.MaxHP)
                {
                    Pause("Rest");
                    hero.Rest();
                }
                Pause("Trade");
                hero.VisitMerchant(merchantInventory);
            }
            else
            {
                hero.DeathReport();
                if (AnsiConsole.Confirm("Play again?"))
                {
                    hero = new Hero(name: warriorName, color: "red", maxHP: 5, attackDie: 5);
                    merchantInventory = new()
                    {
                        { "Leather Armor", 8 },
                        { "Shield", 10 },
                        { "Morning Star", 12 },
                        { "None", null }
                    };

                    AnsiConsole.MarkupLine("\n[red]Hello again, warrior[/]\n");
                }
            }
        } while (hero.HP > 0);
    }

    public static Style SelectStyle => new Style().Foreground(Color.Red);

    public static void Pause(string prompt = "Proceed") =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>().HighlightStyle(SelectStyle).AddChoices(prompt)
        );

    public static List<Creature> Foes =>
        [
            new Creature(
                name: "Goblin",
                color: "chartreuse3",
                maxHP: 5,
                attackDie: 4,
                gold: 4,
                level: 1
            ),
            new Creature(
                name: "Cultist",
                color: "orangered1",
                maxHP: 8,
                attackDie: 6,
                gold: 6,
                level: 2
            ),
            new Creature(
                name: "Manticore",
                color: "darkgoldenrod",
                maxHP: 15,
                attackDie: 9,
                gold: 8,
                level: 3
            ),
            new Creature(
                name: "Lich",
                color: "royalblue1",
                maxHP: 18,
                attackDie: 12,
                gold: 15,
                level: 4
            ),
            new Creature(
                name: "Leviathan",
                color: "red",
                maxHP: 50,
                attackDie: 35,
                gold: 100,
                level: int.MaxValue
            ),
        ];

    public static Creature GetFoe(int heroLevel) =>
        Foes.FirstOrDefault(x => x.Level >= heroLevel, Foes[^1]);

    public static void Loot(this Hero hero, Creature corpse)
    {
        var rule = new Rule("Loot") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        hero.Gold += corpse.Gold;
        AnsiConsole.MarkupLine(
            $"You loot the [{corpse.Color}]{corpse.Name}[/] for [orange1]{corpse.Gold} gold[/] pieces. "
                + "You drop them into your coinpurse."
        );
        AnsiConsole.MarkupLine(
            $"[grey][[hero is carrying[/] [orange1]{hero.Gold} gold[/] [grey]pieces]][/]\n"
        );
    }

    public static int Attack(this Creature hero, Creature foe)
    {
        Random rdm = new();

        bool miss = foe.IsShielded && rdm.Next(1, 5) == 1;

        if (miss)
        {
            Console.WriteLine("The shield deflected the attack.");
            return 0;
        }

        int atkDmg = rdm.Next(1, hero.AttackDie);

        if (foe.IsArmored)
        {
            int dmgReduction = rdm.Next(1, 4);
            AnsiConsole.MarkupLine($"[grey][[armor reduced damage by {dmgReduction}]][/]");
            atkDmg = Math.Max(atkDmg - dmgReduction, 0);
        }
        foe.HP -= atkDmg;

        return atkDmg;
    }

    public static bool Encounter(this Hero hero, Creature foe)
    {
        var rule = new Rule($"[{foe.Color}]{foe.Name} Battle[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"You encounter a [{foe.Color}]{foe.Name}[/].\n");

        do
        {
            Pause("Attack");
            Console.WriteLine("You attack!");

            int atkDmg = hero.Attack(foe);
            AnsiConsole.MarkupLine($"[{foe.Color}]{foe.Name}[/] takes {atkDmg} damage.");

            foe.PrintHealthBar();

            if (foe.HP <= 0)
            {
                AnsiConsole.MarkupLine(
                    $"The [{foe.Color}]{foe.Name}[/] falls dead at your feet.\n"
                );
                AnsiConsole.MarkupLine($"[{hero.Color}]{hero.Name}[/] stands victorious!");

                hero.FoesFelled.Add(foe);
                hero.Experience += foe.MaxHP + foe.AttackDie;
                AnsiConsole.MarkupLine(
                    $"[grey][[hero gains {foe.MaxHP + foe.AttackDie} XP "
                        + $"for a total {hero.Experience} XP, next level at {hero.LevelXP} XP]][/]\n"
                );

                if (hero.Experience >= hero.LevelXP)
                    hero.LevelUp();

                Pause("Loot");
                hero.Loot(foe);

                return true;
            }
            else
            {
                AnsiConsole.MarkupLine(
                    $"The [{foe.Color}]{foe.Name}[/] still stands, sneering at you.\n"
                );
                AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] attacks!");
                int foeAtkDmg = foe.Attack(hero);
                Console.WriteLine($"You take {foeAtkDmg} damage.");

                hero.PrintHealthBar();

                if (hero.HP <= 0)
                {
                    AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] strikes you down.\n");

                    return false;
                }
                else if (foeAtkDmg > 0)
                {
                    Console.WriteLine("You are hurt, but not dead yet.");
                }
                Console.WriteLine("You steel your nerves for another attack.\n");
            }
        } while (foe.HP > 0 && hero.HP > 0);

        return true;
    }

    public static void LevelUp(this Hero hero)
    {
        var rule = new Rule("[red]You Level Up![/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        hero.Level += 1;
        AnsiConsole.MarkupLine($"Level {hero.Level}");

        hero.Experience -= hero.LevelXP;
        hero.LevelXP += hero.LevelXP / 5;
        AnsiConsole.MarkupLine($"[grey][[next level at {hero.LevelXP} XP]][/]\n");

        hero.MaxHP += 3;
        hero.HP = hero.MaxHP;
        AnsiConsole.MarkupLine($"Your maximum HP is increased to [green]{hero.MaxHP} HP[/].\n");
    }

    public static void Rest(this Hero hero)
    {
        var rule = new Rule("[dodgerblue1]Rest[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        Random rdm = new();
        int restHP = rdm.Next(1, 5);

        if (hero.HP + restHP <= hero.MaxHP)
        {
            AnsiConsole.MarkupLine($"You rest and restore [green]{restHP} HP[/].");
            hero.HP += restHP;
        }
        else
        {
            AnsiConsole.MarkupLine(
                $"You gain [green]{hero.MaxHP - hero.HP} HP[/] and feel fully restored."
            );
            hero.HP = hero.MaxHP;
        }
        hero.PrintHealthBar();
        Console.WriteLine();
    }

    public static void VisitMerchant(this Hero hero, Dictionary<string, int?> merchantInventory)
    {
        var rule = new Rule("[royalblue1]Merchant[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine("You encounter a [royalblue1]Merchant[/].");
        AnsiConsole.MarkupLine("[royalblue1]\"Hello, weary traveler. See anything you like?\"[/]");
        AnsiConsole.MarkupLine(
            $"[grey][[hero is carrying[/] [orange1]{hero.Gold} gold[/] [grey]pieces]][/]\n"
        );

        var purchase = AnsiConsole.Prompt(
            new SelectionPrompt<KeyValuePair<string, int?>>()
                .HighlightStyle(SelectStyle)
                .AddChoices(merchantInventory)
                .UseConverter(pair => $"{pair.Key} {pair.Value}")
        );

        if (purchase.Key == "None" || hero.Gold < purchase.Value)
        {
            AnsiConsole.MarkupLine(
                "[royalblue1]\"Come back when you're ready to spend some coin.\"[/]\n"
            );
        }
        else
        {
            if (purchase.Value.HasValue)
            {
                hero.Gold -= (int)purchase.Value;
                merchantInventory.Remove(purchase.Key);
            }

            if (purchase.Key == "Leather Armor")
            {
                hero.IsArmored = true;
                Console.WriteLine("You don the leather armor.");
                AnsiConsole.MarkupLine(
                    "[royalblue1]\"You think this will protect you? Good luck.\"[/]"
                );
            }
            else if (purchase.Key == "Shield")
            {
                hero.IsShielded = true;
                Console.WriteLine("You lift the shield.");
                AnsiConsole.MarkupLine(
                    "[royalblue1]\"Ah, the trusty shield. May it guard you well\"[/]"
                );
            }
            else if (purchase.Key == "Morning Star")
            {
                hero.AttackDie = 8;
                hero.CarriesMorningStar = true;
                Console.WriteLine("You heft the morning star.");
                AnsiConsole.MarkupLine(
                    "[royalblue1]\"So, you lust for blood. Heh heh... Strike true, warrior.\"[/]"
                );
            }
            AnsiConsole.MarkupLine(
                $"[grey][[you are left with[/] [orange1]{hero.Gold} gold[/] [grey]pieces]][/]\n"
            );
        }
    }

    public static void PrintHealthBar(this Creature creature)
    {
        AnsiConsole.Write(
            new BreakdownChart()
                .Width(creature.MaxHP)
                .AddItem("Health", creature.HP, Color.Green)
                .AddItem("Not", creature.MaxHP - creature.HP, Color.Red)
                .HideTags()
        );
    }

    public static void Report(this Hero hero)
    {
        var rule = new Rule("Report") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        hero.PrintHealthBar();
        AnsiConsole.MarkupLine($"[red]{hero.Name}[/]");
        Console.WriteLine($"Level {hero.Level}");
        AnsiConsole.WriteLine($"{hero.FoesFelled.Count} foes vanquished");

        AnsiConsole.MarkupLine($"[orange1]{hero.Gold} gold[/]");

        var inventoryString = "Short Sword";

        if (hero.CarriesMorningStar)
            inventoryString = "Morning Star";
        if (hero.IsArmored)
            inventoryString += ", Leather Armor";
        if (hero.IsShielded)
            inventoryString += ", Shield";

        AnsiConsole.WriteLine($"Inventory: {inventoryString}\n");
    }

    public static void ReportFelledFoes(this Hero hero)
    {
        var felledFoes = new Table();
        felledFoes.HideHeaders();
        felledFoes.Border(TableBorder.Simple);

        felledFoes.AddColumn("Foe");
        felledFoes.AddColumn("Count");

        if (hero.FoesFelled.Exists(x => x.Name == "Goblin"))
        {
            felledFoes.AddRow(
                new Text("Goblins:"),
                new Text(hero.FoesFelled.Count(x => x.Name == "Goblin").ToString())
            );
        }

        if (hero.FoesFelled.Exists(x => x.Name == "Cultist"))
        {
            felledFoes.AddRow(
                new Text("Cultists:"),
                new Text(hero.FoesFelled.Count(x => x.Name == "Cultist").ToString())
            );
        }

        if (hero.FoesFelled.Exists(x => x.Name == "Manticore"))
        {
            felledFoes.AddRow(
                new Text("Manticores:"),
                new Text(hero.FoesFelled.Count(x => x.Name == "Manticore").ToString())
            );
        }

        if (hero.FoesFelled.Exists(x => x.Name == "Lich"))
        {
            felledFoes.AddRow(
                new Text("Lichs:"),
                new Text(hero.FoesFelled.Count(x => x.Name == "Lich").ToString())
            );
        }
        AnsiConsole.Write(felledFoes);
    }

    public static void DeathReport(this Hero hero)
    {
        var rule = new Rule("Death") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"[orange1]{hero.Gold} gold[/] pieces spill out of your coinpurse.");
        Console.WriteLine(
            $"You reached level {hero.Level} and felled {hero.FoesFelled.Count} foes before meeting your end."
        );

        hero.ReportFelledFoes();

        AnsiConsole.MarkupLine($"Rest in peace, [{hero.Color}]{hero.Name}[/].\n");
    }
}

public class Creature(
    string name,
    string color,
    int maxHP,
    int attackDie,
    int gold = 0,
    int level = 1
)
{
    public string Name { get; set; } = name;
    public string Color { get; set; } = color;
    public int HP { get; set; } = maxHP;
    public int MaxHP { get; set; } = maxHP;
    public int AttackDie { get; set; } = attackDie;
    public int Gold { get; set; } = gold;
    public bool IsArmored { get; set; } = false;
    public bool IsShielded { get; set; } = false;
    public int Level { get; set; } = level;
}

public class Hero(string name, string color, int maxHP, int attackDie)
    : Creature(name, color, maxHP, attackDie)
{
    public int Experience { get; set; } = 0;
    public int LevelXP { get; set; } = 20;
    public List<Creature> FoesFelled { get; set; } = [];
    public bool CarriesMorningStar { get; set; } = false;
}
