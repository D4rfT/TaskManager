using Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AppServices.Interfaces;
using AppServices.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infra.Data.Context;
using System;

namespace TaskManager.ConsoleApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddDbContext<TaskContext>(options =>
            {
                var dbPath = @"C:\Users\Daniel\source\repos\TaskManager\Infra.Data\bin\Debug\net8.0\TaskManager.db";
                options.UseSqlite($"Data Source={dbPath}");
            });

            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskService, TaskService>();

            var serviceProvider = services.BuildServiceProvider();
            var taskService = serviceProvider.GetRequiredService<ITaskService>();

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
                Console.WriteLine("0. Sair");
                Console.Write("\nEscolha uma opção: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await CreateTaskAsync(taskService);
                        break;
                    case "2":
                        await ListAllTasksAsync(taskService);
                        break;
                    case "3":
                        await GetTaskByIdAsync(taskService);
                        break;
                    case "4":
                        await GetCompletedTasksAsync(taskService);
                        break;
                    case "5":
                        await GetPendingTasksAsync(taskService);
                        break;
                    case "6":
                        await GetOverdueTasksAsync(taskService);
                        break;
                    case "7":
                        await UpdateTaskAsync(taskService);
                        break;
                    case "8":
                        await DeleteTaskAsync(taskService);
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
        static async Task CreateTaskAsync(ITaskService taskService)
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
                Console.WriteLine($"Tentando criar: '{title}', '{description}', '{dateInput}'");

                if (DateTime.TryParse(dateInput, out DateTime dueDate))
                {
                    Console.WriteLine($"Data parseada: {dueDate}");
                    var task = await taskService.CreateTaskAsync(title, description, dueDate);
                    Console.WriteLine($"[OK] Task criada com ID: {task.Id}");
                }
                else
                {
                    Console.WriteLine("[ERRO] Data inválida!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
                Console.WriteLine($"Tipo do erro: {ex.GetType().Name}");

                // Mostrar TODOS os inner exceptions em cascata
                Exception current = ex;
                int level = 0;
                while (current != null)
                {
                    Console.WriteLine($"Erro nível {level}: {current.GetType().Name}: {current.Message}");
                    current = current.InnerException;
                    level++;
                }
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
        /*static async Task CreateTaskAsync(ITaskService taskService)
        {
            Console.Clear();
            Console.WriteLine("=== CRIAR NOVA TASK ===");

            Console.Write("Título: ");
            var title = Console.ReadLine();

            Console.Write("Descrição: ");
            var description = Console.ReadLine();

            Console.Write("Data de Vencimento (aaaa/MM/dd): ");
            var dateInput = Console.ReadLine();

            try
            {
                if (DateTime.TryParse(dateInput, out DateTime dueDate))
                {
                    var task = await taskService.CreateTaskAsync(title, description, dueDate);
                    Console.WriteLine($"[OK] Task criada com ID: {task.Id}");
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
        }*/

        static async Task ListAllTasksAsync(ITaskService taskService)
        {
            Console.Clear();
            Console.WriteLine("=== TODAS AS TASKS ===");

            try
            {
                var tasks = await taskService.GetAllTasksAsync();

                foreach (var task in tasks)
                {
                    var status = task.IsCompleted ? "CONCLUÍDA" : "PENDENTE";
                    var dueDate = task.DueDate.ToString("dd/MM/yyyy");
                    Console.WriteLine($"[{task.Id}] {task.Title} - {status} - Vence: {dueDate}");
                }

                Console.WriteLine($"\nTotal: {tasks.Count()} task(s)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task GetTaskByIdAsync(ITaskService taskService)
        {
            Console.Clear();
            Console.WriteLine("=== BUSCAR TASK POR ID ===");

            Console.Write("ID da Task: ");
            var idInput = Console.ReadLine();

            try
            {
                if (int.TryParse(idInput, out int id))
                {
                    var task = await taskService.GetTaskByIdAsync(id);

                    Console.WriteLine($"\n--- DETALHES DA TASK {task.Id} ---");
                    Console.WriteLine($"Título: {task.Title}");
                    Console.WriteLine($"Descrição: {task.Description}");
                    Console.WriteLine($"Status: {(task.IsCompleted ? "CONCLUÍDA" : "PENDENTE")}");
                    Console.WriteLine($"Data de Vencimento: {task.DueDate:dd/MM/yyyy}");
                    Console.WriteLine($"Criada em: {task.CreatedAt:dd/MM/yyyy HH:mm}");

                    if (task.UpdatedAt.HasValue)
                    {
                        Console.WriteLine($"Atualizada em: {task.UpdatedAt.Value:dd/MM/yyyy HH:mm}");
                    }

                    if (task.IsOverdue())
                    {
                        Console.WriteLine("[ATRASADA] Esta task está atrasada!");
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

        static async Task GetCompletedTasksAsync(ITaskService taskService)
        {
            Console.Clear();
            Console.WriteLine("=== TASKS CONCLUÍDAS ===");

            try
            {
                var tasks = await taskService.GetCompletedTasksAsync();

                foreach (var task in tasks)
                {
                    var dueDate = task.DueDate.ToString("dd/MM/yyyy");
                    var completedDate = task.UpdatedAt?.ToString("dd/MM/yyyy") ?? "N/A";
                    Console.WriteLine($"[{task.Id}] {task.Title} - Concluída em: {completedDate} - Vencia: {dueDate}");
                }
                Console.WriteLine($"\nTotal: {tasks.Count()} task(s) concluída(s)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task GetPendingTasksAsync(ITaskService taskService)
        {
            Console.Clear();
            Console.WriteLine("=== TASKS PENDENTES ===");

            try
            {
                var tasks = await taskService.GetPendingTasksAsync();

                foreach(var task in tasks)
                {
                    var dueDate = task.DueDate.ToString("dd/MM/yyyy");
                    Console.WriteLine($"[{task.Id}] {task.Title} - Vence em: {dueDate}");
                }
                Console.WriteLine($"\nTotal: {tasks.Count()} task(s) pendente(s)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task GetOverdueTasksAsync(ITaskService taskService)
        {
            Console.Clear();
            Console.WriteLine("=== TASKS ATRASADAS ===");

            try
            {
                var tasks = await taskService.GetOverdueTasksAsync();

                foreach(var task in tasks)
                {
                    var dueDate = task.DueDate.ToString("dd/MM/yyyy");
                    Console.WriteLine($"[{task.Id}] {task.Title} - Venceu em: {dueDate}");
                }
                Console.WriteLine($"\nTotal: {tasks.Count()} task(s) atrasada(s)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        static async Task UpdateTaskAsync(ITaskService taskService)
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

            try
            {
                var existingTask = await taskService.GetTaskByIdAsync(id);

                Console.WriteLine($"\n--- EDITANDO TASK {existingTask.Id} ---");
                Console.WriteLine($"Título atual: {existingTask.Title}");
                Console.WriteLine($"Descrição atual: {existingTask.Description}");
                Console.WriteLine($"Data atual: {existingTask.DueDate:dd/MM/yyyy}");
                Console.WriteLine("-----------------------------------\n");

                Console.Write("Novo Título: ");
                var title = Console.ReadLine();

                Console.Write("Nova Descrição: ");
                var description = Console.ReadLine();

                Console.Write("Nova Data (dd/mm/aaaa): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime dueDate))
                {
                    Console.WriteLine("[ERRO] Data inválida!");
                    Console.ReadKey();
                    return;
                }
    
                await taskService.UpdateTaskAsync(id, title, description, dueDate);
                Console.WriteLine("[OK] Task atualizada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.ReadKey();
        }

        

        static async Task DeleteTaskAsync(ITaskService taskService)
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

            try
            {
                await taskService.DeleteTaskAsync(id);
                Console.WriteLine("[OK] Task deletada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}