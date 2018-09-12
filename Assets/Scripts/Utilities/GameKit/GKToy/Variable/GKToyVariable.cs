using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public abstract class GKToyVariable
    {

        [SerializeField]
        private bool isGlobal;
        public bool IsGlobal
        {
            get { return isGlobal;}
            set { isGlobal = value; }
        }

        [SerializeField]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [SerializeField]
        private string propertyMapping;
        public string PropertyMapping
        {
            get { return propertyMapping; }
            set { propertyMapping = value; }
        }

        [SerializeField]
        private GKToyBaseOverlord propertyMappingOwner;
        public GKToyBaseOverlord PropertyMappingOwner
        {
            get { return propertyMappingOwner; }
            set { propertyMappingOwner = value; }
        }

        public void ValueChanged()
        {
            if(null != PropertyMappingOwner)
            {
                PropertyMappingOwner.data.variableChanged = true;
            }
        }

        public virtual void InitializePropertyMapping(GKToyBaseOverlord overlord)
        {
            PropertyMappingOwner = overlord;
            PropertyMapping = GetType().ToString();
        }

        public abstract object GetValue();
        public abstract void SetValue(object value);
    }
}
