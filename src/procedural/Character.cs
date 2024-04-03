using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Intrinsics.X86;

public class Character
{

    /// <summary>
    ///  Три Слэша - Всему Голова (ну и всё перефакторить ес, названия)
    /// </summary>

    #region [Character characteristics]

    public string name;
    public int    age;
    // TODO: Пол
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
        new Trait(  "Амбициозный",      true), // Ambitious
        new Trait(  "Приземлённый",     true), // Content

        new Trait(  "Храбрый",          true), // Brave
        new Trait(  "Трусливый",        true), // Craven

        new Trait(  "Спокойный",        true), // Calm
        new Trait(  "Гневный",          true), // Wrathful

        new Trait(  "Целомудренный",    true), // Chastle
        new Trait(  "Похотливый",       true), // Lustful

        new Trait(  "Усердный",         true), // Diligent
        new Trait(  "Ленивый",          true), // Lazy

        new Trait(  "Общительный",      true), // Generous
        new Trait(  "Стеснительный",    true), // Shy

        new Trait(  "Щедрый",           true), // Gregarious
        new Trait(  "Жадный",           true), // Greedy

        new Trait(  "Честный",          true), // Honest
        new Trait(  "Лживый",           true), // Deceitful

        new Trait(  "Скромный",         true), // Humble
        new Trait(  "Высокомерный",     true), // Arrogant

        new Trait(  "Взвешенный",       true), // Just
        new Trait(  "Взбалмошный",      true), // Arbitrary

        new Trait(  "Терпеливый",       true), // Patient
        new Trait(  "Нетерпеливый",     true), // Impatient

        new Trait(  "Сдержанный",       true), // Temperate
        new Trait(  "Прожорливый",      true), // Gluttonous

        new Trait(  "Доверчивый",       true), // Trusting
        new Trait(  "Параноик",         true), // Paranoid

        new Trait(  "Ревностный",       true), // Zealous
        new Trait(  "Циничный",         true), // Cynical

        new Trait(  "Сочувствующий",    true), // Compassionate
        new Trait(  "Жестокий",         true), // Callous

        new Trait(  "Переменчивый",     true), // Fickle
        new Trait(  "Упёртый",          true), // Stubborn

        new Trait(  "Заурядный",        true), // Ordinary
        new Trait(  "Эксцентричный",    true), // Eccentric

        new Trait(  "Садист",           true), // Sadistic
        new Trait(  "Мазохист",         true), // Masochism

        new Trait(  "Религиозный",      true), // Religious
        new Trait(  "Фанатик",          true), // Fanatic
        new Trait(  "Атеист",           true), // Aetheist

