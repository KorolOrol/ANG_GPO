using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Intrinsics.X86;

public class Character
{

    /// <summary>
    /// Три Слэша - Всему Голова
    /// </summary>

    #region [Character characteristics]

    public string name;
    public int    age;
    // TODO: Пол
    public string description;
    public string location;
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
        new Trait("Амбициозный", "Имеющий или проявляющий притязания на достижение значительных целей, больших успехов."),    // Ambitious
        new Trait("Приземлённый", "Трезво оценивающий реальную действительность, избегающий иллюзий."),   // Content
                                     
        new Trait("Храбрый", new List<string>
        {
            "Не знающий страха, решительный.",
            "Не знает страха в минуту опасности, способен преодолеть его.",
            "Легко рискующий своей жизнью ради достижения цели или спасения другого человека."


        }),        // Brave
        new Trait("Трусливый", new List<string>
        {
            "Робкий, боязливый.",
            "Склонен к боязни и панике, избегает опасных ситуаций.",
            "Боящийся даже незначительных опасностей."

        }),      // Craven
                                     
        new Trait("Спокойный"),      // Calm
        new Trait("Гневный"),        // Wrathful
                                     
        new Trait("Целомудренный"),  // Chastle
        new Trait("Похотливый"),     // Lustful
                                     
        new Trait("Усердный"),       // Diligent
        new Trait("Ленивый"),        // Lazy
                                     
        new Trait("Общительный"),    // Generous
        new Trait("Стеснительный"),  // Shy
                                     
        new Trait("Щедрый"),         // Gregarious
        new Trait("Жадный"),         // Greedy
                                     
        new Trait("Честный"),        // Honest
        new Trait("Лживый"),         // Deceitful
                                     
        new Trait("Скромный"),       // Humble
        new Trait("Высокомерный"),   // Arrogant
                                     
        new Trait("Взвешенный"),     // Just
        new Trait("Взбалмошный"),    // Arbitrary
                                     
        new Trait("Терпеливый"),     // Patient
        new Trait("Нетерпеливый"),   // Impatient
                                     
        new Trait("Сдержанный"),     // Temperate
        new Trait("Прожорливый"),    // Gluttonous
                                     
        new Trait("Доверчивый"),     // Trusting
        new Trait("Параноик"),       // Paranoid
                                     
        new Trait("Ревностный"),     // Zealous
        new Trait("Циничный"),       // Cynical
                                     
        new Trait("Сочувствующий"),  // Compassionate
        new Trait("Жестокий"),       // Callous
                                     
        new Trait("Переменчивый"),   // Fickle
        new Trait("Упёртый"),        // Stubborn
                                     
        new Trait("Заурядный"),      // Ordinary
        new Trait("Эксцентричный"),  // Eccentric
                                     
        new Trait("Садист"),         // Sadistic
        new Trait("Мазохист"),       // Masochism
                                     
        new Trait("Религиозный"),    // Religious
        new Trait("Фанатик"),        // Fanatic
        new Trait("Атеист"),         // Aetheist
                                     
