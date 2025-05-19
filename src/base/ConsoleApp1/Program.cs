using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services;
using System.Diagnostics;


// Создаем сюжет  
var plot = new Plot();

// Персонажи  
var alchemist = new Element(ElemType.Character, "Эдгар Вердан",
    "Алхимик, когда-то служивший королю, но изгнанный за попытку создать эликсир бессмертия. Теперь скрывается, продолжая свои опыты.");

var thief = new Element(ElemType.Character, "Лира Теней",
    "Воровка с таинственным прошлым. Украла древний свиток у ведьмы, не подозревая, что он может пробудить древнее зло.");

var knight = new Element(ElemType.Character, "Сэр Галлант",
    "Рыцарь, некогда спасший королевство от демона. Теперь подозревает, что тьма возвращается, и ищет алхимика, чтобы предотвратить катастрофу.");

var witch = new Element(ElemType.Character, "Моргана Лестар",
    "Ведьма, заключившая договор с демоном. Преследует Лиру, чтобы вернуть свиток, необходимый для ритуала освобождения своего господина.");

// Предметы  
var cursedAmulet = new Element(ElemType.Item, "Проклятый амулет",
    "Создан алхимиком в попытке обмануть смерть. Дарует силу, но превращает владельца в монстра.");

var ancientScroll = new Element(ElemType.Item, "Древний свиток пробуждения",
    "Содержит заклинание, способное разорвать печати, сдерживающие Повелителя Тьмы.");

var phantomBlade = new Element(ElemType.Item, "Клинок призрачного рыцаря",
    "Меч, способный ранить даже демонов. Последний артефакт ордена Света, переданный Сэру Галланту.");

var elixirOfLife = new Element(ElemType.Item, "Эликсир вечной жизни",
    "Незавершенное творение Эдгара. Дарует бессмертие, но взамен забирает воспоминания и эмоции.");

// Места  
var forgottenTower = new Element(ElemType.Location, "Забытая башня алхимиков",
    "Здесь Эдгар проводит свои эксперименты. Говорят, в подвалах башни скрыты ужасные создания.");

var blackMarket = new Element(ElemType.Location, "Теневой базар",
    "Место, где Лира продает украденные артефакты. Сюда же ведьма направила своих прислужников.");

var cursedForest = new Element(ElemType.Location, "Лес проклятых душ",
    "Обитель Морганы. Те, кто заходит слишком глубоко, становятся частью леса... навсегда.");

var royalCastle = new Element(ElemType.Location, "Королевский замок Луменис",
    "Когда-то здесь правил мудрый король, но теперь тени сгущаются над троном.");

// События  
var eclipseRitual = new Element(ElemType.Event, "Ритуал кровавого затмения",
    "Раз в век демон может вернуться в мир, если ритуал будет совершен в час полного затмения.");

var greatHeist = new Element(ElemType.Event, "Кража королевской реликвии",
    "Лира украла свиток из хранилища, не зная, что за этим последует.");

var demonAwakening = new Element(ElemType.Event, "Пробуждение Повелителя Тьмы",
    "Если свиток будет прочитан в Забытой башне во время затмения, демон вырвется на свободу.");

var knightTournament = new Element(ElemType.Event, "Последний турнир",
    "Сэр Галлант должен победить, чтобы получить доступ к королевской библиотеке и найти способ остановить ведьму.");

// Связи между персонажами  
Binder.Bind(alchemist, knight);
Binder.Bind(thief, witch);
Binder.Bind(knight, witch);
Binder.Bind(alchemist, witch);

// Связи с предметами  
Binder.Bind(alchemist, elixirOfLife);
Binder.Bind(thief, ancientScroll);
Binder.Bind(knight, phantomBlade);
Binder.Bind(witch, cursedAmulet);

// Связи с событиями  
Binder.Bind(eclipseRitual, demonAwakening);
Binder.Bind(greatHeist, demonAwakening);
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


Serializer.Serialize(plot, "test.txt");
DataBase.DataBaseManager dataBaseManager = new(@"C:\Users\slend\Documents\Monke\GPO\src\base\ConsoleApp1\bin\Debug\net9.0\test.sliccdb");
dataBaseManager.StorePlot(plot);
var newplot = dataBaseManager.ReadPlot();
Console.WriteLine();