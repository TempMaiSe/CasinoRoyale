using MediatR;

namespace CasinoRoyale.Api.Application.Queries;

public interface IQuery<out TResult> : IRequest<TResult>
{
}