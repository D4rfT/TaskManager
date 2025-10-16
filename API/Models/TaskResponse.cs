namespace API.Models
{
    // Para TODOS os GETs - formato padronizado de resposta
    public class TaskResponse
    {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime DueDate { get; set; }
            public bool IsCompleted { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string Status { get; set; } // "pending", "completed", "overdue"

    }
}
