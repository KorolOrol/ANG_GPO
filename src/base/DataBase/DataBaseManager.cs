using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using SliccDB.Core;
using SliccDB.Exceptions;
using SliccDB.Fluent;
using SliccDB.Serialization;
using BaseClasses.Services;
using System.Xml.Linq;


namespace DataBase
{
    public class DataBaseManager
    {
        #region MapEnteties
        // Used to map base propereties of each type of IElement.

        public class Character
        {
            public string Name { get; set; }
            public string Description { get; set; } = "Description";
        }

        public class Item
        {
            public string Name { get; set; }
            public string Description { get; set; } = "Description";
        }

        public class Event
        {
            public string Name { get; set; }
            public string Description { get; set; } = "Description";
        }

        public class Location
        {
            public string Name { get; set; }
            public string Description { get; set; } = "Description";
        }

        #endregion

        #region Fields

        private readonly string[] _exceptedProperties = ["Locations", "Location", "Events", "Characters", "Host", "Items", "Relations"];
        /// <summary>
        /// Подключение к базе данных.
        /// </summary>
        private readonly DatabaseConnection Connection;

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="filepath">Путь к файлу базы данных. Расширение файла </param>
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
                        });
                        break;
                    }
                    case ElemType.Item:
                    {
                        createdNode = Connection.CreateNode(new Item() { Name = element.Name, Description = element.Description });
                        break;
                    }
                    case ElemType.Event:
                    {
                        createdNode = Connection.CreateNode(new Event() { Name = element.Name, Description = element.Description });
                        break;
                    }
                    case ElemType.Location:
                    {
                        createdNode =Connection.CreateNode(new Location() { Name = element.Name, Description = element.Description });
                        break;
                    }
                    default: { break;}
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
        /// <returns>IElement inctance.</returns>
        public IElement Read(string elementName)
        {
            

            var elementNode = Connection.Nodes().Properties("Name".Value(elementName)).First();
            Element element = new(Enum.Parse<ElemType>(elementNode.Labels.First()), elementName, elementNode.Properties["Description"]);
            FillParams(elementNode, element);

            var relations = Connection.Relations.Where(x => x.SourceHash == elementNode.Hash).ToList();

            foreach (var rel in relations)
            {
                var relatedNode = Connection.Nodes.Where(x => x.Hash == rel.TargetHash).First();
                if (relatedNode != null)
                {
                    Element relatedElement = new(
                        Enum.Parse<ElemType>(relatedNode.Labels.First()), 
                        relatedNode.Properties["Name"],
                        relatedNode.Properties["Description"]);

                    FillParams(relatedNode, relatedElement);
                    Binder.Bind(element, relatedElement);
                }
            }

            return element;
        }

        /// <summary>
        /// Fill params of element from node data.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="element"></param>
        private static void FillParams(Node node, Element element)
        {
            foreach (var prop in node.Properties.Keys)
            {
                if (prop == "Description" || prop == "Name") { continue; }
        
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
                Element elem = new(Enum.Parse<ElemType>(node.Labels.First()));
                elem.Name = node.Properties["Name"];
                elem.Description = node.Properties["Description"];
                FillParams(node, elem);
                plot.Add(elem);
            }

            foreach (var rel in Connection.Relations)
            {
                if (rel.RelationName != "Relation")
                {
                    var node1 = Connection.Nodes.Where(x => x.Hash == rel.SourceHash).First();
                    var node2 = Connection.Nodes.Where(x => x.Hash == rel.TargetHash).First();
                    Binder.Bind(
                        plot.Elements.Where(x => x.Name == node1.Properties["Name"]).First(),
                        plot.Elements.Where(x => x.Name == node2.Properties["Name"]).First()
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

            var @params = element.Params.Keys.Concat(["Description", "Name"]);

            var node = Connection.Nodes().Properties("Name".Value(element.Name)).First();
            foreach (var param in @params.Concat(["Description", "Name"]))
            {
                if (_exceptedProperties.Contains(param)) 
                {
                    continue;
                }

                if (param == "Description")
                {
                    node.Properties[param] = element.Description;
                }
                else if (param == "Name")
                {
                    node.Properties[param] = element.Name;
                }
                else
                {
                    if (!node.Properties.ContainsKey(param))
                    {
                        node.Properties.Add(param, element.Params[param].ToString());
                    }
                    else
                    {
                        node.Properties[param] = element.Params[param].ToString();
                    }
                }
            }
            Connection.Update(node);
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
                            newNode = node;
                            try
                            {
                                Connection.CreateRelation(newNode.Labels.First(), sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash));
                            }
                            catch (RelationExistsException)
                            { }

                            try
                            {
                                Connection.CreateRelation(centralNode.Labels.First(), sn => sn.First(x => x.Hash == newNode.Hash), tn => tn.First(x => x.Hash == centralNode.Hash));
                            }
                            catch (RelationExistsException)
                            { }
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

                        try
                        {
                            Connection.CreateRelation(newNode.Labels.First(), sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash));
                        }
                        catch (RelationExistsException)
                        { }

                        try
                        {
                            Connection.CreateRelation(centralNode.Labels.First(), sn => sn.First(x => x.Hash == newNode.Hash), tn => tn.First(x => x.Hash == centralNode.Hash));
                        }
                        catch (RelationExistsException)
                        { }
                    }
                }
                else if (element.Params[param] is IElement obj)
                {
                    if (Connection.Nodes().Properties("Name".Value(obj.Name)).FirstOrDefault() is var node && node != null)
                    {
                        newNode = node;
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

                    try
                    {
                        Connection.CreateRelation(newNode.Labels.First(), sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash));
                    }
                    catch (RelationExistsException)
                    { }

                    try
                    {
                        Connection.CreateRelation(centralNode.Labels.First(), sn => sn.First(x => x.Hash == newNode.Hash), tn => tn.First(x => x.Hash == centralNode.Hash));
                    }
                    catch (RelationExistsException)
                    { }
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
                }
            }      
        }
        
        #endregion
    }
}
