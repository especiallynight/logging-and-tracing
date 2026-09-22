namespace Task_Management.Clients;

public class ProjectClient
{
    private readonly HttpClient _httpClient;

    public ProjectClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        var response = await _httpClient.GetAsync(
            $"api/projects/{projectId}");

        return response.IsSuccessStatusCode;
    }
}