using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services;
using System.Diagnostics;


// Создаем сюжет  
var plot = new Plot();

// Персонажи  
var alchemist = new Element(ElemType.Character, "Эдгар Вердан",
    "Бывший придворный алхимик, изгнанный за опасные эксперименты. Теперь скрывается в Забытой башне, пытаясь искупить вину.");

var thief = new Element(ElemType.Character, "Лира Теней",
    "Молодая воровка, случайно укравшая древний свиток у ведьмы. Теперь за ней охотятся тени.");

var knight = new Element(ElemType.Character, "Сэр Галлант",
    "Легендарный рыцарь, победивший демона в прошлом. Чувствует, что тьма возвращается, и ищет способ остановить её.");

var witch = new Element(ElemType.Character, "Моргана Лестар",
    "Тёмная ведьма, служащая Повелителю Тьмы. Пытается заполучить свиток, чтобы завершить ритуал пробуждения.");

// Предметы  
var cursedAmulet = new Element(ElemType.Item, "Проклятый амулет",
    "Артефакт, созданный алхимиком. Дарует силу, но медленно превращает носителя в монстра.");

var ancientScroll = new Element(ElemType.Item, "Свиток пробуждения",
    "Древний манускрипт, содержащий заклинание, способное разорвать печати, сдерживающие демона.");

var phantomBlade = new Element(ElemType.Item, "Клинок призрачного рыцаря",
    "Меч, способный убивать нежить и демонов. Последняя надежда Сэра Галланта.");

var elixirOfLife = new Element(ElemType.Item, "Эликсир вечности",
    "Незавершённое зелье Эдгара. Дарует бессмертие, но стирает личность.");

// Места  
var forgottenTower = new Element(ElemType.Location, "Забытая башня",
    "Здесь алхимик проводит свои опыты. В подземельях башни скрыты ужасные создания.");

var blackMarket = new Element(ElemType.Location, "Чёрный рынок",
    "Место, где Лира пытается продать свиток. Здесь же орудуют агенты ведьмы.");

var cursedForest = new Element(ElemType.Location, "Лес проклятых душ",
    "Обитель Морганы. Те, кто заходит слишком далеко, становятся частью леса.");

var royalCastle = new Element(ElemType.Location, "Королевский замок",
    "Когда-то здесь правил справедливый король. Теперь тени сгущаются над троном.");

// События  
var eclipseRitual = new Element(ElemType.Event, "Ритуал кровавого затмения",
    "Во время затмения ведьма попытается пробудить демона, используя свиток.");

var greatHeist = new Element(ElemType.Event, "Кража свитка",
    "Лира украла свиток у Морганы, не зная его истинной ценности.");

var demonAwakening = new Element(ElemType.Event, "Пробуждение демона",
    "Если ритуал завершится, королевство погрузится в вечную тьму.");

var knightTournament = new Element(ElemType.Event, "Последний турнир",
    "Сэр Галлант участвует в турнире, чтобы получить доступ к королевским архивам.");

// Связи (персонажи ↔ персонажи)  
Binder.Bind(alchemist, knight, -50);
Binder.Bind(alchemist, witch, -50);
Binder.Bind(thief, witch, 0);
Binder.Bind(knight, witch, -100);

// Связи (персонажи ↔ предметы)  
Binder.Bind(alchemist, cursedAmulet);
Binder.Bind(thief, ancientScroll);
Binder.Bind(knight, phantomBlade);
Binder.Bind(witch, elixirOfLife);

// Связи (персонажи ↔ места)  
Binder.Bind(alchemist, forgottenTower);
Binder.Bind(thief, blackMarket);
Binder.Bind(witch, cursedForest);
Binder.Bind(knight, royalCastle);

// Связи (события ↔ персонажи/предметы/места)  
Binder.Bind(eclipseRitual, witch);
Binder.Bind(greatHeist, thief);
Binder.Bind(demonAwakening, ancientScroll);
Binder.Bind(knightTournament, royalCastle);

// Добавляем всё в сюжет  
plot.Add(alchemist);
plot.Add(thief);
plot.Add(knight);
plot.Add(witch);
plot.Add(cursedAmulet);
plot.Add(ancientScroll);
plot.Add(phantomBlade);
plot.Add(elixirOfLife);
plot.Add(forgottenTower);
plot.Add(blackMarket);
plot.Add(cursedForest);
plot.Add(royalCastle);
plot.Add(eclipseRitual);
plot.Add(greatHeist);
plot.Add(demonAwakening);
plot.Add(knightTournament);


Serializer.Serialize(plot, "test.json");
DataBase.DataBaseManager dataBaseManager = new(@"C:\Users\Egor\Documents\Monke\Tusur\AGN\src\base\ConsoleApp1\bin\Debug\net9.0\test2.sliccdb");
dataBaseManager.StorePlot(plot);
