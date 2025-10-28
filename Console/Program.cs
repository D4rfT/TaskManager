using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskManager.Domain.Interfaces;
using TaskManager.Infra.Data.Context;

namespace TaskManager.ConsoleApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7198");
            string token = string.Empty;

            // TELA DE LOGIN/REGISTRO
            bool authenticated = false;
            while (!authenticated)
            {
                Console.Clear();
                Console.WriteLine("=== TASK MANAGER - LOGIN ===");
                Console.WriteLine("1. Fazer Login");
                Console.WriteLine("2. Registrar");
                Console.WriteLine("0. Sair");
                Console.Write("\nEscolha uma opção: ");

                var authOption = Console.ReadLine();

                switch (authOption)
                {
                    case "1":
                        var (success, newToken) = await HandleLoginAsync(httpClient);
                        if (success)
                        {
                            token = newToken;
                            httpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", token);
                            authenticated = true;
                        }
                        break;
                    case "2":
                        await HandleRegisterAsync(httpClient);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida!");
                        Console.ReadKey();
                        break;
                }
            }

            bool continueRunning = true;

            while (continueRunning)
            {
                Console.Clear();
                Console.WriteLine("=== TASK MANAGER ===");
                Console.WriteLine("1. Criar Nova Task");
                Console.WriteLine("2. Listar Todas as Tasks");
                Console.WriteLine("3. Buscar Task por ID");
                Console.WriteLine("4. Listar Tasks Concluídas");
                Console.WriteLine("5. Listar Tasks Pendentes");
                Console.WriteLine("6. Listar Tasks Atrasadas");
                Console.WriteLine("7. Atualizar Task");
                Console.WriteLine("8. Deletar Task");
                Console.WriteLine("9. Logout");
                Console.WriteLine("0. Sair");
                Console.Write("\nEscolha uma opção: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await CreateTaskAsync(httpClient);
                        break;
                    case "2":
                        await ListAllTasksAsync(httpClient);
                        break;
                    case "3":
                        await GetTaskByIdAsync(httpClient);
                        break;
                    case "4":
                        await GetCompletedTasksAsync(httpClient);
                        break;
                    case "5":
                        await GetPendingTasksAsync(httpClient);
                        break;
                    case "6":
                        await GetOverdueTasksAsync(httpClient);
                        break;
                    case "7":
                        await UpdateTaskAsync(httpClient);
                        break;
                    case "8":
                        await DeleteTaskAsync(httpClient);
                        break;
                    case "9":
                        httpClient.DefaultRequestHeaders.Authorization = null;
                        continueRunning = false;
                        break;
                    case "0":
                        continueRunning = false;
                        break;
                    default:
                        Console.WriteLine("Opção inválida!");
                        Console.ReadKey();
                        break;
                }
            }

            Console.WriteLine("Saindo do Task Manager...");
        }

        static async Task CreateTaskAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== CRIAR NOVA TASK ===");

            Console.Write("Título: ");
            var title = Console.ReadLine();

            Console.Write("Descrição: ");
            var description = Console.ReadLine();

            Console.Write("Data de Vencimento (yyyy-mm-dd): ");
            var dateInput = Console.ReadLine();

            try
            {
                if (DateTime.TryParse(dateInput, out DateTime dueDate))
                {
                    var createTaskRequest = new { Title = title, Description = description, DueDate = dueDate };
                    var json = JsonSerializer.Serialize(createTaskRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("/api/tasks", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);

                        if (responseObj.TryGetProperty("id", out var idProperty) &&
                            idProperty.ValueKind == JsonValueKind.Number)
                        {
                            Console.WriteLine($"[OK] Task criada com ID: {idProperty.GetInt32()}");
                        }
                        else
                        {
                            Console.WriteLine("[OK] Task criada com sucesso!");
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ERRO] {response.StatusCode}: {errorContent}");
                    }
                }
                else
                {
                    Console.WriteLine("[ERRO] Data inválida!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task ListAllTasksAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== TODAS AS TASKS ===");

            try
            {
                var response = await httpClient.GetAsync("/api/tasks");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseContent);

                    int taskCount = 0;

                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var taskElement in jsonDocument.RootElement.EnumerateArray())
                        {
                            if (taskElement.TryGetProperty("id", out var idProp) &&
                                taskElement.TryGetProperty("title", out var titleProp) &&
                                taskElement.TryGetProperty("isCompleted", out var completedProp) &&
                                taskElement.TryGetProperty("dueDate", out var dueDateProp))
                            {
                                var id = idProp.GetInt32();
                                var title = titleProp.GetString() ?? "Sem título";
                                var isCompleted = completedProp.GetBoolean();
                                var dueDate = dueDateProp.GetDateTime();

                                var status = isCompleted ? "CONCLUÍDA" : "PENDENTE";
                                var dueDateStr = dueDate.ToString("dd/MM/yyyy");

                                Console.WriteLine($"[{id}] {title} - {status} - Vence: {dueDateStr}");
                                taskCount++;
                            }
                        }

                        if (taskCount == 0)
                        {
                            Console.WriteLine("Nenhuma task encontrada.");
                        }
                        else
                        {
                            Console.WriteLine($"\nTotal: {taskCount} task(s)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Formato de resposta inválido.");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERRO] {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task GetTaskByIdAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== BUSCAR TASK POR ID ===");

            Console.Write("ID da Task: ");
            var idInput = Console.ReadLine();

            try
            {
                if (int.TryParse(idInput, out int id))
                {
                    if (id <= 0)
                    {
                        Console.WriteLine("[ERRO] ID deve ser maior que zero!");
                        Console.ReadKey();
                        return;
                    }

                    var response = await httpClient.GetAsync($"/api/tasks/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var jsonDocument = JsonDocument.Parse(responseContent);
                        var taskElement = jsonDocument.RootElement;

                        if (taskElement.TryGetProperty("id", out var idProp) &&
                            taskElement.TryGetProperty("title", out var titleProp) &&
                            taskElement.TryGetProperty("description", out var descProp) &&
                            taskElement.TryGetProperty("isCompleted", out var completedProp) &&
                            taskElement.TryGetProperty("dueDate", out var dueDateProp) &&
                            taskElement.TryGetProperty("createdAt", out var createdAtProp))
                        {
                            var taskId = idProp.GetInt32();
                            var title = titleProp.GetString() ?? "Sem título";
                            var description = descProp.GetString() ?? "Sem descrição";
                            var isCompleted = completedProp.GetBoolean();
                            var dueDate = dueDateProp.GetDateTime();
                            var createdAt = createdAtProp.GetDateTime();

                            Console.WriteLine($"\n--- DETALHES DA TASK {taskId} ---");
                            Console.WriteLine($"Título: {title}");
                            Console.WriteLine($"Descrição: {description}");
                            Console.WriteLine($"Status: {(isCompleted ? "CONCLUÍDA" : "PENDENTE")}");
                            Console.WriteLine($"Data de Vencimento: {dueDate:dd/MM/yyyy}");
                            Console.WriteLine($"Criada em: {createdAt:dd/MM/yyyy HH:mm}");

                            if (taskElement.TryGetProperty("updatedAt", out var updatedAtProp) &&
                                updatedAtProp.ValueKind != JsonValueKind.Null)
                            {
                                var updatedAt = updatedAtProp.GetDateTime();
                                Console.WriteLine($"Atualizada em: {updatedAt:dd/MM/yyyy HH:mm}");
                            }

                            if (!isCompleted && dueDate < DateTime.UtcNow)
                            {
                                Console.WriteLine("[ATRASADA] Esta task está atrasada!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("[ERRO] Formato de task inválido na resposta.");
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"[ERRO] Task com ID {id} não encontrada.");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ERRO] {response.StatusCode}: {errorContent}");
                    }
                }
                else
                {
                    Console.WriteLine("[ERRO] ID inválido!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task GetCompletedTasksAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== TASKS CONCLUÍDAS ===");

            try
            {
                var response = await httpClient.GetAsync("/api/tasks/completed");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseContent);

                    int taskCount = 0;

                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var taskElement in jsonDocument.RootElement.EnumerateArray())
                        {
                            if (taskElement.TryGetProperty("id", out var idProp) &&
                                taskElement.TryGetProperty("title", out var titleProp) &&
                                taskElement.TryGetProperty("dueDate", out var dueDateProp) &&
                                taskElement.TryGetProperty("updatedAt", out var updatedAtProp))
                            {
                                var id = idProp.GetInt32();
                                var title = titleProp.GetString() ?? "Sem título";
                                var dueDate = dueDateProp.GetDateTime();

                                var dueDateStr = dueDate.ToString("dd/MM/yyyy");
                                var completedDate = "N/A";

                                if (updatedAtProp.ValueKind != JsonValueKind.Null)
                                {
                                    var updatedAt = updatedAtProp.GetDateTime();
                                    completedDate = updatedAt.ToString("dd/MM/yyyy");
                                }

                                Console.WriteLine($"[{id}] {title} - Concluída em: {completedDate} - Vencia: {dueDateStr}");
                                taskCount++;
                            }
                        }

                        if (taskCount == 0)
                        {
                            Console.WriteLine("Nenhuma task concluída encontrada.");
                        }
                        else
                        {
                            Console.WriteLine($"\nTotal: {taskCount} task(s) concluída(s)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Formato de resposta inválido.");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Nenhuma task concluída encontrada.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERRO] {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task GetPendingTasksAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== TASKS PENDENTES ===");

            try
            {
                var response = await httpClient.GetAsync("/api/tasks/pending");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseContent);

                    int taskCount = 0;

                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var taskElement in jsonDocument.RootElement.EnumerateArray())
                        {
                            if (taskElement.TryGetProperty("id", out var idProp) &&
                                taskElement.TryGetProperty("title", out var titleProp) &&
                                taskElement.TryGetProperty("dueDate", out var dueDateProp))
                            {
                                var id = idProp.GetInt32();
                                var title = titleProp.GetString() ?? "Sem título";
                                var dueDate = dueDateProp.GetDateTime();

                                var dueDateStr = dueDate.ToString("dd/MM/yyyy");
                                var status = dueDate < DateTime.UtcNow ? "[ATRASADA] " : "";

                                Console.WriteLine($"{status}[{id}] {title} - Vence em: {dueDateStr}");
                                taskCount++;
                            }
                        }

                        if (taskCount == 0)
                        {
                            Console.WriteLine("Nenhuma task pendente encontrada.");
                        }
                        else
                        {
                            Console.WriteLine($"\nTotal: {taskCount} task(s) pendente(s)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Formato de resposta inválido.");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Nenhuma task pendente encontrada.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERRO] {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task GetOverdueTasksAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== TASKS ATRASADAS ===");

            try
            {
                var response = await httpClient.GetAsync("/api/tasks/overdue");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseContent);

                    int taskCount = 0;

                    if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var taskElement in jsonDocument.RootElement.EnumerateArray())
                        {
                            if (taskElement.TryGetProperty("id", out var idProp) &&
                                taskElement.TryGetProperty("title", out var titleProp) &&
                                taskElement.TryGetProperty("dueDate", out var dueDateProp))
                            {
                                var id = idProp.GetInt32();
                                var title = titleProp.GetString() ?? "Sem título";
                                var dueDate = dueDateProp.GetDateTime();

                                var dueDateStr = dueDate.ToString("dd/MM/yyyy");

                                Console.WriteLine($"[{id}] {title} - Venceu em: {dueDateStr}");
                                taskCount++;
                            }
                        }

                        if (taskCount == 0)
                        {
                            Console.WriteLine("Nenhuma task atrasada encontrada.");
                        }
                        else
                        {
                            Console.WriteLine($"\nTotal: {taskCount} task(s) atrasada(s)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Formato de resposta inválido.");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Nenhuma task atrasada encontrada.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERRO] {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task UpdateTaskAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== ATUALIZAR TASK ===");

            Console.Write("ID da Task para atualizar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("[ERRO] ID inválido!");
                Console.ReadKey();
                return;
            }

            if (id <= 0)
            {
                Console.WriteLine("[ERRO] ID deve ser maior que zero!");
                Console.ReadKey();
                return;
            }

            try
            {
                var getResponse = await httpClient.GetAsync($"/api/tasks/{id}");

                if (!getResponse.IsSuccessStatusCode)
                {
                    if (getResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"[ERRO] Task com ID {id} não encontrada.");
                    }
                    else
                    {
                        var errorContent = await getResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ERRO] {getResponse.StatusCode}: {errorContent}");
                    }
                    Console.ReadKey();
                    return;
                }

                var responseContent = await getResponse.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseContent);
                var taskElement = jsonDocument.RootElement;

                if (taskElement.TryGetProperty("title", out var titleProp) &&
                    taskElement.TryGetProperty("description", out var descProp) &&
                    taskElement.TryGetProperty("dueDate", out var dueDateProp))
                {
                    var currentTitle = titleProp.GetString() ?? "Sem título";
                    var currentDescription = descProp.GetString() ?? "Sem descrição";
                    var currentDueDate = dueDateProp.GetDateTime();

                    Console.WriteLine($"\n--- EDITANDO TASK {id} ---");
                    Console.WriteLine($"Título atual: {currentTitle}");
                    Console.WriteLine($"Descrição atual: {currentDescription}");
                    Console.WriteLine($"Data atual: {currentDueDate:dd/MM/yyyy}");
                    Console.WriteLine("-----------------------------------\n");

                    Console.Write("Novo Título: ");
                    var newTitle = Console.ReadLine();

                    Console.Write("Nova Descrição: ");
                    var newDescription = Console.ReadLine();

                    Console.Write("Nova Data (dd/mm/aaaa): ");
                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime newDueDate))
                    {
                        Console.WriteLine("[ERRO] Data inválida!");
                        Console.ReadKey();
                        return;
                    }

                    var updateTaskRequest = new
                    {
                        Title = newTitle,
                        Description = newDescription,
                        DueDate = newDueDate
                    };

                    var json = JsonSerializer.Serialize(updateTaskRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var putResponse = await httpClient.PutAsync($"/api/tasks/{id}", content);

                    if (putResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[OK] Task atualizada com sucesso!");
                    }
                    else
                    {
                        var errorContent = await putResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ERRO] {putResponse.StatusCode}: {errorContent}");
                    }
                }
                else
                {
                    Console.WriteLine("[ERRO] Não foi possível ler os dados da task.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.ReadKey();
        }

        static async Task DeleteTaskAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== DELETAR TASK ===");

            Console.Write("ID da Task para deletar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("[ERRO] ID inválido!");
                Console.ReadKey();
                return;
            }

            if (id <= 0)
            {
                Console.WriteLine("[ERRO] ID deve ser maior que zero!");
                Console.ReadKey();
                return;
            }

            try
            {
                Console.Write($"\nTem certeza que deseja deletar a task {id}? (s/N): ");
                var confirmation = Console.ReadLine();

                if (!confirmation.Equals("s", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Operação cancelada.");
                    Console.ReadKey();
                    return;
                }

                var response = await httpClient.DeleteAsync($"/api/tasks/{id}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[OK] Task deletada com sucesso!");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"[ERRO] Task com ID {id} não encontrada.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("[ERRO] Você não tem permissão para deletar esta task.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERRO] {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        // DTOs
        public class AuthResponse
        {
            public string Token { get; set; } = string.Empty;
            public UserResponse User { get; set; } = new UserResponse();
        }

        public class UserResponse
        {
            public int Id { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterDto
        {
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        static async Task<(bool success, string token)> HandleLoginAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== LOGIN ===");

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Senha: ");
            var password = Console.ReadLine();

            try
            {
                var loginDto = new LoginDto { Email = email, Password = password };
                var json = JsonSerializer.Serialize(loginDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var token = authResponse.Token;
                    Console.WriteLine($"\n✅ Login realizado com sucesso! Bem-vindo, {authResponse.User.UserName}!");
                    Console.ReadKey();
                    return (true, token);
                }
                else
                {
                    Console.WriteLine("\n❌ Credenciais inválidas!");
                    Console.ReadKey();
                    return (false, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro: {ex.Message}");
                Console.ReadKey();
                return (false, string.Empty);
            }
        }

        static async Task HandleRegisterAsync(HttpClient httpClient)
        {
            Console.Clear();
            Console.WriteLine("=== REGISTRAR USUÁRIO ===");

            Console.Write("Nome de usuário: ");
            var userName = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Senha: ");
            var password = Console.ReadLine();

            try
            {
                var registerDto = new RegisterDto { UserName = userName, Email = email, Password = password };
                var json = JsonSerializer.Serialize(registerDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("\n✅ Usuário registrado com sucesso! Faça login agora.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"\n❌ Erro no registro: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro: {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}