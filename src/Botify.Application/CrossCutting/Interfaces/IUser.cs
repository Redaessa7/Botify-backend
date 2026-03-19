using Botify.Application.CrossCutting.Responses;

namespace Botify.Application.CrossCutting.Interfaces;

public interface IUser
{
    string? Id { get; }
    UserInfoResponse UserInfo();
}

