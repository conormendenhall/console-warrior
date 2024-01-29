public class Hero
{
    public string Name { get; set; } = "Nameless Warrior";
    public int HP { get; set; } = 5;
    public int Gold { get; set; }

    public Hero(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            this.Name = name;
        }
    }
}

public class Goblin
{
    public int HP { get; set; } = 5;

    public Goblin(int hp)
    {
        this.HP = hp;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, Warrior!");
        Console.WriteLine("What is your name?");

        var warriorName = Console.ReadLine();
        var hero = new Hero(warriorName);

        Console.WriteLine($"Well met, {hero.Name}.");

        while (hero.HP > 0)
        {
            Console.WriteLine("You encounter a Goblin.");
            var goblin = new Goblin(hp: 5);

            while (goblin.HP > 0 && hero.HP > 0)
            {
                Console.WriteLine("You attack!");

                Random random = new Random();
                var atkDmg = random.Next(5);

                Console.WriteLine($"You deal {atkDmg} damage.");
                goblin.HP -= atkDmg;

                Console.WriteLine($"[goblin has {goblin.HP} HP]");

                if (goblin.HP <= 0)
                {
                    Console.WriteLine("The goblin falls dead at your feet.");
                    Console.WriteLine($"{hero.Name} stands victorious!");

                    var loot = random.Next(5);
                    Console.WriteLine($"You loot the goblin for {loot} gold pieces. You drop them into your coinpurse.");
                    hero.Gold += loot;
                    Console.WriteLine($"[hero has {hero.Gold} gp]");
                }
                else
                {
                    Console.WriteLine("The goblin still stands, sneering at you.");
                    Console.WriteLine("The goblin attacks!");

                    var gobAtkDmg = random.Next(5);

                    Console.WriteLine($"The goblin deals {gobAtkDmg} damage.");
                    hero.HP -= gobAtkDmg;

                    Console.WriteLine($"[hero has {hero.HP} HP]");

                    if (hero.HP <= 0)
                    {
                        Console.WriteLine("The goblin strikes you down.");
                        Console.WriteLine($"{hero.Gold} gold pieces spill out of your coinpurse.");
                        Console.WriteLine($"Rest in peace, {hero.Name}.");
                    }
                    else
                    {
                        Console.WriteLine("You are hurt but not dead yet. You steel your nerves for another attack.");
                    }
                }
            }
        }
    }
}