        new Trait(  "Справедливый",     true), // Fair
        new Trait(  "Несправедливый",   true), // Unfair

    };

    #endregion

    #region [Additional methods]

    /// <summary>
    /// Создаёт массив натуральных чисел от 0 до i
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private List<int> CreateArr(int i)
    {
        List<int> list = new List<int>();

        for (int a = 0; a < i; a++)
        {
            list.Add(a);
        }

        return list;
    }

    /// <summary>
    /// Сортирует лист черт от наибольшей склонности к наименьшей
    /// </summary>
    /// <param name="traits"></param>
    /// <returns></returns>
    private List<Trait> SortListByAff(List<Trait> traits)
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

    /// <summary>
    /// Проверяет, можно ли добавить черту по её айди в лист черт (совместимость)
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private bool CheckTabl(int i)
    {
        foreach (Trait a in traits)
        {
            if (tabl[a.id, i] == 0)
            {
                /*Console.WriteLine("Черта {0} НЕ совместима с чертой {1}", a.title, traits_list[i].title);*/
                return false;
            }

            /*Console.WriteLine("Черта {0} совместима с чертой {1}", a.title, traits_list[i].title);*/
        }

        /*Console.WriteLine("");*/
        return true;
    }

    #endregion

    #region [Create Character from Zero]  // TODO: Add more ways to generation

    /// <summary>
    /// Генерация черт персонажа через обычный рандом
    /// </summary>
    /// <param name="traits_count"></param>
    public void CreateByChaoticRandom(int traits_count)
    {
        if (max_possible_traits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByChaoticRandom");
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

    /// <summary>
    /// Генерация черт персонажа с помощью логики. Все черты совместимы друг с другом
    /// </summary>
    /// <param name="traits_count"></param>
    public void CreateByLogicRandom(int traits_count)
    {
        if (max_possible_traits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByLogicRandom");
        }
        else
        {
            List<int> ids = CreateArr(traits_list.Count);
            /*Console.WriteLine(string.Join(" ", ids));*/

            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(traits_list.Count);

                if (traits.Count == 0 || CheckTabl(random) && ids.Contains(random)) // Тут || или && ?
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

    /// <summary>
    /// Генерация черт персонажа с помощью двух родителей. Берётся половина рандомных черт от мамы и папы. Дополнительные черты при необходимости генерятся по логике
    /// </summary>
    /// <param name="traits_count"></param>
    /// <param name="mama"></param>
    /// <param name="papa"></param>
    /// <param name="name"></param>
    public void CreateByTwoParentsHalfRandom(int traits_count, Character mama, Character papa, string name)
    {
        if (mama.traits == null || papa.traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }

        else if (max_possible_traits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByTwoParentsHalfRandom");
        }
        else if (traits_count == (mama.traits.Count + papa.traits.Count) / 2)
        {
            CreateByTwoParentsHalf(mama, papa, name);
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

                if (traits.Count == 0 || CheckTabl(combined_traits[random].id))
                {
                    traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            for (int i = traits_list.Count(); i < traits_count; i++)
            {
                var random = new Random().Next(traits_list.Count);

                if (traits.Count == 0 || CheckTabl(random))
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

                if (traits.Count == 0 || CheckTabl(combined_traits[random].id))
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

    /// <summary>
    ///  Генерация черт персонажа с помощью двух родителей. Берётся половина рандомных черт от мамы и папы
    /// </summary>
    /// <param name="mama"></param>
    /// <param name="papa"></param>
    /// <param name="name"></param>
    public void CreateByTwoParentsHalf(Character mama, Character papa, string name)
    {
        if (mama.traits == null || papa.traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
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

                if (traits.Count == 0 || CheckTabl(combined_traits[random].id)) // Тут || или && ?
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

    /// <summary>
    ///  Генерация черт персонажа с помощью двух родителей. Берутся черты с большой склонностью. Дополнительные черты при необходимости генерятся по логике
    /// </summary>
    /// <param name="traits_count"></param>
    /// <param name="mama"></param>
    /// <param name="papa"></param>
    /// <param name="name"></param>
    public void CreateByTwoParentsLogicRandom(int traits_count, Character mama, Character papa, string name) // Разветвление добавить (конструктор)
    {
        this.name = name;

        List<Trait> combined_traits = new List<Trait> { };

        combined_traits.AddRange(mama.traits);
        combined_traits.AddRange(papa.traits);

        combined_traits = SortListByAff(combined_traits);

        for (int i = 0; i < combined_traits.Count / 2; i++)
        {
            if (traits_count != traits.Count)
            {
                Random rand = new Random();
                double random = Math.Round(rand.NextSingle(), 3);

                if (traits.Count == 0 || CheckTabl(combined_traits[i].id) && random <= 0.85d)
                {
                    traits.Add(combined_traits[i]);
                }
                else
                {
                    combined_traits.RemoveAt(i);
                    i--;
                }
            }
        }

        for (int i = traits.Count(); i < traits_count; i++)
        {
            var random = new Random().Next(traits_list.Count);

            if (CheckTabl(random))
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

    /// <summary>
    /// Генерация черт персонажа с помощью двух родителей. Берутся черты с большой склонностью
    /// </summary>
    /// <param name="mama"></param>
    /// <param name="papa"></param>
    /// <param name="name"></param>
    public void CreateByTwoParentsLogic(Character mama, Character papa, string name)
    {
        if (mama.traits == null || papa.traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }
        else
        {
            this.name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.traits);
            combined_traits.AddRange(papa.traits);

            combined_traits = SortListByAff(combined_traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                Random rand = new Random();
                double random = Math.Round(rand.NextSingle(), 3);

                if (traits.Count == 0 || CheckTabl(combined_traits[i].id) && random <= 0.85d)
                {
                    traits.Add(combined_traits[i]);
                }
                else
                {
                    combined_traits.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    #endregion

    #region [Output]
    public void WriteAllTraits()
    {
        Console.Write($"{name} имеет такие черты: ");
        foreach (var trait in traits)
        {
            Console.Write($"{trait.title} ");
        }
    }

    public void WriteAllTraitsWithAff()
    {
        Console.Write($"{name} имеет такие черты: ");
        Console.WriteLine("\n");

        foreach (var trait in traits)
        {
            Console.Write($"{trait.title} {trait.affection} ");
            Console.WriteLine("\n");
        }
    }

    public void WriteAllTraitsWithAffDesc()
    {
        Console.Write($"{name} имеет такие черты: ");

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

    #region [Construstors] // TODO: DO

    public Character(string name, float affected_by_traits)
    {
        this.name = name;
        this.affected_by_traits = affected_by_traits;
    }

    public Character(string name)
    {
        this.name = name;
    }

    public Character()
    {
        this.name = "Undefined";
    }

    #endregion

    #region [Deprecated methods]

    /// <summary>
    /// Проверяет черту по всем якорям.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="anchor"></param>
    /// <returns></returns>
    public bool CheckTablAnchor(int id, int anchor)
    {
        foreach (Trait a in traits)
        {

            if (tabl[a.id, id] == 0 || tabl[a.id, id] != anchor)
            {
                /*Console.WriteLine("Черта {0} НЕ совместима с чертой {1}", a.title, traits_list[id].title);*/
                return false;
            }

            /*Console.WriteLine("Черта {0} совместима с чертой {1}", a.title, traits_list[id].title);*/
        }

        /*Console.WriteLine("");*/
        return true;
    }

    /// <summary>
    /// Проверяет возможность создания персонажа с введённым якорем.
    /// </summary>
    /// <param name="anchor"></param>
    /// <param name="traits_count"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool CheckAnchorPossibility(int anchor, int traits_count, int id)
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
            Console.WriteLine("Черты были очищены, потому что черта {0} имеет cnt {1}", traits_list[id].title, cnt);
            return false;
        }

        Console.WriteLine("");
        return true;
    }


    /// <summary>
    /// Создание персонажа через "якорь". Якорь - число от 0 до 3. Чем якорь больше, тем персонаж будет интереснее (сомнительнее)
    /// </summary>
    /// <param name="traits_count"></param>
    /// <param name="anchor"></param>
    [Obsolete("CreateByAnchorLogic is deprecated, please use CreateByAnchorLogic instead."/*, true*/)]
    public void CreateByAnchorLogic(int traits_count, int anchor)
    {
        if (max_possible_traits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных");
        }
        else
        {
            for (int iv = 0; iv < traits_count; iv++)
            {
                var random = new Random().Next(traits_list.Count);

                List<int> ids = CreateArr(traits_list.Count);
                /*Console.WriteLine(string.Join(" ", ids));*/

                if (CheckAnchorPossibility(anchor, traits_count, random) && CheckTablAnchor(random, anchor) && ids.Contains(random))
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