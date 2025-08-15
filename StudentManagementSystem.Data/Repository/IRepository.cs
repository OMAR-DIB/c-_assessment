using StudentManagementSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagementSystem.Data.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        public List<T> GetAll();
        public List<T> GetManyByFilter(Expression<Func<T,bool>> predicate); // Lambda it check a specific condition
        public T? GetByID(long id);
        public T Create(T entity);
        public T Update(T entity);
        public T? GetOneByFilter(Expression<Func<T, bool>> predicate);
        public bool Any(Expression<Func<T, bool>> predicate);
        public bool Delete(long id);
    }
}
