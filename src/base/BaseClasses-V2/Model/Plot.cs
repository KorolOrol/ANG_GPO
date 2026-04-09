using BaseClasses_V2.Enum;
using BaseClasses_V2.Fluent;
using MessagePack;
using ObservableCollections;

namespace BaseClasses_V2.Model
{
    /// <summary>
    /// История
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class Plot : IDisposable
    {
        #region Fields
        
        [Key("elements")]
        private readonly ObservableHashSet<Element> _elements;
        
        [Key("relations")]
        private readonly ObservableHashSet<Relation> _relations;
        
        #endregion

        #region Constructors
        
        public Plot()
        { 
            _elements = [];
            _relations = [];
            SubscribeToEvents();
        }
    
        // Конструктор для десериализации
        [SerializationConstructor]
        public Plot(
            IReadOnlyCollection<Element>? elements,
            IReadOnlyCollection<Relation>? relations,
            int time,
            HashSet<string> relationTypes)
        { 
            _elements = new ObservableHashSet<Element>(elements ?? []);
            _relations = new ObservableHashSet<Relation>(relations ?? []);
            Time = time;
            RelationTypes = relationTypes ?? [];
            SubscribeToEvents();
        }
        
        #endregion

        #region Finalizers
        
        ~Plot()
        {
            Dispose(false);
        }
        
        #endregion

        #region Properties

        public HashSet<string> RelationTypes { get; } = [];
        
        [IgnoreMember]
        public List<Element> Characters => Elements.Where(e => e.Type == ElemType.Character).ToList<Element>();

        /// <summary>
        /// Локации
        /// </summary>
        [IgnoreMember]
        public List<Element> Locations => Elements.Where(e => e.Type == ElemType.Location).ToList<Element>();

        /// <summary>
        /// Предметы
        /// </summary>
        [IgnoreMember]
        public List<Element> Items => Elements.Where(e => e.Type == ElemType.Item).ToList<Element>();

        /// <summary>
        /// События
        /// </summary>
        [IgnoreMember]
        public List<Element> Events => Elements.Where(e => e.Type == ElemType.Event).ToList<Element>();
        
        
        public IReadOnlyCollection<Element> Elements => _elements;
        
        public IReadOnlyCollection<Relation> Relations => _relations;
        
        /// <summary>
        /// Время
        /// </summary>
        public int Time { get; set; } = 0;
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Полная информация об истории
        /// </summary>
        /// <returns>Полная информация об истории</returns>
        public string FullInfo()
        {
            var info = "";
            foreach (var c in Characters)
            {
                info += c.FullInfo() + "\n";
            }
            foreach (var l in Locations)
            {
                info += l.FullInfo() + "\n";
            }
            foreach (var i in Items)
            {
                info += i.FullInfo() + "\n";
            }
            foreach (var e in Events)
            {
                info += e.FullInfo() + "\n";
            }
            return info;
        }
        
        public IEnumerable<Element> QueryElements() => Elements;
        
        public IEnumerable<Element> QueryElements(Func<Element, bool> predicate) 
            => Elements.Where(predicate);

        public IEnumerable<Element> QueryElements(Func<IEnumerable<Element>, IEnumerable<Element>> query) 
            => query(Elements);
        
        public IEnumerable<Relation> QueryRelations() => Relations;
        
        public IEnumerable<Relation> QueryRelations(Func<Relation, bool> predicate) 
            => Relations.Where(predicate);

        public IEnumerable<Relation> QueryRelations(Func<IEnumerable<Relation>, IEnumerable<Relation>> query) 
            => query(Relations);
        
        /// <summary>
        /// Добавление элемента в историю
        /// </summary>
        /// <param name="element">Элемент</param>
        public void Add(Element element)
        {
            if (_elements.Contains(element)) return;
            if (element.Time == -1) element.Time = Time++;
            _elements.Add(element); 
        }

        public void Add(List<Element> elements)
        {
            foreach (var element in elements.Where(element => !_elements.Contains(element)))
            {
                if (element.Time == -1) element.Time = Time++;
                _elements.Add(element);
            }
        }
        
        public void Update(PlotEntity entity)
        {
            if (entity is Element element)
            {
                _elements.Replace(element);
            }
            else if (entity is Relation relation)
            {
                _relations.Replace(relation);
            }
            else
            {
                throw new InvalidOperationException("Invalid entity type");
            }
        }

        public void BulkUpdate(List<PlotEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity is Element element)
                {
                    _elements.Replace(element);
                }
                else if (entity is Relation relation)
                {
                    _relations.Replace(relation);
                }
                else
                {
                    throw new InvalidOperationException("Invalid entity type");
                }
            }
        }

        public void Delete(PlotEntity entity)
        {
            if (entity is Element)
            {
                _relations.RemoveWhere(r => r.Source.Hash == entity.Hash || r.Target.Hash == entity.Hash);
            }
            _elements.RemoveWhere(n => n.Hash == entity.Hash);
            _relations.RemoveWhere(r => r.Hash == entity.Hash);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Bind(Element sourceElement, Element targetElement, string relationName,
            Dictionary<string, object>? @params = null, HashSet<string>? labels = null)
        {
            @params ??= new Dictionary<string, object>();
            labels ??= [];
            
            var newRelation = new Relation(relationName, @params, labels, sourceElement, targetElement);

            var exists = Relations.Any(existingRel =>
                existingRel.Source.Hash == sourceElement.Hash &&
                existingRel.Target.Hash == targetElement.Hash &&
                existingRel.Equals(newRelation));
            
            if (exists) throw new ArgumentException("Relation already exists");
            _relations.Add(newRelation);
        }
        
        #endregion

        #region Protected Methods
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                _elements.Clear();
                _relations.Clear();
            }
            // free native resources if there are any.
        }
        
        #endregion

        #region Private Methods
        
        private void OnRelationCollectionChanged(in NotifyCollectionChangedEventArgs<Relation> e)
        {
            // switch (e.Action)
            // {
            //     case NotifyCollectionChangedAction.Add:
            //         RelationTypes.Add(e.NewItem.RelationType);
            //         break;
            //     case NotifyCollectionChangedAction.Remove:
            //         Console.WriteLine($"Удалена связь: {e.OldItem}");
            //         break;
            //     case NotifyCollectionChangedAction.Replace:
            //         
            // }
        }

        private void SubscribeToEvents()
        {
            _relations.CollectionChanged += OnRelationCollectionChanged;
            // _elements.CollectionChanged += ElementsOnCollectionChanged;
        }

        private void ElementsOnCollectionChanged(in NotifyCollectionChangedEventArgs<Element> e)
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }
}