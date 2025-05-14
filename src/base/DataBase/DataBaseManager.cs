using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
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

        internal class Сharacter
        {
            internal string Name { get; set; }
            internal string Description { get; set; } = "";
            internal string Traits { get; set; } = "";
        }

        internal class Item
        {
            internal string Name { get; set; }
            internal string Description { get; set; } = "";
        }

        internal class Event
        {
            internal string Name { get; set; }
            internal string Description { get; set; } = "";
        }

        internal class Location
        {
            internal string Name { get; set; }
            internal string Description { get; set; } = "";
        }

        #endregion

        #region Fields

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
                    "AGN");
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
            if (Connection.Nodes().Properties("Name".Value(element.Name)).First() is var node && node != null)
            {
                FillNodeFields(element, node);
            }
            else
            {
                switch (element.Type)
                {
                    case ElemType.Character:
                    {
                        Connection.CreateNode(new Сharacter()
                        {
                            Name = element.Name,
                            Description = element.Description,
                            Traits = (string)element.Params["Traits"]
                        },"Character");
                        break;
                    }
                    case ElemType.Item:
                    {
                        Connection.CreateNode(new Item() { Name = element.Name, Description = element.Description });
                        break;
                    }
                    case ElemType.Event:
                    {
                        Connection.CreateNode(new Event() { Name = element.Name, Description = element.Description });
                        break;
                    }
                    case ElemType.Location:
                    {
                        Connection.CreateNode(new Location() { Name = element.Name, Description = element.Description });
                        break;
                    }
                }
            }
            var createdNode = Connection.Nodes().Properties("Name".Value(element.Name)).First();
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

            foreach (var prop in elementNode.Properties.Keys)
            {
                if (prop == "Description" || prop == "Name") { continue; }
                
                var values = elementNode.Properties[prop]
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

            var relations = Connection.Relations.Where(x => x.SourceHash == elementNode.Hash).ToList();

            foreach (var rel in relations)
            {
                var relatedNode = Connection.Nodes.Where(x => x.Hash == rel.TargetHash).First();
                if (relatedNode != null)
                {
                    var param = relatedNode.Labels.First();

                    if (!element.Params.TryGetValue(param, out object? value))
                    {
                        value = new List<string>();
                        element.Params.Add(param, value);
                    }

                    if (value is List<string> paramValues)
                    {
                        paramValues.Add(relatedNode.Properties["Name"]);
                    }
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
            Plot plot = new Plot();
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
            if (element == null) throw new ArgumentNullException();

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
            var node = Connection.Nodes().Properties("Name".Value(element.Name)).First();
            foreach (var param in paramsName)
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
            Connection.Update(node);
        }

        #endregion
        
        #region Delete

        /// <summary>
        /// Delete the node of <see cref="IElement"/>.
        /// </summary>
        /// <param name="element">Element to delete.</param>
        /// <exception>Erorr on deletation.</exception>
        public void Delete(IElement element)
        {
            try
            {
                Connection.Delete(Connection.Nodes().Properties("Name".Value(element.Name)).First());
            }
            catch (SliccDbException ex)
            {
                throw new Exception("Error on deletion:" + ex.Message);
            }
        }

        #endregion

        #endregion

        #region Private methods

        private bool CreateRelations(IElement element, Node centralNode)
        {
            foreach (var param in element.Params.Keys)
            {
                foreach (var obj in (List<object>)element.Params[param])
                {
                    Node newNode;
                    if (Connection.Nodes().Properties("Name".Value((string)obj)).First() is var node && node != null)
                    {
                        newNode = node;
                    }
                    else
                    {
                        try
                        {
                            switch (param)
                            {
                                case "Locations":
                                    {
                                        newNode = Connection.CreateNode(new Location() { Name = (string)obj });
                                        break;
                                    }
                                case "Events":
                                    {
                                        newNode = Connection.CreateNode(new Event() { Name = (string)obj });
                                        break;
                                    }
                                case "Items":
                                    {
                                        newNode = Connection.CreateNode(new Item() { Name = (string)obj });
                                        break;
                                    }
                                case "Host":
                                    {
                                        newNode = Connection.CreateNode(new Сharacter() { Name = (string)obj });
                                        break;
                                    }
                                case "Characters":
                                    {
                                        newNode = Connection.CreateNode(new Сharacter() { Name = (string)obj });
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
                                    var relationProps = new Dictionary<string, string>();
                                    relationProps.Add("Relation", rel.Value.ToString());
                                    newNode = Connection.CreateNode(new Сharacter() { Name = (string)obj });

                                    Connection.CreateRelation(param[..^1], sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash), relationProps);
                                }
                            }
                        }
                        catch (SliccDbException) { return false; }
                    }
                }
            }
            return true;
        }

        private void FillNodeFields(IElement element, Node node)
        {
            try
            {
                node.Properties["Description"] = element.Description;
                if (element.Type == ElemType.Character)
                {
                    node.Properties["Traits"] = (string)element.Params["Traits"];
                }

                AddOrUpdateParams(element, element.Params.Keys.Except(["Locations", "Events", "Characters", "Host", "Items", "Relation"]));
                
                Connection.Update(node);
            }
            catch (SliccDbException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
