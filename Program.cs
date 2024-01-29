public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, Warrior!");
        Console.WriteLine("What is your name?");

        var warriorName = Console.ReadLine();
        var hero = new Hero(warriorName);

        Console.WriteLine($"Well met, {hero.Name}.");

        Random random = new Random();

        while (hero.HP > 0)
        {
            if (hero.HP < hero.MaxHP)
            {
                var restHP = random.Next(1, 3);
                Console.WriteLine($"You rest and restore {restHP} hit points.");

                if (hero.HP + restHP <= hero.MaxHP)
                {
                    hero.HP += restHP;
                }
                else
                {
                    hero.HP = 5;
                }

                Console.WriteLine($"[hero has {hero.HP} HP]");
            }

            Console.WriteLine("You encounter a Goblin.");
            var goblin = new Goblin(hp: 5);

            while (goblin.HP > 0 && hero.HP > 0)
            {
                Console.WriteLine("You attack!");

                var atkDmg = random.Next(5);

                Console.WriteLine($"You deal {atkDmg} damage.");
                goblin.HP -= atkDmg;

                Console.WriteLine($"[goblin has {goblin.HP} HP]");

                if (goblin.HP <= 0)
                {
                    Console.WriteLine("The goblin falls dead at your feet.");
                    Console.WriteLine($"{hero.Name} stands victorious!");
                    hero.FelledFoes += 1;

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
                        Console.WriteLine($"You felled {hero.FelledFoes} foes before meeting your end.");
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