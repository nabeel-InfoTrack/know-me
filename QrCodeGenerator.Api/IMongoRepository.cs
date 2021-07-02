using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QrCodeGenerator.Api
{
    public interface IMongoRepository
    {
        Task<List<T>> FindAllAsync<T>(string collection, CancellationToken cancellationToken = default);
        Task<T> FindByIdAsync<T>(Guid id, string collection, CancellationToken cancellationToken = default);
        Task InsertAsync(string collection, string serialisedObj, Guid guid);
        Task InsertAsync(string collection, string serialisedObj);
        Task InsertAsync<T>(string collection, T value, int id) where T : class;

        Task ReplaceAsync(string collection, string serialisedObj, int id);
        Task ReplaceAsync<T>(string collection, T value, int id)
            where T : class;

        Task<T> FindRandom<T>(string collection, CancellationToken cancellationToken = default);
    }
}