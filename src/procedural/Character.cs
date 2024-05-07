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

    public int id;

    public string Name;
    public string Surname;
    public int    Age;
    public string ?Gender;
    public string ?Description;
    public string ?Location;
    private const int _MaxPossibleTraits = 10;
    private const int _MaxPossiblePhobias = 3;

    public Dictionary<int, double> Relations = new Dictionary<int, double>();
    public List<Trait> Traits = new List<Trait>();
    public List<Trait> Phobias = new List<Trait>();
    /*public List<Item> Items = new List<Item>();*/
    /*public List<Event> Events = new List<Event>;*/
    // TODO: Фобии

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
                if (traits[i].Affection < traits[i + 1].Affection)
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
        foreach (Trait a in Traits)
        {
            if (GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), i] == 0)
            {
                // Console.WriteLine("Черта {0} НЕ совместима с чертой {1}", a.Title, GlobalData.TraitsList[i].Title);
                return false;
            }

            // Console.WriteLine("Черта {0} совместима с чертой {1}", a.Title, GlobalData.TraitsList[i].Title);
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
        foreach (Trait a in GlobalData.TraitsList)
        {
            if(a.Title == name)
            {
                return a;
            }
        }
        return null;
    }

    /// <summary>
    /// Создаёт описание персонажа по его чертам
    /// </summary>
    private void CreateDesc()
    {
        foreach (var trait in Traits)
        {
            if(trait.Description != "")
            {
                string desc = trait.Description + " ";
                Description += desc;
            }
        }

        foreach (var phobia in Phobias)
        {
            if (phobia.Description != "")
            {
                string desc = phobia.Description + " ";
                Description += desc;
            }
        }

        if (Description != null)
        {
            Description = Description.Remove(Description.Length - 1, 1);
        }
    }

    /// <summary>
    /// Добавляет в словарь отношений значение, зависящее от черт оппонента
    /// </summary>
    /// <param name="a"></param>
    public void GetRelations(Character opponent)
    {
        if (opponent == null)
        {
            Console.WriteLine("NULL");
        }
        else
        {
            double finalRelation = 0;

            foreach (Trait a in Traits)
            {
                double tempRelation = 0;

                foreach (Trait b in opponent.Traits)
                {
                    tempRelation += GlobalData.relations_tabl[GlobalData.TraitsList.IndexOf(a), GlobalData.TraitsList.IndexOf(b)] * 50 * b.Affection;
                }

                tempRelation /= opponent.Traits.Count;
                finalRelation += tempRelation * a.Affection;
            }

            finalRelation /= Traits.Count;

            Relations.Add(opponent.id, Math.Round(finalRelation, 3));
        }
    }

    #endregion

    #region [Create Character Traits from Zero]  // TODO: Add more ways to generation

    /// <summary>
    /// Генерация черт персонажа через обычный рандом
    /// </summary>
    /// <param name="traits_count"></param>
    public void CreateByChaoticRandomTraits(int traits_count)
    {
        if (_MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByChaoticRandom");
        }
        else
        {
            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);
                Traits.Add(GlobalData.TraitsList[random]);
                GlobalData.TraitsList.Remove(GlobalData.TraitsList[random]);
            }
        }
    }

    /// <summary>
    /// Генерация черт персонажа с помощью логики. Все черты совместимы друг с другом
    /// </summary>
    /// <param name="traits_count"></param>
    public void CreateByLogicRandomTraits(int traits_count)
    {
        if (_MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByLogicRandom");
        }
        else
        {
            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (Traits.Count == 0 || CheckTabl(random))
                {
                    Traits.Add(GlobalData.TraitsList[random]);
                }
                else
                {
                    i--;
                }
            }
        }
    }

    #endregion

    #region [Create Character Traits from Parents]

    /// <summary>
    /// Генерация черт персонажа с помощью двух родителей. Берётся половина рандомных черт от мамы и папы. Дополнительные черты при необходимости генерятся по логике
    /// </summary>
    /// <param name="traits_count"></param>
    /// <param name="mama"></param>
    /// <param name="papa"></param>
    /// <param name="name"></param>
    public void CreateByTwoParentsHalfRandomTraits(int traits_count, Character mama, Character papa, string name)
    {
        if (_MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByTwoParentsHalfRandom");
        }
        if (mama.Traits == null || papa.Traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }
        else if (traits_count == (mama.Traits.Count + papa.Traits.Count) / 2)
        {
            CreateByTwoParentsHalfTraits(mama, papa, name);
        }

        else if (traits_count > (mama.Traits.Count + papa.Traits.Count) / 2)
        {
            this.Name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.Traits);
            combined_traits.AddRange(papa.Traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (Traits.Count == 0 || CheckTabl(GlobalData.TraitsList.IndexOf(combined_traits[random])))
                {
                    Traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            for (int i = GlobalData.TraitsList.Count(); i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (Traits.Count == 0 || CheckTabl(random))
                {
                    Traits.Add(GlobalData.TraitsList[random]);
                }
                else
                {
                    i--;
                }
            }
        }

        else if (traits_count < (mama.Traits.Count + papa.Traits.Count) / 2)
        {
            this.Name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.Traits);
            combined_traits.AddRange(papa.Traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (Traits.Count == 0 || CheckTabl(GlobalData.TraitsList.IndexOf(combined_traits[random])))
                {
                    Traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            for (int i = cmb_traits_count - 1; i >= traits_count; i--)
            {
                Traits.RemoveAt(i);
            }
        }
    }

    /// <summary>
    ///  Генерация черт персонажа с помощью двух родителей. Берётся половина рандомных черт от мамы и папы
    /// </summary>
    /// <param name="mama"></param>
    /// <param name="papa"></param>
    /// <param name="name"></param>
    public void CreateByTwoParentsHalfTraits(Character mama, Character papa, string name)
    {
        if (mama.Traits == null || papa.Traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }
        else
        {
            this.Name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.Traits);
            combined_traits.AddRange(papa.Traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (Traits.Count == 0 || CheckTabl(GlobalData.TraitsList.IndexOf(combined_traits[random])))
                {
                    Traits.Add(combined_traits[random]);
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
    public void CreateByTwoParentsLogicRandomTraits(int traits_count, Character mama, Character papa, string name) // Разветвление добавить (конструктор)
    {
        this.Name = name;

        List<Trait> combined_traits = new List<Trait> { };

        combined_traits.AddRange(mama.Traits);
        combined_traits.AddRange(papa.Traits);

        combined_traits = SortListByAff(combined_traits);

        for (int i = 0; i < combined_traits.Count / 2; i++)
        {
            if (traits_count != Traits.Count)
            {
                Random rand = new Random();
                double random = Math.Round(rand.NextSingle(), 3);

                if (Traits.Count == 0 || CheckTabl(GlobalData.TraitsList.IndexOf(combined_traits[i])) && random <= 0.85d)
                {
                    Traits.Add(combined_traits[i]);
                }
                else
                {
                    combined_traits.RemoveAt(i);
                    i--;
                }
            }
        }

        for (int i = Traits.Count(); i < traits_count; i++)
        {
            var random = new Random().Next(GlobalData.TraitsList.Count);

            if (CheckTabl(random))
            {
                Traits.Add(GlobalData.TraitsList[random]);
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
    public void CreateByTwoParentsLogicTraits(Character mama, Character papa, string name)
    {
        if (mama.Traits == null || papa.Traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }
        else
        {
            this.Name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.Traits);
            combined_traits.AddRange(papa.Traits);

            combined_traits = SortListByAff(combined_traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                Random rand = new Random();
                double random = Math.Round(rand.NextSingle(), 3);

                if (Traits.Count == 0 || CheckTabl(GlobalData.TraitsList.IndexOf(combined_traits[i])) && random <= 0.85d)
                {
                    Traits.Add(combined_traits[i]);
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

    #region [Create Character Traits from Input traits]

    /// <summary>
    /// Генерация черт персонажа с помощью одной введённой черты. Использует логику
    /// </summary>
    /// <param name="trait_name"></param>
    /// <param name="traits_count"></param>
    public void CreateByInputTrait(string trait_name, int traits_count)
    {
        if (_MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByInputTrait");
        }
        if (!GlobalData.TraitsList.Any(trait => trait.Title == trait_name))
        {
            Console.WriteLine("Данная черта не найдена в базе данных для CreateByInputTrait");
        }
        else
        {
            Trait trait = CheckTraitInList(trait_name);
            Traits.Add(trait);

            for (int i = 1; i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (Traits.Count == 0 || CheckTabl(random))
                {
                    Traits.Add(GlobalData.TraitsList[random]);
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
        if (_MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByInputTrait");
        }
        foreach (string name in traits_names)
        {
            if (!GlobalData.TraitsList.Any(trait => trait.Title == name))
            {
                Console.WriteLine("Черта {0} не найдена в базе данных для CreateByInputTraits", name);
            }
            else
            {
                Trait trait = CheckTraitInList(name);
                Traits.Add(trait);
            }
        }

        for (int i = 1; i < traits_count; i++)
        {
            var random = new Random().Next(GlobalData.TraitsList.Count);

            if (Traits.Count == 0 || CheckTabl(random))
            {
                Traits.Add(GlobalData.TraitsList[random]);
            }
            else
            {
                i--;
            }
        }
    }


    #endregion

    #region [Create Character Phobias from Zero]

    /// <summary>
    /// Генерация фобий персонажа через обычный рандом
    /// </summary>
    /// <param name="phobias_count"></param>
    public void CreateByChaoticRandomPhobias(int phobias_count)
    {
        if (_MaxPossiblePhobias < phobias_count)
        {
            Console.WriteLine("Недостаточно фобий в базе данных для CreateByChaoticRandomPhobias");
        }
        else
        {
            for (int i = 0; i < phobias_count; i++)
            {
                var random = new Random().Next(GlobalData.PhobiasList.Count);
                Phobias.Add(GlobalData.PhobiasList[random]);
                GlobalData.PhobiasList.Remove(GlobalData.PhobiasList[random]);
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
        Console.Write($"{Name} имеет такие черты: ");
        foreach (var trait in Traits)
        {
            Console.Write($"{trait.Title} ");
        }
    }

    /// <summary>
    /// Выводит все черты персонажа с параметром их влияния 
    /// </summary>
    public void WriteAllTraitsWithAff()
    {
        Console.Write($"{Name} имеет такие черты: ");
        Console.WriteLine("\n");

        foreach (var trait in Traits)
        {
            Console.Write($"{trait.Title} {trait.Affection} ");
            Console.WriteLine("\n");
        }
    }

    /// <summary>
    /// Выводит все черты персонажа с префиксными словами, определяющими уровень влияния черты
    /// </summary>
    public void WriteAllTraitsWithAffDesc()
    {
        Console.Write($"{Name} имеет такие черты: ");

        foreach (var trait in Traits)
        {
            if (trait.Affection <= 0.20)
            {
                Console.Write($"чуток {trait.Title} ");
            }
            else if (trait.Affection <= 0.4)
            {
                Console.Write($"слегка {trait.Title} ");
            }
            else if (trait.Affection <= 0.6)
            {
                Console.Write($"довольно {trait.Title} ");
            }
            else if (trait.Affection <= 0.8)
            {
                Console.Write($"максимально {trait.Title} ");
            }
            else if (trait.Affection <= 1)
            {
                Console.Write($"чрезвычайно {trait.Title} ");
            }
        }
    }

    /// <summary>
    /// Выводит все фобии персонажа
    /// </summary>
    public void WriteAllPhobias()
    {
        Console.Write($"{Name} имеет такие фобии: ");
        foreach (var phobia in Phobias)
        {
            Console.Write($"{phobia.Title} ");
        }
    }

    /// <summary>
    /// Выводит все фобии персонажа с параметром их влияния 
    /// </summary>
    public void WriteAllPhobiasWithAff()
    {
        Console.Write($"{Name} имеет такие фобии: ");
        Console.WriteLine("\n");

        foreach (var phobia in Phobias)
        {
            Console.Write($"{phobia.Title} {phobia.Affection} ");
            Console.WriteLine("\n");
        }
    }

    /// <summary>
    /// Выводит описание персонажа
    /// </summary>
    public void WriteDesc()
    {
        CreateDesc();
        Console.Write($"Персонажа {Name} можно описать так. ");
        Console.Write(Description);
    }

    #endregion

    #region [Construstors] // TODO: DO

    public Character(string name)
    {
        this.Name = name;
        this.Surname = "";
        this.id = GlobalData.CharactersCreated;

        GlobalData.CharactersCreated++;
    }

    public Character(string name, string surname)
    {
        this.Name = name;
        this.Surname = surname;
        this.id = GlobalData.CharactersCreated;

        GlobalData.CharactersCreated++;
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
        foreach (Trait a in Traits)
        {

            if (GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), id] == 0 || GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), id] != anchor)
            {
                /*Console.WriteLine("Черта {0} НЕ совместима с чертой {1}", a.title, GlobalData.traits_list[id].title);*/
                return false;
            }

            /*Console.WriteLine("Черта {0} совместима с чертой {1}", a.title, GlobalData.traits_list[id].title);*/
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

        for (int i = 0; i < GlobalData.TraitsList.Count; i++)
        {
            if (GlobalData.tabl[id, i] == anchor)
            {
                cnt++;
            }
        }

        if (cnt < traits_count)
        {
            Console.WriteLine("Черты были очищены, потому что черта {0} имеет cnt {1}", GlobalData.TraitsList[id].Title, cnt);
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
        if (_MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных");
        }
        else
        {
            for (int iv = 0; iv < traits_count; iv++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (CheckAnchorPossibility(anchor, traits_count, random) && CheckTablAnchor(random, anchor))
                {
                    Traits.Add(GlobalData.TraitsList[random]);
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