        new Trait("Справедливый"),   // Fair
        new Trait("Несправедливый"), // Unfair

    };

    #endregion

    #region [Additional methods]

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
            if (tabl[traits_list.IndexOf(a), i] == 0)
            {
                Console.WriteLine("Черта {0} НЕ совместима с чертой {1}", a.title, traits_list[i].title);
                return false;
            }

            Console.WriteLine("Черта {0} совместима с чертой {1}", a.title, traits_list[i].title);
        }

        Console.WriteLine("");
        return true;
    }

    /// <summary>
    /// Проверяет имя черты в листе черт. Возвращает черту с совпадением имени
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private Trait CheckTraitInList(string name)
    {
        foreach (Trait a in traits_list)
        {
            if(a.title == name)
            {
                return a;
            }
        }
        return null;
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
            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(traits_list.Count);

                if (traits.Count == 0 || CheckTabl(random))
                {
                    traits.Add(traits_list[random]);
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
        if (max_possible_traits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByTwoParentsHalfRandom");
        }
        if (mama.traits == null || papa.traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
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

                if (traits.Count == 0 || CheckTabl(traits_list.IndexOf(combined_traits[random])))
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

                if (traits.Count == 0 || CheckTabl(traits_list.IndexOf(combined_traits[random])))
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

                if (traits.Count == 0 || CheckTabl(traits_list.IndexOf(combined_traits[random])))
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

                if (traits.Count == 0 || CheckTabl(traits_list.IndexOf(combined_traits[i])) && random <= 0.85d)
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

                if (traits.Count == 0 || CheckTabl(traits_list.IndexOf(combined_traits[i])) && random <= 0.85d)
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

    #region [Create Character from Input traits]

    /// <summary>
    /// Генерация черт персонажа с помощью одной введённой черты. Использует логику
    /// </summary>
    /// <param name="trait_name"></param>
    /// <param name="traits_count"></param>
    public void CreateByInputTrait(string trait_name, int traits_count)
    {
        if (max_possible_traits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByInputTrait");
        }
        if (!traits_list.Any(trait => trait.title == trait_name))
        {
            Console.WriteLine("Данная черта не найдена в базе данных для CreateByInputTrait");
        }
        else
        {
            Trait trait = CheckTraitInList(trait_name);
            traits.Add(trait);

            for (int i = 1; i < traits_count; i++)
            {
                var random = new Random().Next(traits_list.Count);

                if (traits.Count == 0 || CheckTabl(random))
                {
                    traits.Add(traits_list[random]);
                }
                else
                {
                    i--;
                }
            }
        }
    }

    /// <summary>
    /// Генерация черт персонажа с помощью введённого списка черт. Использует логику
    /// </summary>
    /// <param name="traits_names"></param>
    /// <param name="traits_count"></param>
    public void CreateByInputTraits(List<string> traits_names, int traits_count)
    {
        if (max_possible_traits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByInputTrait");
        }
        foreach (string name in traits_names)
        {
            if (!traits_list.Any(trait => trait.title == name))
            {
                Console.WriteLine("Черта {0} не найдена в базе данных для CreateByInputTraits", name);
            }
            else
            {
                Trait trait = CheckTraitInList(name);
                traits.Add(trait);
            }
        }

        for (int i = 1; i < traits_count; i++)
        {
            var random = new Random().Next(traits_list.Count);

            if (traits.Count == 0 || CheckTabl(random))
            {
                traits.Add(traits_list[random]);
            }
            else
            {
                i--;
            }
        }
    }


    #endregion

    #region [Output]

    /// <summary>
    /// Выводит все черты персонажа
    /// </summary>
    public void WriteAllTraits()
    {
        Console.Write($"{name} имеет такие черты: ");
        foreach (var trait in traits)
        {
            Console.Write($"{trait.title} ");
        }
    }

    /// <summary>
    /// Выводит все черты персонажа с параметром их влияния 
    /// </summary>
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

    /// <summary>
    /// Выводит все черты персонажа с префиксными словами, определяющими уровень влияния черты
    /// </summary>
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

    /// <summary>
    /// Выводит описание персонажа
    /// </summary>
    public void WriteDesc()
    {
        Console.Write($"Персонажа {name} можно описать так. ");

        foreach (var trait in traits)
        {
            if (trait.description.Count != 0)
            {
                if (trait.description.Count > 1)
                {
                    Random rand = new Random();
                    string desc = trait.description[rand.Next(trait.description.Count)];
                    Console.WriteLine($"{desc} ");
                }
                else
                {
                    Console.WriteLine($"{trait.description[0]} ");
                }
            }
            else
            {
                Console.Write($"{trait.title} {trait.affection} ");
            }


        }
    }

    #endregion

    #region [Construstors] // TODO: DO

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

            if (tabl[traits_list.IndexOf(a), id] == 0 || tabl[traits_list.IndexOf(a), id] != anchor)
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

                if (CheckAnchorPossibility(anchor, traits_count, random) && CheckTablAnchor(random, anchor))
                {
                    traits.Add(traits_list[random]);
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