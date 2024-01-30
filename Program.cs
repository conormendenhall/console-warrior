using Spectre.Console;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, Warrior!");

        var warriorName = AnsiConsole.Ask<string>("What is your [red]name[/]?");
        var hero = new Hero(warriorName);

        AnsiConsole.Markup($"Well met, [red]{hero.Name}[/].\n");

        Random random = new Random();

        while (hero.HP > 0)
        {
            if (hero.HP < hero.MaxHP)
            {
                var restHP = random.Next(1, 3);

                if (hero.HP + restHP <= hero.MaxHP)
                {
                    hero.HP += restHP;
                    Console.WriteLine($"You rest and restore {restHP} HP.");
                }
                else
                {
                    hero.HP = 5;
                    Console.WriteLine($"You rest and feel fully restored.");
                }
                Console.WriteLine($"You have {hero.HP} HP.\n");
            }

            AnsiConsole.Markup($"You encounter a [chartreuse3]goblin[/].");
            var goblin = new Goblin(hp: 5);

            while (goblin.HP > 0 && hero.HP > 0)
            {
                Console.WriteLine("You attack!");

                var atkDmg = random.Next(5);

                Console.WriteLine($"You deal {atkDmg} damage.");
                goblin.HP -= atkDmg;

                if (goblin.HP <= 0)
                {
                    AnsiConsole.Markup($"The [chartreuse3]goblin[/] falls dead at your feet.\n");
                    Console.WriteLine($"{hero.Name} stands victorious!\n");
                    hero.FelledFoes += 1;

                    var loot = random.Next(5);
                    AnsiConsole.Markup($"You loot the [chartreuse3]goblin[/] for {loot} gold pieces. You drop them into your coinpurse.\n");
                    hero.Gold += loot;
                    Console.WriteLine($"You are carrying {hero.Gold} gold.\n");
                }
                else
                {
                    AnsiConsole.Markup("The [chartreuse3]goblin[/] still stands, sneering at you.\n");
                    Console.WriteLine();

                    AnsiConsole.Markup("The [chartreuse3]goblin[/] attacks!\n");
                    var gobAtkDmg = random.Next(5);

                    AnsiConsole.Markup($"The [chartreuse3]goblin[/] deals {gobAtkDmg} damage.\n");
                    hero.HP -= gobAtkDmg;

                    Console.WriteLine($"[hero has {hero.HP} HP]");

                    if (hero.HP <= 0)
                    {
                        AnsiConsole.Markup("The [chartreuse3]goblin[/] strikes you down.\n");
                        Console.WriteLine($"{hero.Gold} gold pieces spill out of your coinpurse.");
                        Console.WriteLine($"You felled {hero.FelledFoes} foes before meeting your end.");
                        Console.WriteLine($"Rest in peace, {hero.Name}.");
                    }
                    else
                    {
                        Console.WriteLine($"You are hurt but not dead yet. You steel your nerves for another attack.{Environment.NewLine}");
                    }
                }
            }
        }
    }
}