using SliccDB.Serialization;
using BaseClasses.Interface;
using SliccDB.Core;
using SliccDB;


namespace DataBase
{
    public class DataBaseManager
    {
        #region Classes for db mapping
        private class Character
        {

        }

        private class Item
        {

        }

        private class Event
        {

        }

        public class Location
        {

        }
        #endregion

        #region Fields
        
        private readonly DatabaseConnection Connection;

        #endregion

        #region Constructor
        DataBaseManager(string filepath)
        {
            if (filepath == null)
            {
                filepath = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Personal),
                    "AGN");

            }
            Connection = new DatabaseConnection(filepath);
        }
        #endregion

        #region CRUD    

        #region Create
        public bool Create(IElement element)
        {
            throw new NotImplementedException();
        }

        public bool Create(List<IElement> elements) 
        { 
            throw new NotImplementedException(); 
        }
        #endregion
            
        #region Read
        public bool Read(IElement element)
        {
            throw new NotImplementedException();
        }

        public bool Read(List<IElement> elements)
        {
            throw new NotImplementedException();
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
    }
}
