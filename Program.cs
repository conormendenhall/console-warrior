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

        do
        {
            Pause("Venture forth");
            Creature foe = GetFoe(hero.Level);
            bool heroSurvives = hero.Encounter(foe);

            if (heroSurvives)
            {
                hero.Loot(foe);

                hero.PrintCharacterSheet();

                if (hero.HP < hero.MaxHP)
                {
                    hero.Rest();
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
            { "None", null }
        };

    public static Style SelectStyle => new Style().Foreground(Color.Red);

    public static void Pause(string prompt = "Proceed") =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>().HighlightStyle(SelectStyle).AddChoices(prompt)
        );

    public static List<Creature> Foes =>
        [
            new Creature(name: "Goblin", color: "chartreuse3", maxHP: 5, damageDie: 3, level: 1),
            new Creature(name: "Skeleton", color: "grey", maxHP: 5, damageDie: 5, level: 2),
            new Creature(name: "Brigand", color: "orange4", maxHP: 6, damageDie: 7, level: 3),
            new Creature(name: "Cultist", color: "orangered1", maxHP: 7, damageDie: 9, level: 4),
            new Creature(name: "Cyclops", color: "turquoise4", maxHP: 10, damageDie: 9, level: 5),
            new Creature(name: "Hag", color: "rosybrown", maxHP: 13, damageDie: 10, level: 6),
            new Creature(
                name: "Manticore",
                color: "darkgoldenrod",
                maxHP: 16,
                damageDie: 11,
                level: 7
            ),
            new Creature(name: "Lich", color: "royalblue1", maxHP: 19, damageDie: 13, level: 8),
            new Creature(
                name: "Leviathan",
                color: "red",
                maxHP: 50,
                damageDie: 35,
                level: int.MaxValue
            ),
        ];

    public static Creature GetFoe(int heroLevel) =>
        Foes.FirstOrDefault(x => x.Level >= heroLevel, Foes[^1]);

    public static void Loot(this Hero hero, Creature corpse)
    {
        Pause("Loot");
        var rule = new Rule("Loot") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        Random rdm = new();
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
                    $"[grey][[[/][{defender.Color}]{defender.Name}[/][grey]'s "
                        + "armor negated all damage]][/]"
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

        if (dmgReduction > 0)
        {
            Thread.Sleep(500);
            AnsiConsole.MarkupLine(
                $"[grey][[[/][{defender.Color}]{defender.Name}[/][grey]'s "
                    + $"armor reduced damage by {dmgReduction}]][/]"
            );
        }
        defender.LiveHealthBar(oldHP);

        return atkDmg;
    }

    public static bool Encounter(this Hero hero, Creature foe)
    {
        var rule = new Rule($"[{foe.Color}]{foe.Name} Battle[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"You encounter a [{foe.Color}]{foe.Name}[/].\n");
        Thread.Sleep(1000);

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
                    hero.Experience += foe.MaxHP + foe.DamageDie;

                    Thread.Sleep(500);
                    AnsiConsole.MarkupLine(
                        $"[grey][[You gain {foe.MaxHP + foe.DamageDie} XP "
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

    public static void Rest(this Hero hero)
    {
        Pause("Rest");
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
        Pause("Trade");
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
                if (merchantInventory.Count == 1)
                    AnsiConsole.MarkupLine(
                        "[purple_1]\"You've cleared me out. "
                            + "Guess I'll head back to the city to restock.\"[/]"
                    );
            }

            if (purchase.Key == "Shield")
            {
                hero.IsShielded = true;
                Console.WriteLine("You lift the shield.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine(
                    "[purple_1]\"Ah, the trusty shield. May it guard you well\"[/]"
                );
            }
            else if (purchase.Key == "Leather Armor")
            {
                hero.ArmorDie = 4;
                Console.WriteLine("You don the leather armor.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine(
                    "[purple_1]\"You think this will protect you? Good luck.\"[/]"
                );
            }
            else if (purchase.Key == "Morning Star")
            {
                hero.DamageDie = 8;
                Console.WriteLine("You heft the morning star.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine("[purple_1]\"So, you lust for blood. Heh heh...\"[/]");
            }
            else if (purchase.Key == "Chain Mail")
            {
                hero.ArmorDie = 6;
                Console.WriteLine("You don the chain armor.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine("[purple_1]\"See these links? They hold strong.\"[/]");
            }
            else if (purchase.Key == "Claymore")
            {
                hero.DamageDie = 10;
                Console.WriteLine("You raise the claymore.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine("[purple_1]\" Strike true, warrior.\"[/]");
            }
            else if (purchase.Key == "Scale Armor")
            {
                hero.ArmorDie = 8;
                Console.WriteLine("You don the scale armor.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine("[purple_1]\"Ah, look how it shimmers. Heh...\"[/]");
            }
            else if (purchase.Key == "Lucerne")
            {
                hero.DamageDie = 12;
                Console.WriteLine("You grip the lucerne.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine("[purple_1]\"Be careful where you swing that thing.\"[/]");
            }
            else if (purchase.Key == "Plate Armor")
            {
                hero.ArmorDie = 10;
                Console.WriteLine("You don the plate armor.");
                Thread.Sleep(500);
                AnsiConsole.MarkupLine("[purple_1]\"This steel is nigh impenetrable.\"[/]");
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
        Pause("View character sheet");
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

        if (hero.DamageDie == 6)
            inventoryString += "  Short Sword (d6)\n";
        if (hero.IsShielded)
            inventoryString += "  Shield (1/5 chance to deflect attack)\n";
        if (hero.ArmorDie == 4)
            inventoryString += "  Leather Armor (-d4)\n";
        if (hero.DamageDie == 8)
            inventoryString = "  Morning Star (d8)\n";
        if (hero.ArmorDie == 6)
            inventoryString = "  Chain Mail (-d6)\n";
        if (hero.DamageDie == 10)
            inventoryString = "  Claymore (d10)\n";
        if (hero.ArmorDie == 8)
            inventoryString = "  Scale Armor (-d8)\n";
        if (hero.DamageDie == 12)
            inventoryString = "  Lucerne (d12)\n";
        if (hero.ArmorDie == 10)
            inventoryString = "  Plate Armor (-d10)\n";

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
                    new Text($"{foe.Name}s:"),
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
}
