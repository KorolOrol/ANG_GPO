using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Intrinsics.X86;

public class Character
{

    #region [ Character characteristics ]

    public string name;
    public int    age;
    public string description;
    public string location;
    public float  affected_by_traits = 0.5f;
    private const int max_possible_traits = 10;

    public List<Trait> traits = new List<Trait>();
    /*public List<Item> items = new List<Item>();*/
    /*public List<Event> events = new List<Event>;*/
    public Dictionary<Character, float> relations;
    // TODO: Фобии

    #endregion

    #region [Tabl and Traits List]

    public int[,] tabl = new int[,]
    {
        {0,   0,   3,   2,   3,   2,   2,   2,   3,   1,   2,   1,   1,   3,   2,   2,   0,   3,   3,   1,   2,   3,   1,   1,   1,   3,   3,   2,   1,   2,   1,   2,   0,   3,   1,   1,   3,   3,   1,   1,   2},
        {0,   0,   1,   3,   2,   2,   3,   1,   1,   3,   1,   2,   1,   1,   2,   1,   3,   0,   2,   1,   2,   0,   2,   1,   2,   1,   1,   1,   2,   1,   1,   2,   2,   1,   1,   1,   2,   2,   2,   2,   1},
        {3,   1,   0,   0,   2,   2,   2,   2,   2,   1,   2,   1,   2,   1,   2,   2,   1,   1,   2,   2,   1,   2,   1,   1,   2,   1,   1,   1,   2,   2,   1,   1,   1,   1,   1,   1,   2,   3,   1,   3,   1},
        {2,   3,   0,   0,   1,   2,   1,   1,   1,   1,   2,   2,   1,   2,   1,   3,   1,   2,   3,   1,   1,   2,   1,   2,   0,   3,   2,   1,   1,   2,   3,   2,   1,   1,   1,   1,   3,   2,   1,   1,   2},
        {3,   2,   2,   1,   0,   0,   2,   1,   2,   2,   2,   1,   1,   1,   2,   2,   2,   1,   1,   1,   2,   0,   2,   1,   1,   0,   0,   2,   1,   1,   1,   1,   2,   1,   1,   1,   1,   0,   1,   1,   1},
        {2,   2,   2,   2,   0,   0,   1,   1,   1,   1,   2,   1,   1,   1,   2,   1,   1,   2,   1,   2,   0,   3,   1,   1,   2,   2,   3,   2,   1,   2,   2,   2,   1,   2,   2,   0,   2,   3,   1,   2,   1},
        {2,   3,   2,   1,   2,   1,   0,   0,   2,   1,   1,   1,   1,   1,   2,   1,   3,   1,   2,   1,   2,   1,   2,   1,   1,   3,   2,   1,   1,   1,   1,   2,   1,   1,   1,   1,   2,   1,   1,   1,   1},
        {2,   1,   2,   1,   1,   1,   0,   0,   1,   1,   2,   2,   1,   2,   3,   2,   2,   3,   1,   2,   2,   3,   1,   1,   1,   3,   2,   1,   1,   1,   1,   1,   1,   1,   3,   3,   3,   3,   1,   1,   1},
        {3,   1,   2,   1,   2,   1,   2,   1,   0,   0,   1,   1,   1,   2,   2,   1,   2,   2,   2,   1,   3,   1,   2,   1,   1,   2,   2,   1,   1,   1,   1,   2,   1,   2,   2,   2,   2,   3,   1,   1,   1},
        {1,   3,   1,   1,   2,   1,   1,   1,   0,   0,   1,   1,   1,   2,   1,   2,   1,   2,   1,   1,   2,   2,   1,   3,   1,   2,   1,   2,   1,   1,   2,   3,   2,   1,   2,   3,   1,   1,   1,   1,   1},
        {2,   1,   2,   2,   2,   2,   1,   2,   1,   1,   0,   0,   2,   1,   1,   3,   1,   1,   1,   3,   1,   3,   1,   1,   2,   2,   1,   1,   2,   1,   1,   1,   2,   2,   1,   1,   3,   3,   2,   2,   1},
        {1,   2,   1,   2,   1,   1,   1,   2,   1,   1,   0,   0,   2,   1,   1,   1,   2,   1,   1,   1,   2,   3,   2,   1,   1,   2,   0,   2,   1,   2,   1,   1,   2,   0,   2,   2,   1,   1,   1,   1,   1},
        {1,   1,   2,   1,   1,   1,   1,   1,   1,   1,   2,   2,   0,   0,   2,   1,   2,   0,   3,   2,   1,   3,   1,   0,   2,   1,   2,   0,   3,   1,   1,   1,   1,   1,   2,   1,   2,   2,   1,   3,   0},
        {3,   1,   1,   2,   1,   1,   1,   2,   2,   2,   1,   1,   0,   0,   1,   2,   1,   3,   2,   1,   1,   3,   2,   2,   0,   3,   2,   2,   1,   2,   1,   3,   1,   2,   3,   3,   3,   1,   1,   1,   2},
        {2,   2,   2,   1,   2,   2,   2,   3,   2,   1,   1,   1,   2,   1,   0,   0,   2,   1,   2,   1,   2,   2,   2,   1,   2,   2,   3,   1,   2,   2,   1,   2,   1,   2,   2,   2,   2,   1,   2,   2,   1},
        {2,   1,   2,   3,   2,   1,   1,   2,   1,   2,   3,   1,   1,   2,   0,   0,   1,   2,   1,   1,   2,   2,   1,   2,   0,   2,   2,   2,   1,   1,   2,   2,   2,   2,   1,   1,   1,   2,   1,   2,   2},
        {0,   3,   1,   1,   2,   1,   3,   2,   2,   1,   1,   2,   2,   1,   2,   1,   0,   0,   1,   1,   2,   0,   1,   1,   1,   1,   2,   1,   2,   1,   1,   2,   3,   1,   0,   3,   2,   1,   1,   1,   1},
        {3,   0,   1,   2,   1,   2,   1,   3,   2,   2,   1,   1,   0,   3,   1,   2,   0,   0,   1,   1,   1,   2,   1,   2,   1,   3,   3,   3,   1,   2,   2,   3,   0,   3,   3,   1,   2,   3,   2,   0,   3},
        {3,   2,   2,   3,   1,   1,   2,   1,   2,   1,   1,   1,   3,   2,   2,   1,   1,   1,   0,   0,   2,   1,   2,   1,   1,   1,   1,   1,   2,   1,   0,   3,   2,   1,   3,   1,   2,   0,   2,   2,   2},
        {1,   1,   2,   1,   1,   2,   1,   2,   1,   1,   3,   1,   2,   1,   1,   1,   1,   1,   0,   0,   2,   3,   0,   2,   2,   1,   1,   1,   1,   1,   3,   3,   1,   3,   2,   2,   1,   3,   1,   1,   1},
        {2,   2,   1,   1,   2,   0,   2,   2,   3,   2,   1,   2,   1,   1,   2,   2,   2,   2,   2,   1,   0,   0,   2,   1,   1,   1,   0,   1,   1,   2,   1,   3,   1,   1,   1,   3,   1,   1,   1,   1,   2},
        {3,   0,   2,   2,   0,   3,   1,   3,   1,   2,   3,   3,   3,   3,   2,   2,   0,   2,   1,   3,   0,   0,   0,   2,   1,   3,   3,   1,   1,   3,   3,   3,   1,   2,   3,   3,   1,   3,   1,   2,   1},
        {1,   2,   1,   1,   2,   1,   2,   1,   2,   1,   1,   2,   1,   2,   2,   1,   1,   1,   2,   1,   2,   0,   0,   0,   1,   2,   1,   1,   1,   1,   0,   3,   2,   1,   3,   3,   2,   1,   1,   2,   2},
        {1,   1,   1,   2,   1,   1,   1,   1,   1,   3,   1,   1,   0,   2,   1,   2,   1,   2,   1,   2,   0,   2,   0,   0,   1,   3,   1,   1,   1,   1,   2,   2,   2,   1,   1,   2,   1,   2,   1,   1,   1},
        {1,   2,   2,   0,   1,   2,   1,   1,   1,   1,   2,   1,   2,   0,   2,   0,   1,   1,   1,   1,   1,   1,   1,   1,   0,   0,   0,   1,   3,   1,   2,   2,   1,   1,   0,   3,   3,   3,   0,   2,   1},
        {3,   1,   1,   3,   0,   2,   3,   3,   2,   2,   2,   2,   1,   3,   2,   2,   1,   3,   1,   1,   1,   3,   2,   3,   0,   0,   3,   1,   2,   1,   3,   3,   1,   2,   1,   3,   3,   3,   1,   1,   1},
        {3,   1,   1,   2,   0,   3,   2,   2,   2,   1,   1,   0,   2,   2,   3,   2,   2,   3,   1,   1,   0,   3,   1,   1,   0,   3,   0,   0,   1,   1,   1,   2,   1,   3,   1,   1,   2,   3,   1,   2,   1},
        {2,   1,   1,   1,   2,   2,   1,   1,   1,   2,   1,   2,   0,   2,   1,   2,   1,   3,   1,   1,   1,   1,   1,   1,   1,   1,   0,   0,   0,   2,   1,   1,   1,   1,   2,   0,   1,   2,   1,   2,   1},
        {1,   2,   2,   1,   1,   1,   1,   1,   1,   1,   2,   1,   3,   1,   2,   1,   2,   1,   2,   1,   1,   1,   1,   1,   3,   2,   1,   0,   0,   0,   1,   2,   2,   1,   0,   0,   2,   1,   1,   2,   3},
        {2,   1,   2,   2,   1,   2,   1,   1,   1,   1,   1,   2,   1,   2,   2,   1,   1,   2,   1,   1,   2,   3,   1,   1,   1,   1,   1,   2,   0,   0,   2,   2,   1,   2,   3,   3,   2,   3,   1,   3,   2},
        {1,   1,   1,   3,   1,   2,   1,   1,   1,   2,   1,   1,   1,   1,   1,   2,   1,   2,   0,   3,   1,   3,   0,   2,   2,   3,   1,   1,   1,   2,   0,   0,   1,   2,   2,   2,   0,   1,   1,   0,   1},
        {2,   2,   1,   2,   1,   2,   2,   1,   2,   3,   1,   1,   1,   3,   2,   2,   2,   3,   3,   3,   3,   3,   3,   2,   2,   3,   2,   1,   2,   2,   0,   0,   1,   3,   2,   3,   2,   3,   1,   3,   2},
        {0,   2,   1,   1,   2,   1,   1,   1,   1,   2,   2,   2,   1,   1,   1,   2,   3,   0,   2,   1,   1,   1,   2,   2,   1,   1,   1,   1,   2,   1,   1,   1,   0,   0,   0,   0,   2,   0,   1,   1,   2},
        {3,   1,   1,   1,   1,   2,   1,   1,   2,   1,   2,   0,   1,   2,   2,   2,   1,   3,   1,   3,   1,   2,   1,   1,   1,   2,   3,   1,   1,   2,   2,   3,   0,   0,   3,   3,   3,   3,   2,   2,   3},
        {1,   1,   1,   1,   1,   2,   1,   3,   2,   2,   1,   2,   2,   3,   2,   1,   0,   3,   3,   2,   1,   3,   3,   1,   0,   1,   1,   2,   0,   3,   2,   2,   0,   3,   0,   3,   2,   3,   1,   1,   2},
        {1,   1,   1,   1,   1,   0,   1,   3,   2,   3,   1,   2,   1,   3,   2,   1,   3,   1,   1,   2,   3,   3,   3,   2,   3,   3,   1,   0,   0,   3,   2,   3,   0,   3,   3,   0,   3,   2,   1,   1,   1},
        {3,   2,   2,   3,   1,   2,   2,   3,   2,   1,   3,   1,   2,   3,   2,   1,   2,   2,   2,   1,   1,   1,   2,   1,   3,   3,   2,   1,   2,   2,   0,   2,   2,   3,   2,   3,   0,   3,   0,   3,   3},
        {3,   2,   3,   2,   0,   3,   1,   3,   3,   1,   3,   1,   2,   1,   1,   2,   1,   3,   0,   3,   1,   3,   1,   2,   3,   3,   3,   2,   1,   3,   1,   3,   0,   3,   3,   2,   3,   0,   3,   0,   3},
        {1,   2,   1,   1,   1,   1,   1,   1,   1,   1,   2,   1,   1,   1,   2,   1,   1,   2,   2,   1,   1,   1,   1,   1,   0,   1,   1,   1,   1,   1,   1,   1,   1,   2,   1,   1,   0,   3,   0,   1,   1},
        {1,   2,   3,   1,   1,   2,   1,   1,   1,   1,   2,   1,   3,   1,   2,   2,   1,   0,   2,   1,   1,   2,   2,   1,   2,   1,   2,   2,   2,   3,   0,   3,   1,   2,   1,   1,   3,   0,   1,   0,   0},
        {2,   1,   1,   2,   1,   1,   1,   1,   1,   1,   1,   1,   0,   2,   1,   2,   1,   3,   2,   1,   2,   1,   2,   1,   1,   1,   1,   1,   3,   2,   1,   2,   2,   3,   2,   1,   3,   3,   1,   0,   0},

    };

