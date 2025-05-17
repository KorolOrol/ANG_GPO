using DataBase;
using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;

namespace DataBase.Tests
{
    public class DataBaseManagerTests
    {
        public class ManagerCreateTests
        {
            [Fact]
            public void Create_NodesWitohutRelation_SuccsessfulCreation()
            {
                string filepath = "CreateNodesWithotRelations.sliccdb";
                DataBaseManager dbm = new(filepath);
                
                Element item = new(ElemType.Item, "Item1", "ItemDescription");
                Element character = new(ElemType.Character, "Character", "CharacterDescription");
                Element location = new(ElemType.Location, "Location", "LocationDescription");
                Element @event = new(ElemType.Event, "Event", "EventDescription");

                var itemResult = dbm.Create(item);
                var characterResult = dbm.Create(character);
                var locationResult = dbm.Create(location);
                var eventResult = dbm.Create(@event);

                Assert.True(itemResult);
                Assert.True(characterResult);
                Assert.True(locationResult);
                Assert.True(eventResult);

                File.Delete(filepath);
            }
        }

        public class ManagerReadTests
        {
            [Fact]
            public void Read_NodesWithoutRelation_SuccessfulReading()
            {
                string filepath = @"Read_NodesWithoutRelation";
                DataBaseManager dbm = new(filepath);

                Element item = new(ElemType.Item, "Item1", "ItemDescription");
                Element character = new(ElemType.Character, "Character", "CharacterDescription");
                Element location = new(ElemType.Location, "Location", "LocationDescription");
                Element @event = new(ElemType.Event, "Event", "EventDescription");

                var itemResult = dbm.Create(item);
                var characterResult = dbm.Create(character);
                var locationResult = dbm.Create(location);
                var eventResult = dbm.Create(@event);

                Assert.Equivalent(item, dbm.Read(item.Name));
                Assert.Equivalent(character, dbm.Read(character.Name));
                Assert.Equivalent(location, dbm.Read(location.Name));
                Assert.Equivalent(@event, dbm.Read(@event.Name));

                File.Delete(@filepath);
            }

            [Fact]
            public void Read_NodesWithRelation_SuccessfulReading()
            {
                string filepath = @"Read_NodesWithRelation";
                DataBaseManager dbm = new(filepath);

                Element item = new(ElemType.Item, "Item1", "ItemDescription");
                Element character = new(ElemType.Character, "Character", "CharacterDescription");
                Element location = new(ElemType.Location, "Location", "LocationDescription");
                Element @event = new(ElemType.Event, "Event", "EventDescription");

                Binder.Bind(character, item);
                Binder.Bind(@event, location);
                Binder.Bind(@event, character);
                Binder.Bind(location, character);

                dbm.Create(item);
                dbm.Create(character);
                dbm.Create(location);
                dbm.Create(@event);

                var itemResult = dbm.Read(item.Name);
                var characterResult = dbm.Read(character.Name);
                var locationResult = dbm.Read(location.Name);
                var eventResult = dbm.Read(@event.Name);

                Assert.Equivalent(itemResult, item);
                Assert.Equivalent(characterResult, character);
                Assert.Equivalent(locationResult, location);
                Assert.Equivalent(eventResult, @event);

                File.Delete(filepath);
            }
        }

        public class ManagerUpdateTests
        {

        }

        public class ManagerDeleteTests
        {

        }
    }
}
