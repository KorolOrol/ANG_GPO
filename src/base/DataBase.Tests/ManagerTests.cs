using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;


/*
 * Для проверок необходимо использовать собственные Assert методы.
 * Из-за высокой степени вложенности, а также несоответсвия последовательностей записи и чтения элементов, 
 * прямое сравнение эквивалетности показывает ошибку, даже при правильной работе.
 * 
 * Методы же гарантирует однообразный порядок для элементов до записи и чтения.
 */


namespace DataBase.Tests
{
    public class DataBaseManagerTests
    {

        #region Necessary methods
        /// <summary>
        /// Method cast element's params dictionary to sortedDict and dicts inside it too.
        /// </summary>
        /// <param name="elem"></param>
        private static void Converter(IElement elem)
        {
            var sortedDict = new SortedDictionary<string, object>(elem.Params);
            elem.Params.Clear();

            foreach (var key in sortedDict.Keys)
            {
                elem.Params[key] = sortedDict[key];
            }
        }

        /// <summary>
        /// The method is necessary to compare the elements, 
        /// since Equivalent throws an error when comparing due to 
        /// the different order of reading the database elements.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="result"></param>
        public static void Assertator(IElement expected, IElement result)
        {
            Converter(expected);
            Converter(result);

            var hui = expected.FullInfo();

            Assert.Equal(expected.FullInfo(), result.FullInfo());
        }

        /// <summary>
        /// The method is necessary to compare the plots, 
        /// since Equivalent throws an error when comparing due to 
        /// the different order of reading the database elements.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="result"></param>
        public static void Assertator(Plot expected, Plot result)
        {
            foreach (var plot in new List<Plot> { expected, result })
            {
                foreach (var elem in plot.Elements)
                {
                    Converter(elem);
                }
                plot.Elements = plot.Elements.OrderBy(x => x.Name).ToList();
            }
            Assert.Equal(expected.FullInfo(), result.FullInfo());
        }

        #endregion

        public class ManagerCreateReadTests
        {

            /// <summary>
            /// Is node creation method works correctly, when node 
            /// contains only basic fields: name and description.
            /// </summary>
            [Fact]
            public void Create_NodesWithoutParams_SuccsessfulCreation()
            {
                string filepath = "CreateNodesWithotRelations.txt";
                DataBaseManager dbm = new(filepath);

                Element item = new(ElemType.Item, "Item", "ItemDescription");
                Element character = new(ElemType.Character, "Character", "CharacterDescription");
                Element location = new(ElemType.Location, "Location", "LocationDescription");
                Element @event = new(ElemType.Event, "Event", "EventDescription");

                var itemResult = dbm.Create(item);
                var characterResult = dbm.Create(character);
                var locationResult = dbm.Create(location);
                var eventResult = dbm.Create(@event);

                try
                {
                    Assert.True(itemResult);
                    Assert.True(characterResult);
                    Assert.True(locationResult);
                    Assert.True(eventResult);
                }
                finally
                {
                    File.Delete(filepath);
                }
            }

            /// <summary>
            /// Is node creation method works correctly, when node
            /// contains basic and additional custom fields.
            /// </summary>
            [Fact]
            public void Create_NodesWithParams_SuccessfullCreation()
            {
                string filepath = "CreateNodesWithParams.txt";
                DataBaseManager dbm = new(filepath);

                Element item = new(
                    ElemType.Item,
                    "Item1",
                    "ItemDescription",
                    new() { { "Height", "100" } });

                Element character = new(
                    ElemType.Character,
                    "Character", "CharacterDescription",
                    new() { { "Traits", new string[] { "Trait1", "Trait2" } } });

                var itemResult = dbm.Create(item);
                var characterResult = dbm.Create(character);

                try
                {
                    Assert.True(itemResult);
                    Assert.True(characterResult);
                }
                finally
                {
                    File.Delete(filepath);
                }
            }