    List<Trait> traits_list = new List<Trait>()
    {
        new Trait(  "Ambitious",        true), // Амбициозный
        new Trait(  "Content",          true), // Приземлённый

        new Trait(  "Brave",            true), // Храбрый
        new Trait(  "Craven",           true), // Трусливый

        new Trait(  "Calm",             true), // Спокойный
        new Trait(  "Wrathful",         true), // Гневный

        new Trait(  "Chastle",          true), // Целомудренный
        new Trait(  "Lustful",          true), // Похотливый

        new Trait(  "Diligent",         true), // Усердный
        new Trait(  "Lazy",             true), // Ленивый

        new Trait(  "Generous",         true), // Общительный
        new Trait(  "Shy",              true), // Стеснительный

        new Trait(  "Gregarious",       true), // Щедрый
        new Trait(  "Greedy",           true), // Жадный

        new Trait(  "Honest",           true), // Честный
        new Trait(  "Deceitful",        true), // Лживый

        new Trait(  "Humble",           true), // Скромный
        new Trait(  "Arrogant",         true), // Высокомерный

        new Trait(  "Just",             true), // Взвешенный
        new Trait(  "Arbitrary",        true), // Взбалмошный

        new Trait(  "Patient",          true), // Терпеливый
        new Trait(  "Impatient",        true), // Нетерпеливый

        new Trait(  "Temperate",        true), // Сдержанный
        new Trait(  "Gluttonous",       true), // Прожорливый

        new Trait(  "Trusting",         true), // Доверчивый
        new Trait(  "Paranoid",         true), // Параноик

        new Trait(  "Zealous",          true), // Ревностный
        new Trait(  "Cynical",          true), // Циничный

        new Trait(  "Compassionate",    true), // Сочувствующий
        new Trait(  "Callous",          true), // Жестокий

        new Trait(  "Fickle",           true), // Переменчивый
        new Trait(  "Stubborn",         true), // Упёртый

        new Trait(  "Ordinary",         true), // Заурядный
        new Trait(  "Eccentric",        true), // Эксцентричный

        new Trait(  "Sadistic",         true), // Садист
        new Trait(  "Masochism",        true), // Мазохист

        new Trait(  "Religious",        true), // Религиозный
        new Trait(  "Fanatic",          true), // Фанатик
        new Trait(  "Aetheist",         true), // Атеист

        new Trait(  "Fair",             true), // Справедливый
        new Trait(  "Unfair",           true), // Несправедливый

    };

