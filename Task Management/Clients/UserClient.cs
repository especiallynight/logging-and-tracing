namespace TaskManagement.Clients;

public class UserClient
{
    private readonly HttpClient _httpClient;

    public UserClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        var response = await _httpClient.GetAsync(
            $"api/users/{userId}");

        return response.IsSuccessStatusCode;
    }
}