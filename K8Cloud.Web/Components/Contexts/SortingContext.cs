using MudBlazor;

namespace K8Cloud.Web.Components.Contexts;

public sealed class SortingContext
{
    public sealed class Item
    {
        private readonly SortingContext _manager;
        private SortDirection _direction;

        public Item(SortingContext manager, SortDirection direction = SortDirection.None)
        {
            _manager = manager;
            _manager.Add(this);

            _direction = direction;
        }

        public SortDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                _manager.OnItemUpdated(this);
            }
        }

        public void SetToGraphQL(Action<SortEnumType> updateAction)
        {
            switch (_direction)
            {
                case SortDirection.Ascending:
                    updateAction(SortEnumType.Asc);
                    break;
                case SortDirection.Descending:
                    updateAction(SortEnumType.Desc);
                    break;
                default:
                    break;
            }
        }
    }

    private readonly List<Item> _items = new();
    private bool _suspendUpdates;

    private void Add(Item item)
    {
        _items.Add(item);
    }

    private void OnItemUpdated(Item item)
    {
        if (_suspendUpdates)
        {
            return;
        }

        _suspendUpdates = true;
        foreach (var otherItem in _items.Where(x => x != item))
        {
            if (otherItem.Direction != SortDirection.None)
            {
                otherItem.Direction = SortDirection.None;
            }
        }
        _suspendUpdates = false;
    }
}
