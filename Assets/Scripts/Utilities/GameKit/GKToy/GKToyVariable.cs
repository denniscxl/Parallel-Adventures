using UnityEngine;

namespace GKToy
{
    public abstract class GKToyVariable
    {
        [SerializeField]
        private bool isGlobal;
        public bool IsGlobal
        {
            get;
            set;
        }

        [SerializeField]
        private string name;
        public string Name
        {
            get;
            set;
        }

        [SerializeField]
        private string propertyMapping;
        public string PropertyMapping
        {
            get;
            set;
        }

        [SerializeField]
        private GameObject propertyMappingOwner;
        public GameObject PropertyMappingOwner
        {
            get;
            set;
        }

        public void ValueChanged()
        {
            
        }

        public virtual void InitializePropertyMapping(GKToyBaseOverlord overlord)
        {
            
        }

        public abstract object GetValue();
        public abstract void SetValue(object value);
    }
}
