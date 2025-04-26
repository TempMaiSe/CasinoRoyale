using MediatR;

namespace CasinoRoyale.Api.Application.Commands;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResult> : IRequest<TResult>
{
}