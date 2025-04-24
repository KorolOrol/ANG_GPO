using SliccDB.Serialization;
using BaseClasses.Interface;
using SliccDB.Core;
using BaseClasses.Enum;
using SliccDB.Fluent;
using SliccDB.Exceptions;
using BaseClasses.Model;


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
                            Name = RemoveTag(element.Name),
                            Description = element.Description,
                            Traits = (string)element.Params["Traits"]
                        });
                        break;
                    }
                    case ElemType.Item:
                    {
                        Connection.CreateNode(new Item() { Name = RemoveTag(element.Name), Description = element.Description });
                        break;
                    }
                    case ElemType.Event:
                    {
                        Connection.CreateNode(new Event() { Name = RemoveTag(element.Name), Description = element.Description });
                        break;
                    }
                    case ElemType.Location:
                    {
                        Connection.CreateNode(new Location() { Name = RemoveTag(element.Name), Description = element.Description });
                        break;
                    }
                }
            }
            var createdNode = Connection.Nodes().Properties("Name".Value(RemoveTag(element.Name))).First();
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
            throw new NotImplementedException();

            Node element = Connection.Nodes().Properties("Name".Value(elementName)).First();
            ElemType type = Enum.Parse<ElemType>(element.Labels.First());

            var relations = Connection.Relations.Where(x => x.SourceHash == element.Hash).ToList();

            
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
                    var clearName = RemoveTag(name);
                    Node newNode;
                    try
                    {
                        switch (param)
                        {
                        case "Locations":
                            {
                                newNode = Connection.CreateNode(new Event() { Name = clearName });
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
                                newNode = Connection.CreateNode(new Item() { Name = clearName });
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

                        Connection.CreateRelation(param.Remove(param.Length - 2), sn => sn.First(x => x.Hash == centralNode.Hash), tn => tn.First(x => x.Hash == newNode.Hash));
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

        private static string RemoveTag(string param)
        {
            return param.Remove(param.IndexOf(": "));
        }

        #endregion
    }
}
