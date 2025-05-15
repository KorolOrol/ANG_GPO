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

        private readonly string[] _exceptedProperties = ["Locations", "Location", "Events", "Characters", "Host", "Items", "Relation"];
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
                AddOrUpdateParams(element, element.Params.Keys);
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
            }
            AddOrUpdateParams(element, element.Params.Keys);

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
            // Fill params of element from node data.
            static void FillParams(Node node, Element element)
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
        /// Read whole plot.
        /// </summary>
        /// <returns>Filled <see cref="Plot"/> instance.</returns>
        public Plot ReadPlot()
        {
            Plot plot = new();
            foreach (var node in Connection.Nodes)
            {
                plot.Add(Read(node.Properties["Name"]));
            }
            return plot;
        }

        #endregion
        
        #region Update

        /// <summary>
        /// Update Description of IElement node.
        /// </summary>
        /// <param name="element">IElement to update.</param>
        /// <exception cref="ArgumentNullException">IElement is null.</exception>
        public void UpdateDescription(IElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            var node = Connection.Nodes().Properties("Name".Value(element.Name)).First();

            if (node == null) { Create(element); }
            else
            {
                node.Properties["Description"] = element.Description;
            }

            Connection.Update(node);
        }

        /// <summary>
        /// Add or update properties of <see cref="IElement"/> node.
        /// </summary>
        /// <param name="element"><see cref="IElement"/> to update.</param>
        /// <param name="paramsName">Params to update in IElement node</param>
        public void AddOrUpdateParams(IElement element, IEnumerable<string> paramsName)
        {
            ArgumentNullException.ThrowIfNull(element);

            var node = Connection.Nodes().Properties("Name".Value(element.Name)).First();
            foreach (var param in paramsName)
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
                        node.Properties.Add(param, (string)element.Params[param]);
                    }
                    else
                    {
                        node.Properties[param] = (string)element.Params[param];
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
            foreach (var node in Connection.Nodes)
            {
                Delete(node.Properties["Name"]);
            }
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
                            if (param != "Relations")
                            {
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
                                if (obj is BaseClasses.Model.Relation rel && rel != null)
                                {
                                    var relationProps = new Dictionary<string, string>
                                    {
                                        { "Relation", rel.Value.ToString() }
                                    };
                                    newNode = Connection.CreateNode(new Character() { Name = obj.Name });

                                    Connection.CreateRelation(param[..^1], sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash), relationProps);
                                }
                            }
                        }
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
            }      
        }
        
        #endregion
    }
}
