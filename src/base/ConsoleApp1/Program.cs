using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services;


var plot = new Plot();
var character = new Element(ElemType.Character, "Name1", "Description1");
var item = new Element(ElemType.Item, "Name2", "Description2");
var location = new Element(ElemType.Location, "Name3", "Description3");
var @event = new Element(ElemType.Event, "Name4", "Description4");
var event2 = new Element(ElemType.Event, "Name5", "Description5");
Binder.Bind(character, item);
Binder.Bind(character, location);
Binder.Bind(character, @event);
Binder.Bind(character, event2);
Binder.Bind(item, location);
Binder.Bind(item, @event);
Binder.Bind(location, @event);
Binder.Bind(location, @event2);
plot.Add(character);
plot.Add(item);
plot.Add(location);
plot.Add(@event);
plot.Add(event2);

Serializer.Serialize(plot, "test.txt");
DataBase.DataBaseManager dataBaseManager = new(null);
dataBaseManager.StorePlot(plot);
var newplot = dataBaseManager.ReadPlot();

List<Plot> plots = new() { plot, newplot };

foreach (Plot plo in plots)
{
    Console.WriteLine("\n\n" + plo.FullInfo());
}
