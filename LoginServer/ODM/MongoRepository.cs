using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;
using LoginServer.ODM;

namespace LoginServer.ODM
{
    public class MongoRepository<T> where T : DBEntity
    {
        /// <summary>
        /// 对应集合的应用
        /// </summary>
        private readonly IMongoCollection<T> _collection;

        public IMongoCollection<T> Collection { get =>_collection; }

        public MongoRepository(MongoDbContext dbContext)
        {
            _collection = dbContext.GetCollection<T>();
        }

        /// <summary>
        /// 增加一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Add(T entity)
        {
            try
            {
                _collection.InsertOne(entity);
                return true;
            }catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 异步增加一条记录
        /// </summary>
        /// <param name="entity">要插入的记录</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> AddAsync(T entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public bool Delete(T entity,Expression<Func<T,bool>> conditions = null)
        {
            try
            {
                string _id = string.Empty;

                if(conditions == null)
                {
                    foreach(PropertyInfo item in entity.GetType().GetProperties())
                    {
                        if(item.Name == "ID" && item.GetValue(entity)!= null)
                        {
                            _id = item.GetValue(entity).ToString();

                            DeleteResult result = _collection.DeleteOne(new BsonDocument("_id", BsonValue.Create(new ObjectId(_id))));

                            return result.IsAcknowledged;
                        }
                    }
                }

                DeleteResult res = _collection.DeleteOne(conditions);
                return res.IsAcknowledged;
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 异步删除一条记录
        /// </summary>
        /// <param name="entity">要删除的记录</param>
        /// <param name="conditions">删除条件（可选）</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeleteAsync(T entity, Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                string _id = string.Empty;

                if (conditions == null)
                {
                    foreach (PropertyInfo item in entity.GetType().GetProperties())
                    {
                        if (item.Name == "ID" && item.GetValue(entity) != null)
                        {
                            _id = item.GetValue(entity).ToString();

                            DeleteResult result = await _collection.DeleteOneAsync(
                                new BsonDocument("_id", BsonValue.Create(new ObjectId(_id))));

                            return result.IsAcknowledged;
                        }
                    }
                }

                DeleteResult res = await _collection.DeleteOneAsync(conditions);
                return res.IsAcknowledged;
            }
            catch (Exception)
            {
                // 可以在此记录日志或执行其他操作
                throw;
            }
        }


        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public bool Update(T entity, Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                ObjectId _id;

                var options = new ReplaceOptions() { IsUpsert = true };

                if (conditions == null)
                {
                    foreach (PropertyInfo item in entity.GetType().GetProperties())
                    {
                        if (item.Name == "ID" && item.GetValue(entity) != null)
                        {
                            _id = new ObjectId(item.GetValue(entity).ToString());

                            ReplaceOneResult result = _collection.ReplaceOne(new BsonDocument("_id", BsonValue.Create(_id)), entity, options);

                            return result.IsAcknowledged;
                        }
                    }
                }

                ReplaceOneResult res = _collection.ReplaceOne(conditions, entity, options);
                return res.IsAcknowledged;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 异步更新一条记录
        /// </summary>
        /// <param name="entity">要更新的记录</param>
        /// <param name="conditions">更新条件（可选）</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> UpdateAsync(T entity, Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                ObjectId _id;

                var options = new ReplaceOptions() { IsUpsert = true };

                if (conditions == null)
                {
                    foreach (PropertyInfo item in entity.GetType().GetProperties())
                    {
                        if (item.Name == "ID" && item.GetValue(entity) != null)
                        {
                            _id = new ObjectId(item.GetValue(entity).ToString());

                            ReplaceOneResult result = await _collection.ReplaceOneAsync(
                                new BsonDocument("_id", BsonValue.Create(_id)), entity, options);

                            return result.IsAcknowledged;
                        }
                    }
                }

                ReplaceOneResult res = await _collection.ReplaceOneAsync(conditions, entity, options);
                return res.IsAcknowledged;
            }
            catch (Exception)
            {
                // 可以在此记录日志或执行其他操作
                throw;
            }
        }

        /// <summary>
        /// 查找记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<T> Find(Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                if(conditions == null)
                {
                    conditions = t => true;
                }
                return _collection.Find(conditions).ToList() ?? new List<T>();
            }catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 异步查找记录
        /// </summary>
        /// <param name="conditions">查找条件（可选）</param>
        /// <returns>查找到的记录列表</returns>
        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                if (conditions == null)
                {
                    conditions = t => true;
                }

                var result = await _collection.FindAsync(conditions);
                return result.ToList() ?? new List<T>();
            }
            catch (Exception)
            {
                // 可以在此记录日志或执行其他操作
                throw;
            }
        }
    }
}
