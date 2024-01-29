public class Hero
{
    public string Name { get; set; } = "Nameless Warrior";
    public int MaxHP { get; set; } = 5;
    public int HP { get; set; } = 5;
    public int Gold { get; set; }
    public int FelledFoes { get; set; }

    public Hero(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            this.Name = name;
        }
    }
}
