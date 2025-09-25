using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;
using SliccDB.Core;
using SliccDB.Exceptions;
using SliccDB.Fluent;
using SliccDB.Serialization;



namespace DataBase
{
    public class DataBaseManager
    {
        #region MapEnteties
        // Used to map base propereties of each type of IElement.

        public class Character
        {
            public string Name { get; set; }

            public int Time { get; set; } = -1;

            public string Description { get; set; } = "Description";
        }

        public class Item
        {
            public string Name { get; set; }

            public int Time { get; set; } = -1;

            public string Description { get; set; } = "Description";
        }

        public class Event
        {
            public string Name { get; set; }

            public int Time { get; set; } = -1;

            public string Description { get; set; } = "Description";
        }

        public class Location
        {
            public string Name { get; set; }

            public int Time { get; set; } = -1;

            public string Description { get; set; } = "Description";
        }

        #endregion

        #region Fields
        /// <summary>
        /// Params of Element those represents relations between.
        /// </summary>
        private readonly string[] _exceptedProperties = ["Locations", "Location", "Events", "Characters", "Host", "Items", "Relations"];
        
        /// <summary>
        /// Подключение к базе данных.
        /// </summary>
        private readonly DatabaseConnection Connection;

        #endregion

        #region Constructor

        /// <summary>
        /// Clss constructor.
        /// </summary>
        /// <param name="filepath">Path to db file. File extension can be text.</param>
        public DataBaseManager(string filepath)
        {
            filepath ??= Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Personal),
                    "AGN.sliccdb");
            Connection = new DatabaseConnection(filepath);
        }
        #endregion

        #region CRUD    

        #region Create

        /// <summary>
        /// Store <see cref="Plot"/> instance into DB.
        /// </summary>
        /// <param name="plot">Plot instance to store.</param>
        public void StorePlot(Plot plot)
        {
            foreach (var elem in plot.Elements)
            {
                Create(elem);
            }
        }

        /// <summary>
        /// Creeate a node representing IElement instance.
        /// </summary>
        /// <param name="element">IElement to store in db.</param>
        /// <returns></returns>
        public bool Create(IElement element)
        {
            Node createdNode = null;
            if (Connection.Nodes().Properties("Name".Value(element.Name)).FirstOrDefault() is var node && node != null)
            {
                Update(element);
                createdNode = node;
            }
            else
            {
                switch (element.Type)
                {
                    case ElemType.Character:
                    {
                        createdNode = Connection.CreateNode(new Character()
                        {
                            Name = element.Name,
                            Description = element.Description,
                            Time = element.Time
                        });
                        break;
                    }
                    case ElemType.Item:
                    {
                        createdNode = Connection.CreateNode(new Item() { Name = element.Name, Description = element.Description, Time = element.Time });
                        break;
                    }
                    case ElemType.Event:
                    {
                        createdNode = Connection.CreateNode(new Event() { Name = element.Name, Description = element.Description, Time = element.Time });
                        break;
                    }
                    case ElemType.Location:
                    {
                        createdNode = Connection.CreateNode(new Location() { Name = element.Name, Description = element.Description, Time = element.Time });
                        break;
                    }
                }

            Update(element);
            }

            CreateRelations(element, createdNode);
            return true;
        }

        #endregion

        #region Read

        /// <summary>
        /// Read node.
        /// </summary>
        /// <param name="elementName">Name of element to read.</param>
        /// <param name="related">Is any relations needed</param>
        /// <returns>IElement inctance.</returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IElement Read(string elementName, bool related = true)
        {
            if (Connection.Nodes.Count == 0)
            {
                throw new Exception("DB is empty.");
            }
            var elementNode = Connection.Nodes().Properties("Name".Value(elementName)).First();

            if (elementNode is null) throw new ArgumentException("Element has not found.");

            Element element = new(
                Enum.Parse<ElemType>(elementNode.Labels.First()),
                elementName, 
                elementNode.Properties["Description"],
                time: Convert.ToInt32(elementNode.Properties["Time"]));

            FillParams(elementNode, element);

            if (related)
            {
                var relations = Connection.Relations.Where(x => x.SourceHash == elementNode.Hash).ToList();

                foreach (var rel in relations)
                {
                    var relatedNode = Connection.Nodes.Where(x => x.Hash == rel.TargetHash).First();
                    if (relatedNode != null)
                    {
                        Element relatedElement = new(
                            Enum.Parse<ElemType>(relatedNode.Labels.First()),
                            relatedNode.Properties["Name"],
                            relatedNode.Properties["Description"],
                            time: Convert.ToInt32(relatedNode.Properties["Time"]));

                        FillParams(relatedNode, relatedElement);

                        if (rel.RelationName == "Relation")
                        {
                            Binder.Bind(element, relatedElement, Convert.ToDouble(rel.Properties["Relation"]));
                        }
                        else
                        {
                            Binder.Bind(element, relatedElement);
                        }
                    }
                }
            }

            return element;
        }

        /// <summary>
        /// Returns a List of elements, sorted by type without relations
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<IElement> ReadElementsByType(ElemType type)
        {
            var nodes = GetNodesByLabel(type.ToString());

            List<IElement> elementList = new();

            foreach (var node in nodes)
            {
                elementList.Add(Read(node.Properties["Name"]));
            }

            return elementList;
        }

        /// <summary>
        /// Fill params of element from node data.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="element"></param>
        private static void FillParams(Node node, IElement element)
        {
            foreach (var prop in node.Properties.Keys)
            {
                if (prop == "Description" || prop == "Name" || prop == "Time") { continue; }
        
                var values = node.Properties[prop]
                        .Trim('[', ']').Split(',')
                        .Select(s => s.Trim()).ToList();
        
                if (values.Count == 1)
                {
                    element.Params.Add(prop, values[0]);
                }
                else
                {
                    element.Params.Add(prop, values);
                }
            }
        }

        /// <summary>
        /// Read whole plot.
        /// </summary>
        /// <returns>Filled <see cref="Plot"/> instance.</returns>
        public Plot ReadPlot()
        {
            Plot plot = new();
            foreach (var node in Connection.Nodes)
            {
                Element elem = new(Enum.Parse<ElemType>(node.Labels.First()))
                {
                    Name = node.Properties["Name"],
                    Description = node.Properties["Description"],
                    Time = Convert.ToInt32(node.Properties["Time"])
                };
                FillParams(node, elem);
                plot.Add(elem);
            }

            foreach (var rel in Connection.Relations)
            {
                var node1 = Connection.Nodes.Where(x => x.Hash == rel.SourceHash).First();
                var node2 = Connection.Nodes.Where(x => x.Hash == rel.TargetHash).First();

                if (rel.RelationName != "Relation")
                {
                    Binder.Bind(
                        plot.Elements.Where(x => x.Name == node1.Properties["Name"]).First(),
                        plot.Elements.Where(x => x.Name == node2.Properties["Name"]).First()
                        );
                }
                else
                {
                    Binder.Bind(
                        plot.Elements.Where(x => x.Name == node1.Properties["Name"]).First(),
                        plot.Elements.Where(x => x.Name == node2.Properties["Name"]).First(),
                        Convert.ToDouble(rel.Properties["Relation"])
                        );
                }
            }
            return plot;
        }

        #endregion
        
        #region Update

        /// <summary>
        /// Add or update properties of <see cref="IElement"/> node.
        /// </summary>
        /// <param name="element"><see cref="IElement"/> to update.</param>
        /// <param name="paramsName">Params to update in IElement node</param>
        public void Update(IElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            var @params = element.Params.Keys.Concat(["Description", "Time"]);

            var node = Connection.Nodes().Properties("Name".Value(element.Name)).First();

            foreach (var prop in node.Properties.Keys)
            {
                if (!new string[] { "Description", "Time", "Name"}.Contains(prop))
                {
                    node.Properties.Remove(prop);
                }
            }

            foreach (var param in @params)
            {
                if (_exceptedProperties.Contains(param)) 
                {
                    continue;
                }

                if (param == "Description")
                {
                    node.Properties[param] = element.Description;
                }
                else if (param == "Time")
                {
                    node.Properties[param] = element.Time.ToString();
                }
                else
                {
                    string paramValue;
                    if (element.Params[param] is IEnumerable<object> p)
                    {
                        paramValue = string.Join(", ", p);
                    }
                    else
                    {
                        paramValue = element.Params[param].ToString();
                    }

                    node.Properties[param] = paramValue;
                }
            }

            Connection.Update(node);
            CreateRelations(element, node);
        }

        public void Update(IElement element, string previousName)
        {
            ArgumentNullException.ThrowIfNull(element);

            var node = Connection.Nodes().Properties("Name".Value(previousName)).First();
            node.Properties["Name"] = element.Name;
            Update(element);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete the node of <see cref="IElement"/>.
        /// </summary>
        /// <param name="element">Element to delete.</param>
        /// <exception cref="Exception">Erorr on deletation.</exception>
        public void Delete(IElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            try
            {
                Connection.Delete(Connection.Nodes().Properties("Name".Value(element.Name)).FirstOrDefault());
            }
            catch (SliccDbException ex)
            {
                throw new Exception("Error on deletion:" + ex.Message);
            }
        }

        /// <summary>
        /// Delete the node of <see cref="IElement"/>.
        /// </summary>
        /// <param name="element">Element to delete.</param>
        /// <exception cref="Exception">Erorr on deletation.</exception>
        public void Delete(string elementName)
        {
            try
            {
                Connection.Delete(Connection.Nodes().Properties("Name".Value(elementName)).FirstOrDefault());
            }
            catch (SliccDbException ex)
            {
                throw new Exception("Error on deletion:" + ex.Message);
            }
        }

        /// <summary>
        /// Clears db.
        /// </summary>
        public void DeletePlot()
        {
            Connection.Entities().Clear();
        }

        #endregion

        #endregion

        #region Private methods

        /// <summary>
        /// Create relations with two nodes with additional params.
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="additionalParams"></param>
        /// <exception cref="ArgumentException"></exception>
        private void CreateRelations(Node node1, Node node2, Dictionary<string, string> additionalParams = null)
        {
            ArgumentNullException.ThrowIfNull(node1);
            ArgumentNullException.ThrowIfNull(node2);

            if (Connection.Nodes.Contains(node1) && Connection.Nodes.Contains(node2))
            {
                try
                {
                    Connection.CreateRelation(node1.Labels.First(), sn => sn.First(x => x.Hash == node2.Hash), tn => tn.First(x => x.Hash == node1.Hash), additionalParams);
                }
                catch (RelationExistsException)
                { }

                try
                {
                    Connection.CreateRelation(node2.Labels.First(), sn => sn.First(x => x.Hash == node1.Hash), tn => tn.First(x => x.Hash == node2.Hash), additionalParams);
                }
                catch (RelationExistsException)
                { }
            }
            else
            {
                throw new ArgumentException("Node is not in database.");
            }
        }

        /// <summary>
        /// Creates all relations of node with nodes, that has only Name.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="centralNode"></param>
        private void CreateRelations(IElement element, Node centralNode)
        {
            ArgumentNullException.ThrowIfNull(element);
            ArgumentNullException.ThrowIfNull(centralNode);

            foreach (var param in element.Params.Keys)
            {
                Node newNode;
                if (element.Params[param] is IEnumerable<IElement> elements)
                {
                    foreach (var obj in elements)
                    {
                        if (Connection.Nodes().Properties("Name".Value(obj.Name)).FirstOrDefault() is var node && node != null)
                        {
                            if (Connection.Relations.Where(x => x.SourceHash ==  centralNode.Hash && x.TargetHash == node.Hash).Any())
                            {
                                continue;
                            }
                            else
                            {
                                newNode = node;
                            }
                        }
                        else
                        {
                            switch (param)
                            {
                                case "Locations":
                                {
                                    newNode = Connection.CreateNode(new Location() { Name = obj.Name });
                                    break;
                                }
                                case "Events":
                                {
                                    newNode = Connection.CreateNode(new Event() { Name = obj.Name });
                                    break;
                                }
                                case "Items":
                                {
                                    newNode = Connection.CreateNode(new Item() { Name = obj.Name });
                                    break;
                                }
                                case "Characters":
                                {
                                    newNode = Connection.CreateNode(new Character() { Name = obj.Name });
                                    break;
                                }
                                default: { continue; }
                            }
                        }
                        CreateRelations(centralNode, newNode);
                    }
                }
                else if (element.Params[param] is IElement obj)
                {
                    if (Connection.Nodes().Properties("Name".Value(obj.Name)).FirstOrDefault() is var node && node != null)
                    {
                        if (Connection.Relations.Where(x => x.SourceHash == centralNode.Hash && x.TargetHash == node.Hash).Any())
                        {
                            continue;
                        }
                        else
                        {
                            newNode = node;
                        }
                    }
                    else
                    {
                        switch (param)
                        {
                            case "Host":
                            {
                                newNode = Connection.CreateNode(new Character() { Name = obj.Name });
                                break;
                            }
                            case "Location":
                            {
                                newNode = Connection.CreateNode(new Location() { Name = obj.Name });
                                break;
                            }
                            default: { continue; }
                        }
                    }
                    CreateRelations(centralNode, newNode);
                }
                else if (element.Params[param] is IEnumerable<BaseClasses.Model.Relation> rels && rels != null)
                {
                    foreach (var rel in rels)
                    {
                        var relationProps = new Dictionary<string, string>
                                    {
                                        { "Relation", rel.Value.ToString() }
                                    };

                        if (Connection.Nodes().Properties("Name".Value(rel.Character.Name)).FirstOrDefault() is var node && node != null)
                        {
                            newNode = node;
                        }
                        else
                        {
                           newNode = Connection.CreateNode(new Character() { Name = rel.Character.Name });
                        }

                        try
                        {
                            Connection.CreateRelation(param[..^1], sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash), relationProps);
                        }
                        catch (RelationExistsException) { }
                    }
                    continue;
                }
            }      
        }

        /// <summary>
        /// Get nodes that has some tag(label).
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        private IEnumerable<Node> GetNodesByLabel(string label)
        {
            return Connection.Nodes().Where(x => x.Labels.Contains(label));
        }
        #endregion
    }
}
