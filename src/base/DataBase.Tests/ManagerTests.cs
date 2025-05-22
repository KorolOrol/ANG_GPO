using DataBase;
using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseClasses.Interface;
using System.Security.Cryptography.X509Certificates;

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

                dbm.Create(character);
                dbm.Create(item);
                dbm.Create(location);
                dbm.Create(@event);

                Assert.Equivalent(item, dbm.Read(item.Name));
                Assert.Equivalent(character, dbm.Read(character.Name));
                Assert.Equivalent(location, dbm.Read(location.Name));
                Assert.Equivalent(@event, dbm.Read(@event.Name));

                File.Delete(@filepath);
            }

            [Fact]
            public void Read_NodesWithRelation_SuccessfulReading()
            {
                string filepath = @"Read_NodesWithRelation.sliccdb";
                DataBaseManager dbm = new(filepath);

                Element item = new(ElemType.Item, "Item", "ItemDescription");
                Element character = new(ElemType.Character, "Character", "CharacterDescription");
                Element location = new(ElemType.Location, "Location", "LocationDescription");
                Element @event = new(ElemType.Event, "Event", "EventDescription");

                Binder.Bind(character, item);
                Binder.Bind(character, location);
                Binder.Bind(character, @event);
                Binder.Bind(location, item);

                Plot plot = new();
                plot.Add(item);
                plot.Add(character);
                plot.Add(location);
                plot.Add(@event);

                dbm.StorePlot(plot);

                var resultPlot = dbm.ReadPlot();

                Assert.Equal(JsonSerializer.Serialize(plot, Serializer.Options), JsonSerializer.Serialize(resultPlot, Serializer.Options));

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
