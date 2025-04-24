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

        private readonly DatabaseConnection Connection;

        #endregion

        #region Constructor
        DataBaseManager(string filepath)
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
        public bool Create(IElement element)
        {
            if (Connection.Nodes().Properties("Name".Value(element.Name)).First() is var node && node != null)
            {
                FillNodeFields(element, node);
                return true;
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
                        });
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

        public bool Create(List<IElement> elements) 
        {
            foreach (var element in elements)
            {
                if (!Create(element))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
            
        #region Read

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
        #endregion
        
        #region Update
        public bool Update(IElement element)
        {
            throw new NotImplementedException();
        }

        public bool Update(List<IElement> elements)
        {
            throw new NotImplementedException();
        }
        #endregion
        
        #region Delete
        public bool Delete(IElement element)
        {
            throw new NotImplementedException();
        }

        public bool Delete(List<IElement> elements)
        {
            throw new NotImplementedException();
        }
        #endregion

        #endregion

        #region Private methods

        private bool CreateRelations(IElement element, Node centralNode)
        {
            foreach (var param in element.Params.Keys)
            {
                foreach (string name in (List<string>)element.Params[param])
                {
                    var clearName = name;
                    Node newNode;
                    try
                    {
                        switch (param)
                        {
                        case "Locations":
                            {
                                newNode = Connection.CreateNode(new Location() { Name = clearName });
                                break;
                            }
                        case "Events":
                            {
                                newNode = Connection.CreateNode(new Event() { Name = clearName });
                                break;
                            }
                        case "Items":
                            {
                                newNode = Connection.CreateNode(new Item() { Name = clearName });
                                break;
                            }
                        case "Host":
                            {
                                newNode = Connection.CreateNode(new Сharacter() { Name = clearName });
                                break;
                            }
                        case "Relations":
                            {
                                newNode = Connection.CreateNode(new Сharacter() { Name = clearName });
                                break;
                            }
                        case "Characters":
                            {
                                newNode = Connection.CreateNode(new Сharacter() { Name = clearName });
                                break;
                            }
                        default: { continue; }
                        }

                        if (param != "Relations")
                        {
                            Connection.CreateRelation(newNode.Labels.First(), sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash));
                            Connection.CreateRelation(centralNode.Labels.First(), sn => sn.First(x => x.Hash == newNode.Hash), tn => tn.First(x => x.Hash == centralNode.Hash));

                        }
                        else
                        {
                            //TODO
                        }

                    }
                    catch (SliccDbException) { return false; }
                }
            }
            return true;
        }

        private bool FillNodeFields(IElement element, Node node)
        {
            try
            {
                node.Properties["Description"] = element.Description;
                if (element.Type == ElemType.Character)
                {
                    node.Properties["Traits"] = (string)element.Params["Traits"];
                }
                Connection.Update(node);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
