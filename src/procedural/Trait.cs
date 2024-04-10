using System;
public class Trait
{
    public static Random rand = new Random();

    public string title;
    public float affection = rand.NextSingle();
    public List<string> description = new List<string> {};

    public Trait(string title)
    {
        this.title = title;
    }

    public Trait(string title, string description)
    {
        this.title = title;
        this.description.Add(description);
    }

    public Trait(string title, List<string> description)
    {
        this.title = title;
        this.description = description;
    }
}