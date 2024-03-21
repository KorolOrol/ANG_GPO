using System;
public class Trait
{
    public int id;

	public string title;
	public float affection;
	public string description;
    public Trait(string title, bool isRandomAffection, string conflict_trait)
	{
		this.title = title;
        description = "";

        if (isRandomAffection)
        {
            Random rand = new Random();
            affection = rand.NextSingle();
        }
        else { affection = 0.5f; }
    }

    public Trait(string title, bool isRandomAffection)
    {
        this.title = title;
        description = "";

        if (isRandomAffection)
        {
            Random rand = new Random();
            affection = rand.NextSingle();
        }
        else { affection = 0.5f; }
    }

    // Why? Yes.
    public Trait(string title, string description, bool isRandomAffection)
    {
        this.title = title;
        this.description = description;

        if (isRandomAffection)
        {
            Random rand = new Random();
            affection = rand.NextSingle();
        }
        else { affection = 0.5f; }
    }

    public Trait(string title, float affection, string description)
    {
        this.title = title;
        this.affection = affection;
        this.description = description;
    }

    public Trait(string title, float affection)
    {
        this.title = title;
        this.affection = affection;
        description = "";
    }
}