using BaseClasses.Enum;
using BaseClasses.Model;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Генератор персонажей с различными стратегиями создания черт и фобий
/// </summary>
public class PrGenerator
{
    #region [Create PrCharacter Traits from Zero]

    /// <summary>
    /// Генерирует черты персонажа через полный случайный выбор без проверки совместимости
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="traits_count">Количество черт для генерации</param>
    public static void CreateByChaoticRandomTraits(PrCharacter character, int traits_count)
    {
        if (GlobalData._MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByChaoticRandom");
        }
        else
        {
            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);
                character.Traits.Add(GlobalData.TraitsList[random]);
                GlobalData.TraitsList.Remove(GlobalData.TraitsList[random]);
            }
        }
    }

    /// <summary>
    /// Генерирует черты персонажа с проверкой совместимости между чертами
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="traits_count">Количество черт для генерации</param>
    public static void CreateByLogicRandomTraits(PrCharacter character, int traits_count)
    {
        if (GlobalData._MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByLogicRandom");
        }
        else
        {
            for (int i = 0; i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (character.Traits.Count == 0 || CheckTabl(character, random))
                {
                    character.Traits.Add(GlobalData.TraitsList[random]);
                }
                else
                {
                    i--;
                }
            }
        }
    }

    #endregion

    #region [Create PrCharacter Traits from Parents]

    /// <summary>
    /// Генерирует черты персонажа от двух родителей с наследованием половины черт и дополнением при необходимости
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="traits_count">Общее количество черт для генерации</param>
    /// <param name="motherPrCharacter">Персонаж-мать для наследования черт</param>
    /// <param name="fatherPrCharacter">Персонаж-отец для наследования черт</param>
    /// <param name="name">Имя создаваемого персонажа</param>
    public static void CreateByTwoParentsHalfRandomTraits(PrCharacter character, int traits_count, PrCharacter motherPrCharacter, PrCharacter fatherPrCharacter, string name)
    {
        if (GlobalData._MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных для CreateByTwoParentsHalfRandom");
        }
        if (motherPrCharacter.Traits == null || fatherPrCharacter.Traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }
        else if (traits_count == (motherPrCharacter.Traits.Count + fatherPrCharacter.Traits.Count) / 2)
        {
            CreateByTwoParentsHalfTraits(character, motherPrCharacter, fatherPrCharacter, name);
        }

        else if (traits_count > (motherPrCharacter.Traits.Count + fatherPrCharacter.Traits.Count) / 2)
        {

            Random rand = new Random();

            character.Name = name;
            character.Surname = fatherPrCharacter.Surname;

            character.Gender = rand.NextDouble() >= 0.5;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(motherPrCharacter.Traits);
            combined_traits.AddRange(fatherPrCharacter.Traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (character.Traits.Count == 0 || CheckTabl(character, GlobalData.TraitsList.IndexOf(combined_traits[random])))
                {
                    character.Traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            for (int i = character.Traits.Count(); i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (CheckTabl(character, random))
                {
                    character.Traits.Add(GlobalData.TraitsList[random]);
                }
                else
                {
                    i--;
                }
            }
        }

        else if (traits_count < (motherPrCharacter.Traits.Count + fatherPrCharacter.Traits.Count) / 2)
        {
            character.Name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(motherPrCharacter.Traits);
            combined_traits.AddRange(fatherPrCharacter.Traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (character.Traits.Count == 0 || CheckTabl(character, GlobalData.TraitsList.IndexOf(combined_traits[random])))
                {
                    character.Traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            for (int i = cmb_traits_count - 1; i >= traits_count; i--)
            {
                character.Traits.RemoveAt(i);
            }
        }

        character.MotherID = motherPrCharacter.ID;
        character.FatherID = fatherPrCharacter.ID;
    }

    /// <summary>
    /// Генерирует черты персонажа строго как половину черт от двух родителей
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="mama">Персонаж-мать для наследования черт</param>
    /// <param name="papa">Персонаж-отец для наследования черт</param>
    /// <param name="name">Имя создаваемого персонажа</param>
    public static void CreateByTwoParentsHalfTraits(PrCharacter character, PrCharacter mama, PrCharacter papa, string name)
    {
        if (mama.Traits == null || papa.Traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }
        else
        {
            Random rand = new Random();

            character.Name = name;
            character.Surname = papa.Surname;

            character.Gender = rand.NextDouble() >= 0.5;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.Traits);
            combined_traits.AddRange(papa.Traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                var random = new Random().Next(combined_traits.Count);

                if (character.Traits.Count == 0 || CheckTabl(character, GlobalData.TraitsList.IndexOf(combined_traits[random])))
                {
                    character.Traits.Add(combined_traits[random]);
                }
                else
                {
                    i--;
                }
            }

            character.MotherID = mama.ID;
            character.FatherID = papa.ID;
        }
    }

    /// <summary>
    /// Генерирует черты персонажа от родителей с приоритетом черт с высокой склонностью и дополнением при необходимости
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="traits_count">Общее количество черт для генерации</param>
    /// <param name="mama">Персонаж-мать для наследования черт</param>
    /// <param name="papa">Персонаж-отец для наследования черт</param>
    /// <param name="name">Имя создаваемого персонажа</param>
    public static void CreateByTwoParentsLogicRandomTraits(PrCharacter character, int traits_count, PrCharacter mama, PrCharacter papa, string name)
    {
        character.Name = name;

        List<Trait> combined_traits = new List<Trait> { };

        combined_traits.AddRange(mama.Traits);
        combined_traits.AddRange(papa.Traits);

        combined_traits = SortListByAff(combined_traits);

        for (int i = 0; i < combined_traits.Count / 2; i++)
        {
            if (traits_count != character.Traits.Count)
            {
                Random rand = new Random();
                double random = Math.Round(rand.NextDouble(), 3);

                if (character.Traits.Count == 0 || CheckTabl(character, GlobalData.TraitsList.IndexOf(combined_traits[i])) && random <= 0.85d)
                {
                    character.Traits.Add(combined_traits[i]);
                }
                else
                {
                    combined_traits.RemoveAt(i);
                    i--;
                }
            }
        }

        for (int i = character.Traits.Count(); i < traits_count; i++)
        {
            var random = new Random().Next(GlobalData.TraitsList.Count);

            if (CheckTabl(character, random))
            {
                character.Traits.Add(GlobalData.TraitsList[random]);
            }
            else
            {
                i--;
            }
        }

        character.MotherID = mama.ID;
        character.FatherID = papa.ID;
    }

    /// <summary>
    /// Генерирует черты персонажа строго от родителей с приоритетом черт с высокой склонностью
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="mama">Персонаж-мать для наследования черт</param>
    /// <param name="papa">Персонаж-отец для наследования черт</param>
    /// <param name="name">Имя создаваемого персонажа</param>
    public static void CreateByTwoParentsLogicTraits(PrCharacter character, PrCharacter mama, PrCharacter papa, string name)
    {
        if (mama.Traits == null || papa.Traits == null)
        {
            Console.WriteLine("У Мамы и/или Папы нет черт");
        }
        else
        {
            character.Name = name;

            List<Trait> combined_traits = new List<Trait> { };
            combined_traits.AddRange(mama.Traits);
            combined_traits.AddRange(papa.Traits);

            combined_traits = SortListByAff(combined_traits);

            int cmb_traits_count = (combined_traits.Count) / 2;

            for (int i = 0; i < cmb_traits_count; i++)
            {
                Random rand = new Random();
                double random = Math.Round(rand.NextDouble(), 3);

                if (character.Traits.Count == 0 || CheckTabl(character, GlobalData.TraitsList.IndexOf(combined_traits[i])) && random <= 0.85d)
                {
                    character.Traits.Add(combined_traits[i]);
                }
                else
                {
                    combined_traits.RemoveAt(i);
                    i--;
                }
            }
        }

        character.MotherID = mama.ID;
        character.FatherID = papa.ID;
    }

    #endregion

    #region [Create PrCharacter Traits from Input traits]

    /// <summary>
    /// Генерирует черты персонажа на основе одной начальной черты с последующим логическим дополнением
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="trait_name">Название начальной черты</param>
    /// <param name="traits_count">Общее количество черт для генерации</param>
    public static void CreateByInputTrait(PrCharacter character, string trait_name, int traits_count)
    {
        if (GlobalData._MaxPossibleTraits < traits_count)
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
            character.Traits.Add(trait);

            for (int i = 1; i < traits_count; i++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (character.Traits.Count == 0 || CheckTabl(character, random))
                {
                    character.Traits.Add(GlobalData.TraitsList[random]);
                }
                else
                {
                    i--;
                }
            }
        }
    }

    /// <summary>
    /// Генерирует черты персонажа на основе списка начальных черт с последующим логическим дополнением
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="traits_names">Список названий начальных черт</param>
    /// <param name="traits_count">Общее количество черт для генерации</param>
    public static void CreateByInputTraits(PrCharacter character, List<string> traits_names, int traits_count)
    {
        if (GlobalData._MaxPossibleTraits < traits_count)
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
                character.Traits.Add(trait);
            }
        }

        for (int i = 1; i < traits_count; i++)
        {
            var random = new Random().Next(GlobalData.TraitsList.Count);

            if (character.Traits.Count == 0 || CheckTabl(character, random))
            {
                character.Traits.Add(GlobalData.TraitsList[random]);
            }
            else
            {
                i--;
            }
        }
    }

    #endregion

    #region [Create PrCharacter Phobias from Zero]

    /// <summary>
    /// Генерирует фобии персонажа через полный случайный выбор
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления фобий</param>
    /// <param name="phobias_count">Количество фобий для генерации</param>
    public static void CreateByChaoticRandomPhobias(PrCharacter character, int phobias_count)
    {
        if (GlobalData._MaxPossiblePhobias < phobias_count)
        {
            Console.WriteLine("Недостаточно фобий в базе данных для CreateByChaoticRandomPhobias");
        }
        else
        {
            for (int i = 0; i < phobias_count; i++)
            {
                var random = new Random().Next(GlobalData.PhobiasList.Count);
                character.Phobias.Add(GlobalData.PhobiasList[random]);
                GlobalData.PhobiasList.Remove(GlobalData.PhobiasList[random]);
            }
        }
    }

    #endregion

    #region [Deprecated methods]

    /// <summary>
    /// Проверяет совместимость черты по указанному якорю со всеми текущими чертами персонажа
    /// </summary>
    /// <param name="character">Персонаж для проверки совместимости</param>
    /// <param name="id">Идентификатор проверяемой черты</param>
    /// <param name="anchor">Значение якоря для проверки</param>
    /// <returns>True если черта совместима по якорю</returns>
    [Obsolete("Метод устарел, используйте CheckTabl")]
    public static bool CheckTablAnchor(PrCharacter character, int id, int anchor)
    {
        foreach (Trait a in character.Traits)
        {

            if (GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), id] == 0 || GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), id] != anchor)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Проверяет возможность создания персонажа с указанным якорем и количеством черт
    /// </summary>
    /// <param name="anchor">Значение якоря для проверки</param>
    /// <param name="traits_count">Требуемое количество черт</param>
    /// <param name="id">Идентификатор черты для проверки</param>
    /// <returns>True если создание возможно</returns>
    [Obsolete("Метод устарел")]
    public static bool CheckAnchorPossibility(int anchor, int traits_count, int id)
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
    /// Создает персонажа через систему якорей (устаревший метод)
    /// </summary>
    /// <param name="character">Целевой персонаж для добавления черт</param>
    /// <param name="traits_count">Количество черт для генерации</param>
    /// <param name="anchor">Значение якоря для фильтрации черт</param>
    [Obsolete("CreateByAnchorLogic is deprecated, please use CreateByLogicRandomTraits instead.")]
    public static void CreateByAnchorLogic(PrCharacter character, int traits_count, int anchor)
    {
        if (GlobalData._MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных");
        }
        else
        {
            for (int iv = 0; iv < traits_count; iv++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (CheckAnchorPossibility(anchor, traits_count, random) && CheckTablAnchor(character, random, anchor))
                {
                    character.Traits.Add(GlobalData.TraitsList[random]);
                }
                else
                {
                    iv--;
                }
            }
        }
    }

    #endregion

    #region [Additional methods]

    /// <summary>
    /// Сортирует список черт по убыванию значения склонности (Affection)
    /// </summary>
    /// <param name="traits">Список черт для сортировки</param>
    /// <returns>Отсортированный список черт</returns>
    private static List<Trait> SortListByAff(List<Trait> traits)
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
    /// Проверяет совместимость черты со всеми текущими чертами персонажа
    /// </summary>
    /// <param name="character">Персонаж для проверки совместимости</param>
    /// <param name="i">Идентификатор проверяемой черты</param>
    /// <returns>True если черта совместима со всеми текущими чертами</returns>
    private static bool CheckTabl(PrCharacter character, int i)
    {
        foreach (Trait a in character.Traits)
        {
            if (GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), i] == 0)
            {
                return false;
            }
        }

        Console.WriteLine("");
        return true;
    }

    /// <summary>
    /// Находит черту по названию в глобальном списке черт
    /// </summary>
    /// <param name="name">Название искомой черты</param>
    /// <returns>Найденная черта или null если не найдена</returns>
    private static Trait CheckTraitInList(string name)
    {
        foreach (Trait a in GlobalData.TraitsList)
        {
            if (a.Title == name)
            {
                return a;
            }
        }
        return null;
    }

    /// <summary>
    /// Создает описание персонажа на основе его черт и фобий
    /// </summary>
    /// <param name="character">Персонаж для создания описания</param>
    public static void CreateDesc(PrCharacter character)
    {
        foreach (var trait in character.Traits)
        {
            if (trait.Description != "")
            {
                string desc = trait.Description + " ";
                character.Description += desc;
            }
        }

        foreach (var phobia in character.Phobias)
        {
            if (phobia.Description != "")
            {
                string desc = phobia.Description + " ";
                character.Description += desc;
            }
        }

        if (character.Description != null)
        {
            character.Description = character.Description.Remove(character.Description.Length - 1, 1);
        }
    }

    /// <summary>
    /// Рассчитывает отношение персонажа к оппоненту на основе их черт
    /// </summary>
    /// <param name="character">Основной персонаж</param>
    /// <param name="opponent">Персонаж-оппонент для расчета отношений</param>
    public static void GetRelations(PrCharacter character, PrCharacter opponent)
    {
        if (opponent == null)
        {
            Console.WriteLine("NULL");
        }
        else
        {
            double finalRelation = 0;

            foreach (Trait a in character.Traits)
            {
                double tempRelation = 0;

                foreach (Trait b in opponent.Traits)
                {
                    tempRelation += GlobalData.relations_tabl[GlobalData.TraitsList.IndexOf(a), GlobalData.TraitsList.IndexOf(b)] * 50 * b.Affection;
                }

                tempRelation /= opponent.Traits.Count;
                finalRelation += tempRelation * a.Affection;
            }

            finalRelation /= character.Traits.Count;

            character.Relations.Add(opponent.ID, Math.Round(finalRelation, 3));
        }
    }

    /// <summary>
    /// Возвращает список черт характера персонажа в формате List<string>, используя affection
    /// </summary>
    /// <param name="character">PrCharacter</param>
    /// <returns>List<string> of Traits</returns>
    public static List<string> GetAllTraitsWithAffDesc(List<Trait> traits)
    {

        List<string> listWithAff = new List<string>();

        foreach (var trait in traits)
        {
            if (trait.Affection <= 0.20)
            {
                listWithAff.Add($"чуток {trait.Title}");
            }
            else if (trait.Affection <= 0.4)
            {
                listWithAff.Add($"слегка {trait.Title}");
            }
            else if (trait.Affection <= 0.6)
            {
                listWithAff.Add($"довольно {trait.Title}");
            }
            else if (trait.Affection <= 0.8)
            {
                listWithAff.Add($"максимально {trait.Title}");
            }
            else if (trait.Affection <= 1)
            {
                listWithAff.Add($"чрезвычайно {trait.Title}");
            }
        }

        return listWithAff;
    }

    /// <summary>
    /// Возвращает список фобий персонажа в формате List<string>, используя affection
    /// </summary>
    /// <param name="character">PrCharacter</param>
    /// <returns>List<string> of Phobias</returns>
    public static List<string> GetAllPhobiasWithAffDesc(List<Trait> phobias)
    {

        List<string> listWithAff = new List<string>();

        foreach (var phobia in phobias)
        {
            if (phobia.Affection <= 0.20)
            {
                listWithAff.Add($"минимально слабая {phobia.Title}");
            }
            else if (phobia.Affection <= 0.4)
            {
                listWithAff.Add($"слегка слабая {phobia.Title}");
            }
            else if (phobia.Affection <= 0.6)
            {
                listWithAff.Add($"в меру {phobia.Title}");
            }
            else if (phobia.Affection <= 0.8)
            {
                listWithAff.Add($"максимально сильная {phobia.Title}");
            }
            else if (phobia.Affection <= 1)
            {
                listWithAff.Add($"чрезвычайно сильная {phobia.Title}");
            }
        }

        return listWithAff;

    }

    /// <summary>
    /// Возвращает список отношений персонажа к другим
    /// </summary>
    /// <param name="character">PrCharacter</param>
    /// <returns>List<string> of Relations</returns>
    public static List<string> GetAllRelationsWithDesc(PrCharacter character)
    {

        List<string> listWithAff = new List<string>();

        foreach (var relation in character.Relations)
        {
            PrCharacter opponent = GlobalData.Characters[relation.Key];

            if (relation.Value <= -5.0)
            {
                listWithAff.Add($"Terrible attitude towards {opponent.Name}");
            }
            else if (relation.Value <= -3.0)
            {
                listWithAff.Add($"Bad attitude towards {opponent.Name}");
            }
            else if (relation.Value <= -1.0)
            {
                listWithAff.Add($"Abnormal attitude towards {opponent.Name}");
            }
            else if (relation.Value >= 5.0)
            {
                listWithAff.Add($"Excellent attitude towards {opponent.Name}");
            }
            else if (relation.Value >= 3.0)
            {
                listWithAff.Add($"Good attitude towards {opponent.Name}");
            }
            else if (relation.Value >= 1.0)
            {
                listWithAff.Add($"Normal attitude towards {opponent.Name}");
            }
            else
            {
                listWithAff.Add($"Neutral attitude towards {opponent.Name}");
            }
        }

        return listWithAff;
    }

    #endregion

    #region [Translate To BaseClass]

    /// <summary>
    /// Преобразует PrCharacter в базовый класс Element
    /// </summary>
    /// <param name="character">Исходный персонаж для преобразования</param>
    /// <returns>Элемент базового класса с данными персонажа</returns>
    public static Element Translate(PrCharacter character)
    {
        string baseName = "";
        string description = "";

        if (character.Name != null) { baseName = character.Name; }

        if (character.Surname != null) { baseName += " " + character.Surname; }

        if (character.Description != null) { description = character.Description; }

        Dictionary<string, object> baseParams = new Dictionary<string, object>();

        List<string> traits = new List<string>();
        List<string> phobias = new List<string>();
        List<string> relations = new List<string>();

        traits = GetAllTraitsWithAffDesc(character.Traits);
        phobias = GetAllPhobiasWithAffDesc(character.Phobias);
        relations = GetAllRelationsWithDesc(character);

        baseParams.Add("Traits", traits);
        baseParams.Add("Phobias", phobias);
        baseParams.Add("Relations", relations);

        if (character.Age != null)
        {
            baseParams.Add("Age", character.Age.ToString());
        }

        if (character.Gender != null)
        {
            baseParams.Add("Gender", (bool)character.Gender ? "Мужской" : "Женский");
        }


        Element baseCharacter = new Element(ElemType.Character, baseName, description, baseParams);

        return baseCharacter;
    }

    #endregion
}