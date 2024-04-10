using System;
public class Trait
{
    public static Random rand = new Random();

    public string title;
    public float affection = rand.NextSingle();
    public string description;

    public Trait(string title)
    {
        this.title = title;
        description = "";
    }

    public Trait(string title, string description)
    {
        this.title = title;
        this.description = description;
    }
}