using System;
using System.Collections;
using System.Collections.Generic;
using Osyacat.Ecs.Entity;
using Osyacat.Ecs.World;

namespace Osyacat.Ecs.EntitasEcs.World
{
    internal sealed class Filter : IFilter, IEnumerator<IEntity>
    {
        private readonly Entitas.IGroup<Entity.Entity> _group;
        private HashSet<Entity.Entity>.Enumerator _enumerator;
        private bool _isEnumerating;

        public Filter(Entitas.IGroup<Entity.Entity> group)
        {
            _group = group;
        }

        public IEnumerator<IEntity> GetEnumerator()
        {
            if (_isEnumerating)
            {
                throw new ObjectDisposedException("Before start new enumeration must to dispose old");
            }
        
            _isEnumerating = true;
            _enumerator = _group.GetEnumerator();
            return this;
        }
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }
        
        void IEnumerator.Reset()
        {
            ((IEnumerator)_enumerator).Reset();
        }
        
        public IEntity Current => _enumerator.Current;
        
        object IEnumerator.Current => Current;
        
        public void Dispose()
        {
            _isEnumerating = false;
        }
        
        public IEntity[] GetEntities()
        {
            return _group.GetEntities();
        }

        public void GetEntities(List<IEntity> buffer)
        {
            buffer.Clear();
            foreach (var entity in this)
            {
                buffer.Add(entity);
            }
        }
    }
}