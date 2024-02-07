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

        var hero = RefreshHero(heroName);
        Dictionary<string, int?> merchantInventory = RefreshMerchantInventory();

        AnsiConsole.MarkupLine($"Well met, [{hero.Color}]{hero.Name}[/].\n");

        Random rdm = new();

        do
        {
            SelectPrompt("Venture forth");
            Creature foe = GetFoe(hero.Level, rdm);
            bool heroSurvives = hero.Encounter(foe, rdm);

            if (heroSurvives)
            {
                if (hero.HP < hero.MaxHP)
                {
                    hero.Rest(rdm);
                }
                if (merchantInventory.Count > 1)
                    hero.VisitMerchant(merchantInventory);
            }
            else
            {
                hero.PrintDeathReport(foe);
                if (AnsiConsole.Confirm("Play again?"))
                {
                    hero = RefreshHero(heroName);
                    merchantInventory = RefreshMerchantInventory();

                    Console.WriteLine();
                    AnsiConsole.MarkupLine("[red]Hello again, warrior[/]\n");
                }
            }
        } while (hero.HP > 0);
    }

    public static Hero RefreshHero(string heroName) =>
        new(name: heroName, color: "red", maxHP: 5, damageDie: 6);

    public static Dictionary<string, int?> RefreshMerchantInventory() =>
        new()
        {
            { "Shield", 8 },
            { "Leather Armor", 10 },
            { "Morning Star", 12 },
            { "Chain Mail", 14 },
            { "Claymore", 16 },
            { "Scale Armor", 16 },
            { "Lucerne", 18 },
            { "Plate Armor", 20 },
            { "Cloak of Invisibility", 40 },
            { "None", null }
        };

    public static Style SelectStyle => new Style().Foreground(Color.Red);

    public static bool SelectPrompt(string prompt = "Proceed")
    {
        return AnsiConsole.Prompt(
                new SelectionPrompt<string>().HighlightStyle(SelectStyle).AddChoices(prompt)
            ) == prompt;
    }

    public static List<Creature> Foes =>
        [
            new Creature(name: "Goblin", color: "chartreuse3", maxHP: 5, damageDie: 3, level: 1),
            new Creature(name: "Skeleton", color: "grey", maxHP: 6, damageDie: 5, level: 2),
            new Creature(name: "Brigand", color: "orange4", maxHP: 8, damageDie: 6, level: 3),
            new Creature(name: "Cultist", color: "orangered1", maxHP: 6, damageDie: 8, level: 3),
            new Creature(name: "Hag", color: "rosybrown", maxHP: 8, damageDie: 9, level: 4),
            new Creature(name: "Manticore", color: "tan", maxHP: 12, damageDie: 8, level: 5),
            new Creature(
                name: "Bog Shambler",
                color: "chartreuse4",
                maxHP: 14,
                damageDie: 6,
                level: 5
            ),
            new Creature(name: "Cyclops", color: "turquoise4", maxHP: 13, damageDie: 10, level: 6),
            new Creature(name: "Demonoid", color: "darkmagenta", maxHP: 9, damageDie: 14, level: 6),
            new Creature(name: "Warlock", color: "deeppink1", maxHP: 11, damageDie: 15, level: 7),
            new Creature(name: "Vampire", color: "red3", maxHP: 13, damageDie: 13, level: 7),
            new Creature(name: "Lich", color: "royalblue1", maxHP: 16, damageDie: 13, level: 8),
            new Creature(name: "Wyvern", color: "teal", maxHP: 16, damageDie: 16, level: 9),
            new Creature(
                name: "Leviathan",
                color: "red",
                maxHP: 50,
                damageDie: 35,
                level: int.MaxValue
            ),
        ];

    public static Creature GetFoe(int heroLevel, Random rdm)
    {
        return Foes.Where(x => Math.Abs(x.Level - heroLevel) < 2)
            .OrderBy(x => rdm.Next())
            .FirstOrDefault(Foes[^1]);
    }

    public static void Loot(this Hero hero, Creature corpse, Random rdm)
    {
        SelectPrompt("Loot");
        var rule = new Rule("Loot") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        var loot = rdm.Next(1, corpse.LootDie + 1);

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
                    + $"[{attacker.Color}]{attacker.Name}[/]'s attack with a shield."
            );
            return 0;
        }

        int atkDmg = rdm.Next(1, attacker.DamageDie + 1);
        int dmgReduction = 0;

        if (defender.ArmorDie > 0)
        {
            dmgReduction = rdm.Next(1, defender.ArmorDie + 1);
            atkDmg = Math.Max(atkDmg - dmgReduction, 0);

            if (atkDmg <= 0)
            {
                AnsiConsole.MarkupLine(
                    $"The [{attacker.Color}]{attacker.Name}[/] strikes! "
                        + $"But, the blow is deflected by your armor."
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

        string reductionMessage =
            dmgReduction > 0
                ? $"[grey][[[/][{defender.Color}]{defender.Name}[/][grey]'s "
                    + $"armor reduced damage by {dmgReduction}]][/]"
                : "";
        Console.WriteLine();
        AnsiConsole.MarkupLine($"[red]{atkDmg}[/] damage {reductionMessage}\n");

        defender.LiveHealthBar(oldHP);

        return atkDmg;
    }

    public static bool Encounter(this Hero hero, Creature foe, Random rdm)
    {
        var rule = new Rule($"[{foe.Color}]{foe.Name}[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"You encounter a [{foe.Color}]{foe.Name}[/].\n");
        Thread.Sleep(1000);

        bool attackConfirmed = false;
        bool sneakSuccessful = rdm.Next(1, 4) == 1;

        if (hero.IsCloaked && sneakSuccessful)
        {
            AnsiConsole.MarkupLine(
                $"Your cloak allows you approach the [{foe.Color}]{foe.Name}[/] unseen.\n"
            );

            if (
                AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .HighlightStyle(SelectStyle)
                        .AddChoices("Attack", "Sneak past")
                ) == "Sneak past"
            )
            {
                AnsiConsole.MarkupLine($"You slip past the [{foe.Color}]{foe.Name}[/].\n");
                return true;
            }
            else
                attackConfirmed = true;
        }

        bool ambushed = !hero.IsCloaked && rdm.Next(2) == 1;

        if (hero.IsCloaked && !attackConfirmed && SelectPrompt("Attack"))
            AnsiConsole.MarkupLine($"From the shadows, you make a sneak attack!\n");
        else if (ambushed)
            AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] ambushes you!\n");
        else
            AnsiConsole.MarkupLine($"You strike first!\n");

        Creature attacker = ambushed ? foe : hero;
        Creature defender = ambushed ? hero : foe;

        bool heroAttacking = !ambushed;

        while (foe.HP > 0 && hero.HP > 0)
        {
            if (heroAttacking && !hero.IsCloaked)
            {
                SelectPrompt("Attack");
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
                    hero.Experience += foe.MaxHP + foe.DamageDie;

                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        $"[grey][[You gain {foe.MaxHP + foe.DamageDie} XP "
                            + $"for a total {hero.Experience} XP, next level at {hero.LevelXP} XP]][/]\n"
                    );

                    if (hero.Experience >= hero.LevelXP)
                        hero.LevelUp();

                    hero.Loot(foe, rdm);

                    hero.PrintCharacterSheet();

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
                    SelectPrompt("End turn");
                }
                else
                {
                    if (atkDmg > 0)
                        Console.WriteLine("You are hurt, but not dead yet.");
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
        var rule = new Rule("[green]You Level Up![/]") { Justification = Justify.Left };
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

    public static void Rest(this Hero hero, Random rdm)
    {
        SelectPrompt("Rest");
        var rule = new Rule("[dodgerblue1]Rest[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        int oldHP = hero.HP;

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
        SelectPrompt("Trade");
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

            switch (purchase.Key)
            {
                case "Shield":
                    hero.IsShielded = true;
                    Console.WriteLine("You lift the shield.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        "[purple_1]\"Ah, the trusty shield. May it guard you well\"[/]"
                    );
                    break;
                case "Leather Armor":
                    hero.ArmorDie = 4;
                    Console.WriteLine("You don the leather armor.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        "[purple_1]\"You think this will protect you? Good luck.\"[/]"
                    );
                    break;
                case "Morning Star":
                    hero.DamageDie = 8;
                    Console.WriteLine("You heft the morning star.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine("[purple_1]\"So, you lust for blood. Heh heh...\"[/]");
                    break;
                case "Chain Mail":
                    hero.ArmorDie = 6;
                    Console.WriteLine("You don the chain armor.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        "[purple_1]\"See these links? They may save your hide.\"[/]"
                    );
                    break;
                case "Claymore":
                    hero.DamageDie = 10;
                    Console.WriteLine("You raise the claymore.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine("[purple_1]\"Strike true, warrior.\"[/]");
                    break;
                case "Scale Armor":
                    hero.ArmorDie = 8;
                    Console.WriteLine("You don the scale armor.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine("[purple_1]\"Ah, look how it shimmers. Heh...\"[/]");
                    break;
                case "Lucerne":
                    hero.DamageDie = 12;
                    Console.WriteLine("You grip the lucerne.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        "[purple_1]\"Be careful where you swing that thing.\"[/]"
                    );
                    break;
                case "Plate Armor":
                    hero.ArmorDie = 10;
                    Console.WriteLine("You don the plate armor.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine("[purple_1]\"This steel is nigh impenetrable.\"[/]");
                    break;
                case "Cloak of Invisibility":
                    hero.IsCloaked = true;
                    Console.WriteLine("You wrap yourself in the cloak and slip into shadow.");
                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        "[purple_1]\"I wonder, what will you do when no one can see you?\"[/]"
                    );
                    break;
            }
            Thread.Sleep(500);
            AnsiConsole.MarkupLine(
                $"[grey][[[/][{hero.Color}]{hero.Name}[/][grey] is left with[/] "
                    + $"[orange1]{hero.Gold} gold[/] [grey]pieces]][/]\n"
            );

            if (merchantInventory.Count == 1)
                AnsiConsole.MarkupLine(
                    "[purple_1]\"You've cleared me out. "
                        + "Guess I'll head back to the city to restock.\"[/]"
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
        SelectPrompt("View character sheet");
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
        string inventoryString = "";

        switch (hero.DamageDie)
        {
            case 6:
                inventoryString += "  Short Sword (d6)\n";
                break;
            case 8:
                inventoryString += "  Morning Star (d8)\n";
                break;
            case 10:
                inventoryString += "  Claymore (d10)\n";
                break;
            case 12:
                inventoryString += "  Lucerne (d12)\n";
                break;
        }

        switch (hero.ArmorDie)
        {
            case 4:
                inventoryString += "  Leather Armor (-d4)\n";
                break;
            case 6:
                inventoryString += "  Chain Mail (-d6)\n";
                break;
            case 8:
                inventoryString += "  Scale Armor (-d8)\n";
                break;
            case 10:
                inventoryString += "  Plate Armor (-d10)\n";
                break;
        }

        if (hero.IsShielded)
            inventoryString += "  Shield (1 in 5 chance to deflect attack)\n";

        if (hero.IsCloaked)
            inventoryString +=
                "  Cloak (1 in 3 chance to pass undetected, and can't be ambushed)\n";

        AnsiConsole.Write($"{inventoryString}");

        Thread.Sleep(250);
        Console.WriteLine();
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

        for (int i = 0; i < Foes.Count; i++)
        {
            Creature foe = Foes[i];

            if (hero.FoesFelled.Exists(x => x.Name == foe.Name))
            {
                felledFoes.AddRow(
                    new Text($"{foe.PluralName}:"),
                    new Text(hero.FoesFelled.Count(x => x.Name == foe.Name).ToString())
                );
            }
        }
        AnsiConsole.Write(felledFoes);
    }

    public static void PrintDeathReport(this Hero hero, Creature killer)
    {
        var rule = new Rule("Death") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"[orange1]{hero.Gold} gold[/] pieces spill out of your coinpurse.");
        Console.WriteLine(
            $"You reached level {hero.Level} and felled {hero.FoesFelled.Count} foes "
                + "before meeting your end."
        );

        hero.PrintFelledFoes();

        AnsiConsole.MarkupLine($"Slain by a [{killer.Color}]{killer.Name}[/].\n");

        AnsiConsole.MarkupLine($"Rest in peace, [{hero.Color}]{hero.Name}[/].\n");
    }
}

public class Creature(string name, string color, int maxHP, int damageDie, int level = 1)
{
    public string Name { get; set; } = name;
    public string PluralName { get; set; } = name == "Cyclops" ? "Cyclopes" : name + "s";
    public string Color { get; set; } = color;
    public int HP { get; set; } = maxHP;
    public int MaxHP { get; set; } = maxHP;
    public int DamageDie { get; set; } = damageDie;
    public int LootDie { get; set; } = maxHP + damageDie;
    public int ArmorDie { get; set; } = 0;
    public bool IsShielded { get; set; } = false;
    public int Level { get; set; } = level;
}

public class Hero(string name, string color, int maxHP, int damageDie)
    : Creature(name, color, maxHP, damageDie)
{
    public int Experience { get; set; } = 0;
    public int LevelXP { get; set; } = 20;
    public int Gold { get; set; } = 0;
    public List<Creature> FoesFelled { get; set; } = [];
    public bool IsCloaked { get; set; } = false;
}
