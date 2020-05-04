using System;
using System.Collections.Generic;

namespace SOS.TestHelpers.Helpers.Builders
{
    public abstract class BuilderBase<TBuilder, TItem> where TBuilder : BuilderBase<TBuilder, TItem>
    {
        private readonly List<Action<TItem>> _mutations = new List<Action<TItem>>();

        public static implicit operator TItem(BuilderBase<TBuilder, TItem> builder)
        {
            return builder.Build();
        }

        private TItem MutateItem(TItem item)
        {
            _mutations.ForEach(action => action(item));
            return item;
        }

        protected TBuilder With(Action<TItem> mutation)
        {
            _mutations.Add(mutation);
            return this as TBuilder;
        }

        public virtual TItem Build()
        {
            return MutateItem(CreateEntity());
        }

        protected abstract TItem CreateEntity();

        protected TBuilder ClearMutations()
        {
            _mutations.Clear();
            return this as TBuilder;
        }
    }
}