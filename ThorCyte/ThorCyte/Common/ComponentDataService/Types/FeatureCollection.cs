using System.Collections.Generic;

namespace ComponentDataService.Types
{
    public class FeatureCollection : List<Feature>
    {
        public Feature this[FeatureType type]
        {
            get { return Find(w => w.FeatureType == type); }
        }

        public Feature this[string name]
        {
            get { return Find(w => w.Name == name); }
        }

        public FeatureCollection(int capacity) : base(capacity)
        {
            
        }
       
    }
}