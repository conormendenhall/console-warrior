using Spectre.Console;

namespace ConsoleWarrior;

public static class Program
{
    public static void Main(string[] args)
    {
        var font = FigletFont.Load("epic.flf");
        AnsiConsole.Write(new FigletText(font, "Hello, Warrior!").LeftJustified().Color(Color.Red));

        var nameInput = AnsiConsole.Prompt(
            new TextPrompt<string>("What is your [red]name[/]?").AllowEmpty()
        );
        string warriorName = string.IsNullOrWhiteSpace(nameInput) ? "Nameless Warrior" : nameInput;
        var hero = new Hero(name: warriorName, color: "red", maxHP: 5, attackDie: 5);

        AnsiConsole.MarkupLine($"Well met, [{hero.Color}]{hero.Name}[/].\n");

        do
        {
            Creature foe = GetFoe(hero.FelledFoes);
            bool heroSurvives = hero.Encounter(foe);

            if (heroSurvives)
            {
                if (hero.HP < hero.MaxHP)
                {
                    Pause("Rest");
                    hero.Rest();
                }
                Pause("Trade");
                hero.VisitMerchant();
            }
            else
            {
                var rule = new Rule("Death") { Justification = Justify.Left };
                AnsiConsole.Write(rule);
                AnsiConsole.MarkupLine(
                    $"[orange1]{hero.Gold} gold[/] pieces spill out of your coinpurse."
                );
                Console.WriteLine($"You felled {hero.FelledFoes} foes before meeting your end.");
                AnsiConsole.MarkupLine($"Rest in peace, [{hero.Color}]{hero.Name}[/].");
            }
        } while (hero.HP > 0);
    }

    public static Style SelectStyle => new Style().Foreground(Color.Red);

