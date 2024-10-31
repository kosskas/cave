using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Experimental.Utils
{
    public class CircularIterator<TItem>
    {
        private readonly IList<TItem> _items;
        private readonly TItem _defaultItem;

        private int _index;

        public TItem Current => _items[_index];

        public bool CurrentIsDefault => Current.Equals(_defaultItem);

        public CircularIterator(IList<TItem> items, TItem defaultItem)
        {
            _defaultItem = defaultItem;

            _items = items;
            _items.Insert(0, defaultItem);

            _index = 0;
        }

        public void Previous()
        {
            _index = (_index == 0) ? _items.Count - 1 : _index - 1;
        }

        public void Next()
        {
            _index = (_index + 1 == _items.Count) ? 0 : _index + 1;
        }

        public void PreviousWhile(Predicate<TItem> notAccepted)
        {
            do
            {
                Previous();

            } while (notAccepted(Current));
        }

        public void NextWhile(Predicate<TItem> notAccepted)
        {
            do
            {
                Next();

            } while (notAccepted(Current));
        }
    }
}