    #endregion

    #region [Additional methods]
    private List<int> createArr(int i)
    {
        List<int> list = new List<int>();

        for (int a = 0; a < i; a++)
        {
            list.Add(a);
        }

        return list;
    }
    private List<Trait> sortListByAff(List<Trait> traits)
    {

        for (int step = 0; step < (traits.Count() - 1); ++step)
        {

            int swapping = 0;

            for (int i = 0; i < (traits.Count() - step - 1); ++i)
            {
                if (traits[i].affection < traits[i + 1].affection)
                {
                    Trait temp = traits[i];
                    traits[i] = traits[i + 1];
                    traits[i + 1] = temp;

                    swapping = 1;
                }
            }

            if (swapping == 0)
                break;
        }

        return traits;
    }
    private bool checkTabl(int i)
    {
        foreach (Trait a in traits)
        {
            if (tabl[a.id, i] == 0)
            {
                Console.WriteLine("Trait {0} NOT compatible with trait {1}", a.title, traits_list[i].title);
                return false;
            }

            Console.WriteLine("Trait {0} compatible with trait {1}", a.title, traits_list[i].title);
        }

        Console.WriteLine("");
        return true;
    }

    #endregion

    #region [Create Character from Zero]  // TODO: Add more ways to generation
    public void create_full_random(int traits_count)
    {
        if (max_possible_traits <= traits_count)
        {
            Console.WriteLine("Not enough traits in data base to create_full_random");
        }
        else
        {
            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(traits_list.Count);
                traits.Add(traits_list[random]);
                traits_list.Remove(traits_list[random]);
            }
        }
    }

    public void create_logic_random(int traits_count) // Стоит ли удалять из traits_list и tabl? Типо это метод генерации через таблицу. Думаю стоит, но и думаю, что не стоит
    {
        if (max_possible_traits <= traits_count)
        {
            Console.WriteLine("Not enough traits in data base to create_logic_random");
        }
        else
        {
            List<int> ids = createArr(traits_list.Count);
            /*Console.WriteLine(string.Join(" ", ids));*/

            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(traits_list.Count);

                if (traits.Count == 0 || checkTabl(random) && ids.Contains(random)) // Тут || или && ?
                {
                    traits.Add(traits_list[random]);
                    traits[i].id = random;
                    ids.Remove(random);
                }
                else
                {
                    i--;
                }
            }
        }
    }

    #endregion

    #region [Create Character from Parents]
    public void create_by_two_parents_random_half(int traits_count, Character mama, Character papa, string name) // А что если черты не будут сходиться? Нужно решить этот вопросик -> генерить новую черту
    {
        if (mama.traits == null || papa.traits == null)
        {
            Console.WriteLine("Mama and/or Papa have no traits :c");
        }

        else if (max_possible_traits <= traits_count)
        {
            Console.WriteLine("Not enough traits in data base to create_by_two_parents_random_half");
        }
        else if (traits_count == (mama.traits.Count + papa.traits.Count) / 2)
        {
            create_by_two_parents_random_half(mama, papa, name);
        }

        else if (traits_count > (mama.traits.Count + papa.traits.Count) / 2)
        {
            this.name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.traits);
            combined_traits.AddRange(papa.traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (traits.Count == 0 || checkTabl(combined_traits[random].id)) // Тут || или && ?
                {
                    traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            for (int i = cmb_traits_count; i < traits_count; i++)
            {
                var random = new Random().Next(traits_list.Count);

                if (traits.Count == 0 || checkTabl(random)) // Тут || или && ?
                {
                    traits.Add(traits_list[random]);
                    traits[i].id = random;
                }
                else
                {
                    i--;
                }
            }
        }

        else if (traits_count < (mama.traits.Count + papa.traits.Count) / 2)
        {
            this.name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.traits);
            combined_traits.AddRange(papa.traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (traits.Count == 0 || checkTabl(combined_traits[random].id)) // Тут || или && ?
                {
                    traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            for (int i = cmb_traits_count - 1; i >= traits_count; i--)
            {
                traits.RemoveAt(i);
            }
        }
    }

    public void create_by_two_parents_random_half(Character mama, Character papa, string name) // А что если черты не будут сходиться? Нужно решить этот вопросик -> генерить новую черту
    {
        if (mama.traits == null || papa.traits == null)
        {
            Console.WriteLine("Mama and/or Papa have no traits :c");
        }
        else
        {
            this.name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.traits);
            combined_traits.AddRange(papa.traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (traits.Count == 0 || checkTabl(combined_traits[random].id)) // Тут || или && ?
                {
                    traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }
        }
    }

    public void create_by_two_parents_logic(int traits_count, Character mama, Character papa, string name) // Большой affection -> больше шанс передаться ребёночку
    {
        this.name = name;

        List<Trait> combined_traits = new List<Trait> { };

        combined_traits.AddRange(mama.traits);
        combined_traits.AddRange(papa.traits);

        combined_traits = sortListByAff(combined_traits);

        for (int i = 0; i < combined_traits.Count; i++)
        {
            if (traits_count != traits.Count)
            {
                Random rand = new Random();
                double random = Math.Round(rand.NextSingle(), 3);

                if (traits.Count == 0 || checkTabl(combined_traits[i].id) && random <= 0.55d) // Тут || или && ?
                {
                    traits.Add(combined_traits[i]);
                    Console.WriteLine(random);
                }

                // Генерятся не все черты, типо скипает некоторые, нужно догенерировать их как в прошлом методе
            }
        }
    }

    #endregion

    #region [Output]
    public void write_all_traits()
    {
        Console.Write($"{name} has this traits: ");
        foreach (var trait in traits)
        {
            Console.Write($"{trait.title} ");
        }
    }

    public void write_all_traits_with_aff()
    {
        Console.Write($"{name} has this traits: ");
        Console.WriteLine("\n");

        foreach (var trait in traits)
        {
            Console.Write($"{trait.title} {trait.affection} ");
            Console.WriteLine("\n");
        }
    }

    public void write_all_traits_with_description()
    {
        Console.Write($"{name} has this traits: ");

        foreach (var trait in traits)
        {
            if (trait.affection <= 0.20)
            {
                Console.Write($"a bit {trait.title} ");
            }
            else if (trait.affection <= 0.4)
            {
                Console.Write($"slightly {trait.title} ");
            }
            else if (trait.affection <= 0.6)
            {
                Console.Write($"quite {trait.title} ");
            }
            else if (trait.affection <= 0.8)
            {
                Console.Write($"extremely {trait.title} ");
            }
            else if (trait.affection <= 1)
            {
                Console.Write($"absolutely {trait.title} ");
            }
        }
    }

    #endregion

    #region [Construstors]
    public Character(string name, float affected_by_traits)
    {
        this.name = name;
        this.affected_by_traits = affected_by_traits;
    }

    public Character(string name)
    {
        this.name = name;
        Console.WriteLine("Character {0} is created", name);
    }

    public Character()
    {
        this.name = "Undefined";
    }

    #endregion

    #region [Deprecated methods]
    // Третий вариант создания персонажа через "якорь".
    // Например, 0 - отрицательный персонаж, а 1 - положительный.
    // Создаётся рандомный/введённый "якорь" от 0 до 1                                  // Ну или якорь от 0 до 3))
    // От этого создать генератор так, чтобы 1 был без плохих черт, а 0 без хороших.
    // А 0.5 мог и то, и то.
    public bool checkTablAnchor(int id, int anchor) // row 7 - BOB = i; row 3 - BRV already in traits[0]  = 2
    {
        foreach (Trait a in traits)
        {

            if (tabl[a.id, id] == 0 || tabl[a.id, id] != anchor)
            {
                Console.WriteLine("Trait {0} NOT compatible with trait {1}", a.title, traits_list[id].title);
                return false;
            }

            Console.WriteLine("Trait {0} compatible with trait {1}", a.title, traits_list[id].title);
        }

        Console.WriteLine("");
        return true;
    }

    public bool checkAnchor(int anchor, int traits_count, int id)
    {
        int cnt = 0;

        for (int i = 0; i < traits_list.Count; i++)
        {
            if (tabl[id, i] == anchor)
            {
                cnt++;
            }
        }

        if (cnt < traits_count)
        {
            Console.WriteLine("Traits was cleared because trait {0} has cnt {1}", traits_list[id].title, cnt);
            return false;
        }

        Console.WriteLine("");
        return true;
    }

    [Obsolete("create_logic_anchor_random is deprecated, please use create_logic_random instead."/*, true*/)] // Вернуть, чтобы ес шо вызывал ошибку
    public void create_logic_anchor_random(int traits_count, int anchor)
    {
        if (traits_list.Count / 4 < traits_count)
        {
            Console.WriteLine("Not enough traits in data base");
        }
        else
        {
            for (int iv = 0; iv < traits_count; iv++)
            {
                var random = new Random().Next(traits_list.Count);

                List<int> ids = createArr(traits_list.Count);
                /*Console.WriteLine(string.Join(" ", ids));*/

                if (checkAnchor(anchor, traits_count, random) && checkTablAnchor(random, anchor) && ids.Contains(random))
                {
                    traits.Add(traits_list[random]);
                    traits[iv].id = random;
                    ids.Remove(random);
                }
                else
                {
                    iv--;
                }
            }
        }
    }

    #endregion
}