    public static void Pause(string prompt = "Proceed") =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>().HighlightStyle(SelectStyle).AddChoices(prompt)
        );

    public static Creature GetFoe(int felledFoes) =>
        felledFoes switch
        {
            <= 3
                => new Creature(
                    name: "Goblin",
                    color: "chartreuse3",
                    maxHP: 5,
                    attackDie: 4,
                    gold: 4
                ),
            <= 6
                => new Creature(
                    name: "Cultist",
                    color: "orangered1",
                    maxHP: 8,
                    attackDie: 6,
                    gold: 6
                ),
            <= 8
                => new Creature(
                    name: "Manticore",
                    color: "darkgoldenrod",
                    maxHP: 15,
                    attackDie: 9,
                    gold: 8
                ),
            > 8
                => new Creature(
                    name: "Lich",
                    color: "royalblue1",
                    maxHP: 18,
                    attackDie: 12,
                    gold: 15
                ),
        };
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

        bool miss = foe.IsShielded && rdm.Next(1, 5) == 1;

        if (miss)
        {
            Console.WriteLine("The shield deflected the attack.");
            return 0;
        }

        int atkDmg = rdm.Next(1, AttackDie);

        if (foe.IsArmored)
        {
            int dmgReduction = rdm.Next(1, 4);
            AnsiConsole.MarkupLine($"[grey][[armor reduced damage by {dmgReduction}]][/]");
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

    public Dictionary<string, int?> MerchantInventory { get; set; } =
        new()
        {
            { "Leather Armor", 8 },
            { "Shield", 10 },
            { "Morning Star", 12 },
            { "None", null }
        };

    public void Rest()
    {
        var rule = new Rule("[dodgerblue1]Rest[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);

        Random rdm = new();
        int restHP = rdm.Next(1, 5);

        if (HP + restHP <= MaxHP)
        {
            Console.WriteLine($"You rest and restore {restHP} HP.");
            HP += restHP;
            AnsiConsole.Write(HealthTable(Name, HP, MaxHP, Color));
        }
        else
        {
            Console.WriteLine($"You gain {MaxHP - HP} HP and feel fully restored.");
            HP = MaxHP;
            AnsiConsole.Write(HealthTable(Name, HP, MaxHP, Color));
        }
        AnsiConsole.MarkupLine($"[grey][[hero has {HP} HP]][/]\n");
    }

    public int Loot(Creature corpse) => Gold += corpse.Gold;

    public bool Encounter(Creature foe)
    {
        var rule = new Rule($"[{foe.Color}]{foe.Name} Battle[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"You encounter a [{foe.Color}]{foe.Name}[/].\n");

        do
        {
            Program.Pause("Attack");
            Console.WriteLine("You attack!");

            int atkDmg = Attack(foe);
            AnsiConsole.MarkupLine($"[{foe.Color}]{foe.Name}[/] takes {atkDmg} damage.");
            AnsiConsole.MarkupLine($"[grey][[{foe.Name} has {foe.HP} HP]][/]");

            var foeDisplayHP = Math.Max(foe.HP, 0);
            AnsiConsole.Write(HealthTable(foe.Name, foeDisplayHP, foe.MaxHP, foe.Color));

            if (foe.HP <= 0)
            {
                AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] falls dead at your feet.");
                AnsiConsole.MarkupLine($"[{Color}]{Name}[/] stands victorious!\n");
                FelledFoes += 1;

                int loot = Loot(foe);
                AnsiConsole.MarkupLine(
                    $"You loot the [{foe.Color}]{foe.Name}[/] for [orange1]{loot} gold[/] pieces. "
                        + "You drop them into your coinpurse."
                );
                AnsiConsole.MarkupLine(
                    $"[grey][[hero is carrying[/] [orange1]{Gold} gold[/] [grey]pieces]][/]\n"
                );

                return true;
            }
            else
            {
                AnsiConsole.MarkupLine(
                    $"The [{foe.Color}]{foe.Name}[/] still stands, sneering at you.\n"
                );
                AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] attacks!");
                int foeAtkDmg = foe.Attack(this);
                Console.WriteLine($"You take {foeAtkDmg} damage.");
                AnsiConsole.MarkupLine($"[grey][[hero has {HP} HP]][/]");

                var heroDisplayHP = Math.Max(HP, 0);
                AnsiConsole.Write(HealthTable(Name, heroDisplayHP, MaxHP, Color));

                if (HP <= 0)
                {
                    AnsiConsole.MarkupLine($"The [{foe.Color}]{foe.Name}[/] strikes you down.\n");

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

    public static Canvas HealthBar(int hp, int maxHP)
    {
        var canvas = new Canvas(maxHP, 1);

        for (var i = 0; i < hp; i++)
        {
            canvas.SetPixel(i, 0, Spectre.Console.Color.Green);
        }
        for (var i = hp; i < maxHP; i++)
        {
            canvas.SetPixel(i, 0, Spectre.Console.Color.Red);
        }

        return canvas;
    }

    public static Table HealthTable(string name, int hp, int maxHP, string color)
    {
        var table = new Table();
        table.HideHeaders();
        table.Border(TableBorder.Simple);

        table.AddColumn("Combatant");
        table.AddColumn("Health");

        table.AddRow(new Markup($"[{color}]{name}[/]"), HealthBar(hp, maxHP));

        return table;
    }

    public void VisitMerchant()
    {
        var rule = new Rule("[royalblue1]Merchant[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine("You encounter a [royalblue1]Merchant[/].");
        AnsiConsole.MarkupLine(
            "[royalblue1]\"Hello, weary traveler. See anything you like?\"[/]\n"
        );
        AnsiConsole.MarkupLine(
            $"[grey][[hero is carrying[/] [orange1]{Gold} gold[/] [grey]pieces]][/]"
        );

        var purchase = AnsiConsole.Prompt(
            new SelectionPrompt<KeyValuePair<string, int?>>()
                .HighlightStyle(Program.SelectStyle)
                .AddChoices(MerchantInventory)
                .UseConverter(pair => $"{pair.Key} {pair.Value}")
        );

        if (purchase.Key == "Leather Armor" && Gold >= 8)
        {
            MerchantInventory.Remove(purchase.Key);
            Gold -= 8;
            IsArmored = true;
            Console.WriteLine("You don the leather armor.");
            AnsiConsole.MarkupLine(
                "[royalblue1]\"You think this will protect you? Good luck.\"[/]"
            );
            AnsiConsole.MarkupLine($"You are left with [orange1]{Gold} gold[/] pieces.\n");
        }
        else if (purchase.Key == "Shield" && Gold >= 10)
        {
            MerchantInventory.Remove(purchase.Key);
            Gold -= 10;
            IsShielded = true;
            Console.WriteLine("You lift the shield.");
            AnsiConsole.MarkupLine(
                "[royalblue1]\"Ah, the trusty shield. May it guard you well\"[/]"
            );
            AnsiConsole.MarkupLine($"You are left with [orange1]{Gold} gold[/] pieces.\n");
        }
        else if (purchase.Key == "Morning Star" && Gold >= 12)
        {
            MerchantInventory.Remove(purchase.Key);
            Gold -= 12;
            AttackDie = 8;
            Console.WriteLine("You heft the morningstar.");
            AnsiConsole.MarkupLine(
                "[royalblue1]\"So, you lust for blood. Heh heh... Strike true, warrior.\"[/]"
            );
            AnsiConsole.MarkupLine($"You are left with [orange1]{Gold} gold[/] pieces.\n");
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[royalblue1]\"Come back when you're ready to spend some coin.\"[/]\n"
            );
        }
    }
}
