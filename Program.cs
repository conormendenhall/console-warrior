using Spectre.Console;

namespace ConsoleWarrior;

public static class Program
{
    public static void Main(string[] args)
    {
        AnsiConsole.Write(new Rule("[red]Hello, warrior[/]") { Justification = Justify.Left });

        string nameInput = AnsiConsole.Prompt(
            new TextPrompt<string>("What is your [red]name[/]?").AllowEmpty()
        );
        string heroName = string.IsNullOrWhiteSpace(nameInput) ? "Nameless Warrior" : nameInput;
        var hero = GetFreshHero(heroName);

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
                Pause("Loot");
                hero.Loot(foe);

                Pause("View character sheet");
                hero.PrintCharacterSheet();

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
                hero.PrintDeathReport();
                if (AnsiConsole.Confirm("Play again?"))
                {
                    hero = GetFreshHero(heroName);
                    merchantInventory = new()
                    {
                        { "Leather Armor", 8 },
                        { "Shield", 10 },
                        { "Morning Star", 12 },
                        { "None", null }
                    };

                    Console.WriteLine();
                    AnsiConsole.MarkupLine("[red]Hello again, warrior[/]\n");
                }
            }
        } while (hero.HP > 0);
    }

    public static Hero GetFreshHero(string heroName) =>
        new(name: heroName, color: "red", maxHP: 5, attackDie: 6);

    public static Style SelectStyle => new Style().Foreground(Color.Red);

    public static void Pause(string prompt = "Proceed") =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>().HighlightStyle(SelectStyle).AddChoices(prompt)
        );

    public static List<Creature> Foes =>
        [
            new Creature(name: "Goblin", color: "chartreuse3", maxHP: 5, attackDie: 4, level: 1),
            new Creature(name: "Cultist", color: "orangered1", maxHP: 8, attackDie: 6, level: 2),
            new Creature(
                name: "Manticore",
                color: "darkgoldenrod",
                maxHP: 15,
                attackDie: 9,
                level: 3
            ),
            new Creature(name: "Lich", color: "royalblue1", maxHP: 18, attackDie: 12, level: 4),
            new Creature(
                name: "Leviathan",
                color: "red",
                maxHP: 50,
                attackDie: 35,
                level: int.MaxValue
            ),
        ];

    public static Creature GetFoe(int heroLevel) =>
        Foes.FirstOrDefault(x => x.Level >= heroLevel, Foes[^1]);

    public static void Loot(this Hero hero, Creature corpse)
    {
        var rule = new Rule("Loot") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        Random rdm = new();
        var loot = rdm.Next(1, corpse.LootDie);

        hero.Gold += loot;
        AnsiConsole.MarkupLine(
            $"You loot the [{corpse.Color}]{corpse.Name}[/] for [orange1]{loot} gold[/] pieces. "
                + "You drop them into your coinpurse."
        );
        Thread.Sleep(500);
        AnsiConsole.MarkupLine(
            $"[grey][[[/][{hero.Color}]{hero.Name}[/][grey] is carrying[/] "
                + $"[orange1]{hero.Gold} gold[/] [grey]pieces]][/]\n"
        );
    }

    public static int Attack(this Creature attacker, Creature defender)
    {
        Random rdm = new();

        bool miss = defender.IsShielded && rdm.Next(1, 5) == 1;

        if (miss)
        {
            AnsiConsole.MarkupLine(
                $"[{defender.Color}]{defender.Name}[/] deflects "
                    + $"[{attacker.Color}]{attacker.Name}[/]'s attack with a shield.\n"
            );
            return 0;
        }

        int atkDmg = rdm.Next(1, attacker.AttackDie);
        int dmgReduction = 0;

        if (defender.IsArmored)
        {
            dmgReduction = rdm.Next(1, 4);
            atkDmg = Math.Max(atkDmg - dmgReduction, 0);

            if (atkDmg <= 0)
            {
                AnsiConsole.MarkupLine(
                    $"[grey][[[/][{defender.Color}]{defender.Name}[/][grey]'s "
                        + "armor negates all damage]][/]"
                );
                return 0;
            }
        }
        var oldHP = defender.HP;
        defender.HP -= atkDmg;

        AnsiConsole.MarkupLine(
            $"[{attacker.Color}]{attacker.Name}[/] attacks [{defender.Color}]{defender.Name}[/] for..."
        );

        AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("red"))
            .Start(
                $"damage",
                ctx =>
                {
                    Thread.Sleep(1000);
                }
            );

        Console.WriteLine();
        AnsiConsole.MarkupLine($"[red]{atkDmg}[/] damage\n");

        defender.LiveHealthBar(oldHP);

        if (dmgReduction > 0)
        {
            Thread.Sleep(500);
            AnsiConsole.MarkupLine(
                $"[grey][[[/][{defender.Color}]{defender.Name}[/][grey]'s "
                    + $"armor reduced damage by {dmgReduction}]][/]"
            );
        }

        return atkDmg;
    }

    public static bool Encounter(this Hero hero, Creature foe)
    {
        var rule = new Rule($"[{foe.Color}]{foe.Name} Battle[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"You encounter a [{foe.Color}]{foe.Name}[/].\n");

        Random rdm = new();
        var heroAttacksFirst = rdm.Next(2) == 1;

        if (heroAttacksFirst)
            AnsiConsole.MarkupLine($"You strike the [{foe.Color}]{foe.Name}[/] first!\n");
        else
            AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] gets the drop on you!\n");

        Creature attacker = heroAttacksFirst ? hero : foe;
        Creature defender = heroAttacksFirst ? foe : hero;

        bool heroAttacking = heroAttacksFirst;

        while (foe.HP > 0 && hero.HP > 0)
        {
            if (heroAttacking)
            {
                Pause("Attack");
            }
            int atkDmg = attacker.Attack(defender);
            Console.WriteLine();

            if (defender.HP <= 0)
            {
                if (!heroAttacking)
                {
                    AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] strikes you down.\n");

                    return false;
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        $"The [{foe.Color}]{foe.Name}[/] falls dead at your feet.\n"
                    );
                    AnsiConsole.MarkupLine($"[{hero.Color}]{hero.Name}[/] stands victorious!");

                    hero.FoesFelled.Add(foe);
                    hero.Experience += foe.MaxHP + foe.AttackDie;

                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        $"[grey][[You gain {foe.MaxHP + foe.AttackDie} XP "
                            + $"for a total {hero.Experience} XP, next level at {hero.LevelXP} XP]][/]\n"
                    );

                    if (hero.Experience >= hero.LevelXP)
                        hero.LevelUp();

                    return true;
                }
            }
            else
            {
                if (heroAttacking)
                {
                    AnsiConsole.MarkupLine(
                        $"The [{foe.Color}]{foe.Name}[/] still stands, sneering at you.\n"
                    );
                    Pause("End turn");
                }
                else
                {
                    if (atkDmg > 0)
                        Console.WriteLine("\nYou are hurt, but not dead yet.");
                    Console.WriteLine("You steel your nerves for another attack.\n");
                }

                attacker = heroAttacking ? foe : hero;
                defender = heroAttacking ? hero : foe;
                heroAttacking = !heroAttacking;
            }
        }

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

        Thread.Sleep(500);
        AnsiConsole.MarkupLine($"[grey][[next level at {hero.LevelXP} XP]][/]\n");

        hero.MaxHP += 3;
        hero.HP = hero.MaxHP;

        Thread.Sleep(500);
        AnsiConsole.MarkupLine($"Your health maximum increases to [green]{hero.MaxHP} HP[/].\n");
    }

    public static void Rest(this Hero hero)
    {
        var rule = new Rule("[dodgerblue1]Rest[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        int oldHP = hero.HP;
        Random rdm = new();

        int newHP = Math.Min(hero.HP + rdm.Next(1, 5), hero.MaxHP);
        int restHP = newHP - oldHP;
        hero.HP += restHP;

        if (hero.HP == hero.MaxHP)
            AnsiConsole.MarkupLine($"You gain [green]{restHP} HP[/] and feel fully restored.");
        else
            AnsiConsole.MarkupLine($"You rest and restore [green]{restHP} HP[/].");

        hero.LiveHealthBar(oldHP);
        Console.WriteLine();
    }

    public static void VisitMerchant(this Hero hero, Dictionary<string, int?> merchantInventory)
    {
        var rule = new Rule("[purple_1]Merchant[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine("You encounter a [purple_1]Merchant[/].");
        AnsiConsole.MarkupLine("[purple_1]\"Hello, weary traveler. See anything you like?\"[/]");

        Thread.Sleep(500);
        AnsiConsole.MarkupLine(
            $"[grey][[[/][{hero.Color}]{hero.Name}[/][grey] is carrying[/] "
                + $"[orange1]{hero.Gold} gold[/] [grey]pieces]][/]\n"
        );

        Thread.Sleep(500);
        var purchase = AnsiConsole.Prompt(
            new SelectionPrompt<KeyValuePair<string, int?>>()
                .HighlightStyle(SelectStyle)
                .AddChoices(merchantInventory)
                .UseConverter(pair =>
                    $"{pair.Key} {(pair.Value.HasValue ? "(" + pair.Value + " gold)" : "")}"
                )
        );

        if (purchase.Key == "None" || hero.Gold < purchase.Value)
        {
            AnsiConsole.MarkupLine(
                "[purple_1]\"Come back when you're ready to spend some coin.\"[/]\n"
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
                Thread.Sleep(500);
                AnsiConsole.MarkupLine(
                    "[purple_1]\"You think this will protect you? Good luck.\"[/]"
                );
            }
            else if (purchase.Key == "Shield")
            {
                hero.IsShielded = true;
                Console.WriteLine("You lift the shield.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine(
                    "[purple_1]\"Ah, the trusty shield. May it guard you well\"[/]"
                );
            }
            else if (purchase.Key == "Morning Star")
            {
                hero.AttackDie = 8;
                hero.CarriesMorningStar = true;
                Console.WriteLine("You heft the morning star.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine(
                    "[purple_1]\"So, you lust for blood. Heh heh... Strike true, warrior.\"[/]"
                );
            }
            Thread.Sleep(500);
            AnsiConsole.MarkupLine(
                $"[grey][[[/][{hero.Color}]{hero.Name}[/][grey] is left with[/] "
                    + $"[orange1]{hero.Gold} gold[/] [grey]pieces]][/]\n"
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

    public static void LiveHealthBar(this Creature creature, int oldHP)
    {
        var healthSegment = new BreakdownChartItem("Health", oldHP, Color.Green);
        var damageSegment = new BreakdownChartItem("Damage", creature.MaxHP - oldHP, Color.Red);
        var chart = new BreakdownChart()
            .Width(creature.MaxHP)
            .AddItems([healthSegment, damageSegment])
            .HideTags();

        var steps = Math.Abs(creature.HP - oldHP);

        AnsiConsole
            .Live(chart)
            .Start(ctx =>
            {
                Thread.Sleep(500);
                for (var i = 1; i <= steps; i++)
                {
                    ctx.Refresh();
                    Thread.Sleep(500 / steps);
                    chart.Data.RemoveAll(x => true);

                    int displayHP;
                    if (oldHP > creature.HP)
                        displayHP = oldHP - i;
                    else
                        displayHP = oldHP + i;

                    chart.AddItem("Health", displayHP, Color.Green);
                    chart.AddItem("Damage", creature.MaxHP - displayHP, Color.Red);
                    ctx.Refresh();
                }
                Thread.Sleep(500 / steps);
            });
    }

    public static void PrintCharacterSheet(this Hero hero)
    {
        var rule = new Rule($"[{hero.Color}]{hero.Name}[/] - Level {hero.Level}")
        {
            Justification = Justify.Left,
        };
        AnsiConsole.Write(rule);

        Thread.Sleep(250);
        hero.PrintHealthBar();

        Thread.Sleep(250);
        AnsiConsole.WriteLine($"{hero.Experience} XP");

        Thread.Sleep(250);
        AnsiConsole.MarkupLine($"[orange1]{hero.Gold} gold[/]");

        Thread.Sleep(250);
        Console.WriteLine("Inventory:");
        var inventoryString = "  Short Sword (d6)";

        if (hero.CarriesMorningStar)
            inventoryString = "\n  Morning Star (d8)";
        if (hero.IsArmored)
            inventoryString += "\n  Leather Armor (-d4)";
        if (hero.IsShielded)
            inventoryString += "\n  Shield";

        AnsiConsole.WriteLine($"{inventoryString}");

        Thread.Sleep(250);
        AnsiConsole.WriteLine($"{hero.FoesFelled.Count} foes vanquished");

        Thread.Sleep(250);
        hero.PrintFelledFoes();
    }

    public static void PrintFelledFoes(this Hero hero)
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

    public static void PrintDeathReport(this Hero hero)
    {
        var rule = new Rule("Death") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"[orange1]{hero.Gold} gold[/] pieces spill out of your coinpurse.");
        Console.WriteLine(
            $"You reached level {hero.Level} and felled {hero.FoesFelled.Count} foes "
                + "before meeting your end."
        );

        hero.PrintFelledFoes();

        AnsiConsole.MarkupLine($"Rest in peace, [{hero.Color}]{hero.Name}[/].\n");
    }
}

public class Creature(string name, string color, int maxHP, int attackDie, int level = 1)
{
    public string Name { get; set; } = name;
    public string Color { get; set; } = color;
    public int HP { get; set; } = maxHP;
    public int MaxHP { get; set; } = maxHP;
    public int AttackDie { get; set; } = attackDie;
    public int LootDie { get; set; } = maxHP + attackDie;
    public bool IsArmored { get; set; } = false;
    public bool IsShielded { get; set; } = false;
    public int Level { get; set; } = level;
}

public class Hero(string name, string color, int maxHP, int attackDie)
    : Creature(name, color, maxHP, attackDie)
{
    public int Experience { get; set; } = 0;
    public int LevelXP { get; set; } = 20;
    public int Gold { get; set; } = 0;
    public List<Creature> FoesFelled { get; set; } = [];
    public bool CarriesMorningStar { get; set; } = false;
}
