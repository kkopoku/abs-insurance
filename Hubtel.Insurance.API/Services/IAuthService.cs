namespace Hubtel.Insurance.API.Services;

using Hubtel.Insurance.API.DTOs;


public interface IAuthService {

    Task<ApiResponse<SubscriberResponseDTO>> RegisterAsync(RegisterSubscriberDTO registerUserDTO);
    Task<ApiResponse<string>> LoginAsync(SubscriberDTO subscriberDTO);

}