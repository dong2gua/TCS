using System.Collections.Generic;

namespace ThorCyte.GraphicModule.Models
{
    public class IdManager
    {
        private readonly List<int> _idList = new List<int>();

        #region Methods

        public int GetId()
        {
            var count = _idList.Count;
            var index = _idList.FindIndex(w => w == 0);
            int id;
            if (index >= 0)
            {
                _idList[index] = 1;
                id = index + 1;
            }
            else
            {
                _idList.Add(1);
                id = count + 1;
            }
            return id;
        }

        public void RemoveId(int removeId)
        {
            if (removeId > 0 && removeId <= _idList.Count)
            {
                _idList[removeId - 1] = 0;
            }
        }

        public void InsertId(int id)
        {
            var count = _idList.Count;
            if (id >= count)
            {
                for (var i = count; i < id; i++)
                {
                    var value = 0;
                    if (i == id - 1)
                    {
                        value = 1;
                    }
                    _idList.Add(value);
                }
            }
            else
            {
                var index = id - 1;
                _idList[index] = 1;
            }
        }

        #endregion
    }
}