            [Fact]
            public void Read_TwoCharacterNodes_SuccessfulReading()
            {
                string filepath = "ReadTwoChars.txt";
                DataBaseManager dbm = new(filepath);

                Element char1 = new(ElemType.Character, "Char1", "Char1 desc.");
                Element char2 = new(ElemType.Character, "Char2", "Char2 desc.");

                Binder.Bind(char1, char2, 20);

                Plot p = new();
                p.Add(char1);
                p.Add(char2);

                dbm.StorePlot(p);

                Plot rp = dbm.ReadPlot();

                try
                {
                    Assertator(p, rp);
                }
                finally
                {
                    File.Delete(filepath);
                }
            }

            /// <summary>
            /// Is node reading method works correctly, when nodes
            /// contain basic params and not related to each other.
            /// </summary>
            [Fact]
            public void Read_NodeWithoutParms_SuccessfulReading()
            {
                string filepath = "ReadNodesWithoutParams.txt";
                DataBaseManager dbm = new(filepath);

                Element item = new(ElemType.Item, "Item", "ItemDescription");
                Element character = new(ElemType.Character, "Character", "CharacterDescription");

                dbm.Create(item);
                dbm.Create(character);

                try
                {
                    Assertator(character, dbm.Read("Character"));
                    Assertator(item, dbm.Read("Item"));
                }
                finally
                {
                    File.Delete(filepath);
                }
            }

            /// <summary>
            /// Is node reading method works correctly, when nodes
            /// contain basic and additional custom params and not related to each other.
            /// </summary>
            [Fact]
            public void Read_NodeWithParms_SuccessfulReading()
            {
                string filepath = "ReadNodesWithParams.txt";
                DataBaseManager dbm = new(filepath);

                Element item = new(ElemType.Item, "Item", "ItemDescription", new() { { "Height", "100" } });
                Element character = new(ElemType.Character, "Character", "CharacterDescription", new() { { "Traits", new string[] { "Evil", "Old" } } });

                dbm.Create(item);
                dbm.Create(character);

                try
                {
                    Assertator(item, dbm.Read("Item"));
                    Assertator(character, dbm.Read("Character"));
                }
                finally
                {
                    File.Delete(filepath);
                }
            }

            /// <summary>
            /// Read plot with every type of relation.
            /// </summary>
            [Fact]
            public void Read_NodesWithRelation_SuccessfulReading()
            {
                string filepath = @"Read_NodesWithRelation.txt";
                DataBaseManager dbm = new(filepath);

                Element item = new(ElemType.Item, "Item", "ItemDescription");
                Element character1 = new(ElemType.Character, "Character1", "Character1Description");
                Element character2 = new(ElemType.Character, "Character2", "Character2Description");
                Element location = new(ElemType.Location, "Location", "LocationDescription");
                Element @event = new(ElemType.Event, "Event", "EventDescription");

                Binder.Bind(character1, character2, 100);
                Binder.Bind(character2, character1, 10);
                Binder.Bind(character1, item);
                Binder.Bind(character1, location);
                Binder.Bind(character1, @event);

                Binder.Bind(item, location);
                Binder.Bind(item, @event);

                Binder.Bind(location, @event);

                Plot plot = new();
                plot.Add(character1);
                plot.Add(character2);
                plot.Add(item);
                plot.Add(location);
                plot.Add(@event);

                dbm.StorePlot(plot);

                try
                {
                    Assertator(plot, dbm.ReadPlot());
                }
                finally
                {
                    File.Delete(filepath);
                }
            }

            [Fact]
            public void Read_EmptyDB_ThrowedException()
            {
                string filepath = @"Read_EmptyDB.txt";
                DataBaseManager dbm = new(filepath);

                try
                {
                    Assert.Throws<Exception>(() => dbm.Read("Character"));
                }
                finally
                {
                    File.Delete(filepath);
                }
            }

            [Fact]
            public void Read_NodeDoesntFound_ThrowedException()
            {
                string filepath = @"Read_NodeDoesntExist.txt";
                DataBaseManager dbm = new(filepath);
                Element item = new(ElemType.Item, "Item", "ItemDescription");

                try
                {
                    Assert.Throws<Exception>(() => dbm.Read("Character"));
                }
                finally
                {
                    File.Delete(filepath);
                }
            }
        }

        public class ManagerUpdateTests
        {
            public void Update_NodeWithoutParams_SuccesfullUpdating()
            {

            }
        }

        public class ManagerDeleteTests
        {

        }
    }
}
